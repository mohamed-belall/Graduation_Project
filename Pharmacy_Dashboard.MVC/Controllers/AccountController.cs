using AutoMapper;
using Graduation_Project.Core;
using Graduation_Project.Core.Enums;
using Graduation_Project.Core.IServices;
using Graduation_Project.Core.Models.Identity;
using Graduation_Project.Core.Models.Pharmacies;
using Graduation_Project.Core.Models.SendingEmail;
using Graduation_Project.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pharmacy_Dashboard.MVC.Helpers;
using Pharmacy_Dashboard.MVC.ViewModel.Account;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Graduation_Project.Core.Constants;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Pharmacy_Dashboard.MVC.Controllers
{
    public class AccountController : Controller
    {
        #region Allow Dependancy Injection
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IAuthService _authServices;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly IFileUploadService _fileUploadService;

        public AccountController(
            UserManager<AppUser> userManager ,
            SignInManager<AppUser> signInManager ,
            IAuthService authServices,
            IUnitOfWork unitOfWork ,
            IMapper mapper ,
            IUserService userService ,
            IEmailService emailService ,
            IFileUploadService fileUploadService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            this._authServices = authServices;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
            _emailService = emailService;
            _fileUploadService = fileUploadService;
        }
        #endregion

        #region SignUp
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(RegisterViewModel model)
        {
            if(ModelState.IsValid) 
            { 
                var user = await _userService.UserExistsAsync(model.Email);
                if(user is null)
                {
                    // Create the user of type AppUser
                    var newUser = new AppUser
                    {
                        UserName = model.Email.Split("@")[0],
                        Email = model.Email,
                        FullName = model.FullName,
                    };
                    var result = await _userManager.CreateAsync(newUser, model.Password);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                    }

                    // Add the user to the "Pharmacy" role
                    await _userManager.AddToRoleAsync(newUser, UserRoleType.Pharmacist.ToString());

                    // Validate and upload the LicenseImage
                    if(model.ImageFile is not null && model.ImageFile.Length > 0)
                    {
                        var sanitizedFileName = Regex.Replace(newUser.FullName, @"[^a-zA-Z0-9_-]", ""); // Remove special chars
                        var finalFileName = $"{sanitizedFileName}-{newUser.Id}";
                        var (uploadSuccess, uploadMessage, licenseImageFilePath) = await _fileUploadService.UploadFileAsync(
                            model.ImageFile,
                            "Pharmacy/License/",
                            User,
                            customFileName: finalFileName
                        );

                        if (!uploadSuccess)
                        {
                            await _userManager.DeleteAsync(newUser);
                            ModelState.AddModelError(string.Empty, uploadMessage);
                            return View(model);
                        }

                        model.LicenseImageUrl = licenseImageFilePath;
                    }
                    else
                    {
                        await _userManager.DeleteAsync(newUser);
                        ModelState.AddModelError(string.Empty, "Please upload a valid license image file.");
                        return View(model);
                    }

                    // Create the pharmacy
                    var pharmacy = _mapper.Map<Pharmacy>(model);
                    pharmacy.ApplicationUserID = newUser.Id;

                    // Add the pharmacy to the database
                    await _unitOfWork.Repository<Pharmacy>().AddAsync(pharmacy);
                    var checkResult = await _unitOfWork.CompleteAsync();
                    if(checkResult <= 0)
                    {
                        // Delete the user if pharmacy creation fails
                        await _userManager.DeleteAsync(newUser);

                        ModelState.AddModelError(string.Empty, "Failed to create pharmacy");

                        return View(model);
                    }

                    // check user entered the pharmacy contact or not
                    if (model.PharmacyContact is not null)
                    {
                        model.PharmacyContact.PharmacyId = pharmacy.Id;
                        await _unitOfWork.Repository<PharmacyContact>().AddAsync(model.PharmacyContact);
                        int resultCheck = await _unitOfWork.CompleteAsync();
                        if (resultCheck <= 0)
                        {
                            await DeleteUserAndPharmacyAsync(newUser, pharmacy); // private Method

                            ModelState.AddModelError(string.Empty, "Failed to create pharmacy contact");

                            return View(model);
                        }
                    }

                    // confirm the email
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                    var confirmEmailLink = Url.Action(nameof(ConfirmEmail), "Account", new { email = newUser.Email, token = token }, Request.Scheme);

                    var email = new Email()
                    {
                        Subject = "Confirm Your Email",
                        Recipients = newUser.Email,
                        Body = EmailTemplateService.GetEmailConfirmationBody(newUser.UserName,confirmEmailLink)
                    };

                    var flag = await _emailService.SendEmailAsync(email);
                    if(flag)
                    {
                        TempData["Message"] = "We've sent a confirmation link to your email address. Please check your inbox and follow the instructions to activate your account.";
                        TempData["ResendAction"] = "SignUp";
                        return RedirectToAction(nameof(CheckYourBox));
                    }
                    else
                    {
                       // Delete the user if email sending fails
                        await DeleteUserAndPharmacyAsync(newUser, pharmacy); // private Method
                        ModelState.AddModelError(string.Empty, "Failed to send confirmation email");
                        return View(model);
                    }
                }
                ModelState.AddModelError(string.Empty, "Email already exists");
            }
            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Invalid confirmation link.";
                return View("ConfirmEmailFailed");
            }

            var user = await _userService.UserExistsAsync(email);
            if(user is null)
            {
                ViewBag.Error = "User not found.";
                return View("ConfirmEmailFailed");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if(result.Succeeded)
            {
                TempData["Success"] = "Your email has been confirmed. You can now log in.";
                return RedirectToAction(nameof(Login));
            }
            
            ViewBag.Error = "Email confirmation failed. The token may be invalid or expired.";
            return View("ConfirmEmailFailed");
        }
        #endregion

        #region Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // check this email is exist or not 
                var user = await _userService.UserExistsAsync(model.Email);
                if (user is not null)
                {
                    if (user.EmailConfirmed is false)
                    {
                        ModelState.AddModelError(string.Empty, "You need to confirm your email to log in.");
                        return View(model);
                    }

                    // check password
                    var flag = await _userManager.CheckPasswordAsync(user, model.Password);
                    if (flag)
                    {
                        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
                        if (result.Succeeded)
                        {
                            var token = await _authServices.CreateTokenAsync(user, _userManager);
                            HttpContext.Session.SetString("JWTToken", token);

                            return RedirectToAction("Index", "Dashboard");
                        }
                        if (result.IsLockedOut)
                        {
                            ModelState.AddModelError(string.Empty, "Your account is locked out. Please try again later.");
                            return View(model);
                        }
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid Username or Password");
            }
            return View(model);
        }
        #endregion

        #region SignOut
        public async new Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
        #endregion

        #region Forget Password

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [EnableRateLimiting("PasswordLimiter")]
        [HttpPost]
        public async Task<IActionResult> SendResetLink(ForgotPasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await _userService.UserExistsAsync(model.Email);
                if (user is not null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user); // unique for this user once time
                    var passwordResetLink = Url.Action(nameof(ResetPassword), "Account", new { email = model.Email , token = token } , Request.Scheme);

                    var email = new Email()
                    {
                        Subject = "Reset Your Password",
                        Recipients = model.Email,
                        Body = EmailTemplateService.GetResetPasswordEmailBody(passwordResetLink)
                    };

                    var flag = await _emailService.SendEmailAsync(email);
                    if(flag)
                    {
                        TempData["Message"] = "We've sent a reset password link to your email address. Please check your inbox and follow the instructions.";
                        TempData["ResendAction"] = "ForgetPassword";
                        return RedirectToAction(nameof(CheckYourBox));
                    }

                    ModelState.AddModelError(string.Empty, "Failed to sending Email");
                    return View(model);
                }
                ModelState.AddModelError(string.Empty, "This Email not exist");
            }
            return View(nameof(ForgetPassword),model);
        }

        public IActionResult CheckYourBox()
        {
            return View();
        }
        #endregion

        #region Reset Password
        
        public IActionResult ResetPassword(string email , string token)
        {
            TempData["Email"] = email;
            TempData["Token"] = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                var email = TempData["Email"] as string;
                var token = TempData["Token"] as string;

                var user = await _userService.UserExistsAsync(email);
                if(user is not null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                    if(result.Succeeded)
                    {
                        TempData["Success"] = "Your password has been reset successfully. You can now log in.";
                        return RedirectToAction(nameof(Login));
                    }
                }
                ModelState.AddModelError(string.Empty, "Invalid reset password");
            }
            return View(model);
        }

        #endregion

        #region Change Password
        [Authorize(Roles = nameof(UserRoleType.Pharmacist))]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize(Roles = nameof(UserRoleType.Pharmacist))]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await _userService.UserExistsAsync(User.FindFirstValue(ClaimTypes.Email));
                if(user is not null)
                {
                    var isPasswordValid = await _signInManager.CheckPasswordSignInAsync(user, model.CurrentPassword, true);
                    if (!isPasswordValid.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "Current password is incorrect.");
                        return View(model);
                    }

                    var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                    if(result.Succeeded)
                    {
                        TempData["Success"] = "Your password has been changed successfully.";
                        return RedirectToAction(nameof(ChangePassword));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                        return View(model);
                    }
                }
                ModelState.AddModelError(string.Empty, "User not found");
            }
            return View(model);
        }
        #endregion

        /************************** Private Method **************************/
        #region private methods
        public async Task DeleteUserAndPharmacyAsync(AppUser user, Pharmacy pharmacy)
        {
            await _userManager.DeleteAsync(user);
            _unitOfWork.Repository<Pharmacy>().Delete(pharmacy);
            await _unitOfWork.CompleteAsync();
        }
        #endregion

    }
}
