using Graduation_Project.Api.DTO.Account;
using Graduation_Project.Api.ErrorHandling;
using Graduation_Project.Api.Helpers;
using Graduation_Project.Core;
using Graduation_Project.Core.Constants;
using Graduation_Project.Core.DTOs;
using Graduation_Project.Core.IRepositories;
using Graduation_Project.Core.IServices;
using Graduation_Project.Core.Models.Doctors;
using Graduation_Project.Core.Models.SendingEmail;
using Graduation_Project.Core.Specifications.AccountSpecs;
using Graduation_Project.Core.Specifications.DoctorSpecifications;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;
using System.Numerics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Talabat.API.Dtos.Account;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace Graduation_Project.Api.Controllers
{
    #region Using Directives
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IAuthService _authServices;
        private readonly IGenericRepository<Doctor> _doctorRepo;
        private readonly IGenericRepository<Clinic> _clinicRepo;

        private readonly IGenericRepository<Patient> _patientRepo;
        private readonly IGenericRepository<Specialty> _specialtyRepo;
        private readonly ILogger<AccountController> _logger;
        private readonly IUserService _userService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public AccountController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IAuthService authServices,
            IGenericRepository<Doctor> doctorRepo,
            IGenericRepository<Clinic> clinicRepo,

            IGenericRepository<Patient> patientRepo,
            IGenericRepository<Specialty> specialtyRepo,
            ILogger<AccountController> logger,
            IUserService userService,
            IFileUploadService fileUploadService,
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            IEmailService emailService


            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authServices = authServices;
            _logger = logger;
            _userService = userService;
            _fileUploadService = fileUploadService;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _doctorRepo = doctorRepo;
            _clinicRepo = clinicRepo;

            _patientRepo = patientRepo;
            _specialtyRepo = specialtyRepo;
        }

        #endregion


        #region Login
        /************************** Login ***************************/
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(LoginDTO model)
        {
            // 1. Check if email exists
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "Invalid Email"));


            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);

            if (!result.Succeeded)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized));

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            // Get the current business user (Doctor or Patient)
            var BusinessUser = await _userService.GetCurrentBusinessUserAsync(user.Id,
                (UserRoleType)Enum.Parse(typeof(UserRoleType), role));

            // Generate JWT Token
            var token = await _authServices.CreateTokenAsync(user, _userManager);

            // Generate or Retrieve Active Refresh Token
            var refreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
            if (refreshToken == null)
            {
                refreshToken = TokenHelper.GenerateRefreshToken();
                user.RefreshTokens.Add(refreshToken);
                await _userManager.UpdateAsync(user);
            }

            // Set refresh token in cookie
            SetRefreshTokenInCookie(refreshToken.Token, refreshToken.ExpiresOn);

            // Handle Doctor
            if (BusinessUser is Doctor doctor)
            {
                var doctorDto = new DoctorDTO()
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    Token = token,
                    Role = role,
                    Speciality = doctor.Specialty.Name_en,
                    Description = doctor.Description,
                    PictureUrl = !(string.IsNullOrEmpty(doctor.PictureUrl)) ? $"{_configuration["AzureStorageUrl"]}/{doctor.PictureUrl}" : string.Empty,
                    RefreshToken = refreshToken.Token,
                    RefreshTokenExpiration = refreshToken.ExpiresOn,
                    IsAuthenticated = true
                };
                return Ok(doctorDto);
            }
            // Handle Patient
            else if (BusinessUser is Patient patient)
            {
                var patientDto = new PatientDTO()
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    Token = token,
                    Role = role,
                    PictureUrl = !(string.IsNullOrEmpty(patient.PictureUrl)) ? $"{_configuration["AzureStorageUrl"]}/{patient.PictureUrl}" : string.Empty,
                    BloodType = patient.BloodType,
                    Points = patient.Points,
                    Age = patient.DateOfBirth != null ? DateTime.Now.Year - patient.DateOfBirth.Value.Year : null,
                    RefreshToken = refreshToken.Token,
                    RefreshTokenExpiration = refreshToken.ExpiresOn,
                    IsAuthenticated = true
                };
                return Ok(patientDto);
            }

            return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest));
        }
        #endregion

        #region Doctor Register
        /************************** Doctor Register ***************************/
        [HttpPost("DoctorRegister")] // post: api/account/DoctorRegister
        public async Task<ActionResult<UserDTO>> DoctorRegister( DoctorRegisterDTO model)
        {

            if (CheckEmailExists(model.Email).Result.Value)
                return BadRequest(new ApiValidationErrorResponse() {Errors = new string[] { "This Email is Already Exist" } });

            if (string.IsNullOrWhiteSpace(model.FullName) || model.FullName.Split(" ").Length != 2)
                return BadRequest(new ApiResponse(400, "Full Name must include first and last name."));

            var user = new AppUser()
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email.Split("@")[0],
            };

            var result = await _userManager.CreateAsync(user, model.Password);

           await _userManager.AddToRoleAsync(user, UserRoleType.Doctor.ToString());
           
            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400, "Failed to create user."));

            // Fetch registered user
            var registeredUser = await _userManager.FindByEmailAsync(model.Email);

            if (registeredUser == null)
                return BadRequest(new ApiResponse(400, "User registration failed."));

            var OTP = await GenerateAndSaveOtp(registeredUser , OtpType.EmailVerification); 

            var email = new Email()
            {
                Subject = "Your OTP Code for Email Verification",
                Recipients = model.Email,
                Body = EmailTemplateService.GetOtpEmailBody(user.Email, OTP)
            };
            var reuslt = await _emailService.SendEmailAsync(email);
            if (!reuslt)
                return StatusCode(500, new ApiResponse(500, "Failed to send new OTP code"));

            // split the full name into first and last name
            var nameParts = model.FullName?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];


           //var specialty = await _specialtyRepo.GetAsync(model.SpecialtyId);
           var specialty = await _unitOfWork.Repository<Specialty>().GetAsync(model.SpecialtyId);
            
            if(specialty is  null)
            {
                // Rollback: If doctor creation fails, delete the user from identity DB
                await _userManager.DeleteAsync(registeredUser);
                return BadRequest(new ApiResponse(400, "please enter valid specialty"));
            }


            // upload MedicalLicensePictureUrl

            var sanitizedFileName = Regex.Replace(registeredUser.FullName, @"[^a-zA-Z0-9_-]", ""); // Remove special chars
            var finalFileName = $"{sanitizedFileName}-{registeredUser.Id}";
            if(model.ImageFile is  null)
            {
                // Rollback: If doctor creation fails, delete the user from identity DB
                await _userManager.DeleteAsync(registeredUser);
                return BadRequest(new ApiResponse(400, "medical License is required"));
            }

            var (uploadSuccess, uploadMessage, medicalLicensePictureUrlFilePath) =  await _fileUploadService.UploadFileAsync(model.ImageFile! ,
                                                                                    "Doctor/License/" , 
                                                                                    User,
                                                                                    customFileName: finalFileName);

            if(!uploadSuccess)
            {
                // Rollback: If doctor creation fails, delete the user from identity DB
                await _userManager.DeleteAsync(registeredUser);
                return BadRequest(new ApiResponse(400, uploadMessage));
            }


            var newDoctor = new Doctor()
            {
                FirstName = nameParts.Length > 0 ? nameParts[0] : string.Empty,
                LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : string.Empty,
                ApplicationUserId = registeredUser.Id,
                ConsultationFees = model.ConsultationFees,
                Gender = model.Gender,
                SpecialtyId = model.SpecialtyId,
                Specialty = await _specialtyRepo.GetAsync(model.SpecialtyId),
                SlotDurationMinutes = 20,
                MedicalLicensePictureUrl = medicalLicensePictureUrlFilePath
            };

            try
            {
                // Add Doctor to the application database
              //var regDoctor =  await _doctorRepo.AddWithSaveAsync(newDoctor);
              var regDoctor = await _unitOfWork.Repository<Doctor>().AddWithSaveAsync(newDoctor);

                var newClinic = new Clinic()
                {
                    Name = model.ClinicName,
                    RegionId = model.RegionId,
                    DoctorId = regDoctor.Id
                };

                await _clinicRepo.AddWithSaveAsync(newClinic);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error while creating doctor for user: {Email}", model.Email);

                // Rollback: If doctor creation fails, delete the user from identity DB
                await _userManager.DeleteAsync(registeredUser);

                return BadRequest(new ApiResponse(500, "An unexpected error occurred."));

            }

            _logger.LogInformation("Doctor registered successfully: {Email}", model.Email);

            return Ok(new UserDTO()
            {
                FullName = user.FullName,
                Email = user.Email,
                //Token = await _authServices.CreateTokenAsync(user, _userManager),
                Role = UserRoleType.Doctor.ToString()
            });
        }
        #endregion

        #region Patient Register
        /************************** Patient Register ***************************/
        [HttpPost("PatientRegister")] // post: api/account/PatientRegister
        public async Task<ActionResult<UserDTO>> PatientRegister(PatientRegisterDTO model)
        {
            if (CheckEmailExists(model.Email).Result.Value)
                return BadRequest(new ApiValidationErrorResponse() { Errors = new string[] { "This Email is Already Exist" } });

            if (string.IsNullOrWhiteSpace(model.FullName) || model.FullName.Split(' ').Length != 2)
                return BadRequest(new ApiResponse(500, "you must enter first name and last name."));

            var user = new AppUser()
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email.Split('@')[0],
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400, "Failed to create user."));


            // Fetch registered user
            var registeredPatient = await _userManager.FindByEmailAsync(model.Email);

            var OTP = await GenerateAndSaveOtp(registeredPatient ,OtpType.EmailVerification);

            var email = new Email()
            {
                Subject = "Your OTP Code for Email Verification",
                Recipients = model.Email,
                Body = EmailTemplateService.GetOtpEmailBody(user.Email, OTP)
            };
            var reuslt = await _emailService.SendEmailAsync(email);
            if (!reuslt)
                return StatusCode(500, new ApiResponse(500, "Failed to send new OTP code"));

            await _userManager.AddToRoleAsync(user, UserRoleType.Patient.ToString());

            if (registeredPatient == null)
                return BadRequest(new ApiResponse(400, "User registration failed."));

            // split the full name into first and last name
            var nameParts = model.FullName?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

            var newPatient = new Patient()
            {
                FirstName = nameParts.Length > 0 ? nameParts[0] : string.Empty,
                LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : string.Empty,
                Gender = model.Gender,
                Points = 0,
                ApplicationUserId = registeredPatient.Id
            };

            try
            {
                await _unitOfWork.Repository<Patient>().AddWithSaveAsync(newPatient);
                await _unitOfWork.CompleteAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error while creating patient for user: {Email}", model.Email);


                // Rollback: If doctor creation fails, delete the user from identity DB
                await _userManager.DeleteAsync(registeredPatient);

                return BadRequest(new ApiResponse(500, "An unexpected error occurred."));
            }

            _logger.LogInformation("Patient registered successfully: {Email}", model.Email);

            return Ok(new UserDTO
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    //Token = await _authServices.CreateTokenAsync(user, _userManager),
                    Role = UserRoleType.Patient.ToString()
                });
        }

        #endregion

        #region Patient Register
        /************************** Patient Register ***************************/
        [Authorize(Roles = nameof(UserRoleType.Doctor))]
        [HttpPost("SecretaryRegister")] // post: api/account/PatientRegister
        public async Task<ActionResult<UserDTO>> SecretaryRegister(SecretaryRegisterDto model)
        {
            if (CheckEmailExists(model.Email).Result.Value)
                return BadRequest(new ApiValidationErrorResponse() { Errors = new string[] { "This Email is Already Exist" } });

            if (string.IsNullOrWhiteSpace(model.FullName) || model.FullName.Split(' ').Length != 2)
                return BadRequest(new ApiResponse(500, "you must enter first name and last name."));

            var user = new AppUser()
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email.Split('@')[0],
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(new ApiResponse(400, "Failed to create user."));


            //// Fetch registered user
            var registeredPatient = await _userManager.FindByEmailAsync(model.Email);

            //var OTP = await GenerateAndSaveOtp(registeredPatient, OtpType.EmailVerification);

            //var email = new Email()
            //{
            //    Subject = "Your OTP Code for Email Verification",
            //    Recipients = model.Email,
            //    Body = EmailTemplateService.GetOtpEmailBody(user.Email, OTP)
            //};
            //var reuslt = await _emailService.SendEmailAsync(email);
            //if (!reuslt)
            //    return StatusCode(500, new ApiResponse(500, "Failed to send new OTP code"));

            await _userManager.AddToRoleAsync(user, UserRoleType.Secretary.ToString());

            if (registeredPatient == null)
                return BadRequest(new ApiResponse(400, "User registration failed."));

            // split the full name into first and last name
            var nameParts = model.FullName?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];

            var newSecretary = new Secretary()
            {
                FirstName = nameParts.Length > 0 ? nameParts[0] : string.Empty,
                LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : string.Empty,
                Gender = model.Gender,
                ApplicationUserId = registeredPatient.Id
            };


            // Add Secretary Id to Current Doctor

            int currentDoctorId = int.Parse(User.FindFirstValue(Identifiers.DoctorId));
            var doctorWithClinicSpec = new DoctorWithClinicSpecification(currentDoctorId);
            var doctor = await _unitOfWork.Repository<Doctor>().GetWithSpecsAsync(doctorWithClinicSpec);

            //var SecetaryUser = await _userManager.FindByEmailAsync(model.Email);
            //var Appid = SecetaryUser.Id; // from identity table
            //var secretary = await _unitOfWork.Repository<Secretary>().GetByConditionAsync(s => s.ApplicationUserId == Appid);
            //secretary.clinicId = doctor.Clinic.Id;
            newSecretary.clinicId = doctor.Clinic.Id;

            try
            {
                await _unitOfWork.Repository<Secretary>().AddWithSaveAsync(newSecretary);
                await _unitOfWork.CompleteAsync();

                //_unitOfWork.Repository<Doctor>().Update(doctor);
                //await _unitOfWork.Repository<Doctor>().SaveAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error while creating patient for user: {Email}", model.Email);


                // Rollback: If doctor creation fails, delete the user from identity DB
                await _userManager.DeleteAsync(registeredPatient);

                return BadRequest(new ApiResponse(500, "An unexpected error occurred."));
            }




            _logger.LogInformation("Patient registered successfully: {Email}", model.Email);

            return Ok(new UserDTO
            {
                FullName = model.FullName,
                Email = model.Email,
                Role = UserRoleType.Secretary.ToString()
            });
        }

        #endregion

        #region Get Current User
        /******************************** Get Current User ********************************/
        [Authorize]
        [HttpGet] // GET: api/Account/GetCurrentUser
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email.IsNullOrEmpty())
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "User is unauthorized"));

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "User is unauthorized"));

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            //var role = User.FindFirstValue(ClaimTypes.Role);

            return Ok(new UserDTO
            {
                FullName = user.FullName,
                Email = user.Email,
                Role = role
            });
        }

        #endregion

        #region Save User Device Token
        /******************************** Save Device Token ********************************/
        [Authorize]
        [HttpPost("Save-User-DeviceToken")]
        public async Task<ActionResult> SaveDeviceToken([FromBody] string deviceToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            user.DeviceToken = deviceToken;
            await _userManager.UpdateAsync(user);

            return Ok();
        }

        #endregion

        #region check if user is Exist
        /******************************** Check Email Exists ********************************/
        [HttpGet("EmailExists")] // GET: api/Account/EmailExists
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            return await _userManager.FindByEmailAsync(email) is not null;
        }

        #endregion

        #region Logout
        /******************************** Logout ********************************/
        [HttpPost("logout")] // POST: api/Account/logout
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { message = "No refresh token found" });

            var result = await _authServices.RevokeTokenAsync(refreshToken);
            if (!result)
                return BadRequest(new { message = "Failed to revoke token" });

            Response.Cookies.Delete("refreshToken");
            await _signInManager.SignOutAsync();

            return Ok(new { message = "Logged out successfully" });
        }

        #endregion

        #region Delete User
        /******************************** Delete User ********************************/
        [Authorize]
        [HttpDelete("delete-user")]
        public async Task<ActionResult> DeleteUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email is null)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "User is unauthorized"));

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "User is unauthorized"));

            var role = User.FindFirstValue(ClaimTypes.Role);

            switch (role)
            {
                case nameof(UserRoleType.Doctor):
                    var doctor = await _unitOfWork.Repository<Doctor>().GetByConditionAsync(d => d.ApplicationUserId == user.Id);
                    if (doctor is not null)
                    {
                        var clinic = await _unitOfWork.Repository<Clinic>().GetByConditionAsync(c => c.DoctorId == doctor.Id);
                        if (clinic is not null)
                        {
                            _unitOfWork.Repository<Clinic>().Delete(clinic);
                            await _unitOfWork.CompleteAsync();
                        }
                        _unitOfWork.Repository<Doctor>().Delete(doctor);
                        await _unitOfWork.CompleteAsync();
                    }
                    break;

                case nameof(UserRoleType.Patient):
                    var patient = await _unitOfWork.Repository<Patient>().GetByConditionAsync(p => p.ApplicationUserId == user.Id);
                    if (patient is not null)
                    {
                        _unitOfWork.Repository<Patient>().Delete(patient);
                        await _unitOfWork.CompleteAsync();
                    }
                    break;
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Failed to delete user"));

            return Ok(new ApiResponse(StatusCodes.Status200OK, "User deleted successfully"));
        }

        #endregion

        #region Get Refresh token
        /******************************** Refresh Token ********************************/
        [HttpGet("RefreshToken")] // GET: api/Account/refreshToken
        public async Task<IActionResult> RefreshTokenAsync()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Refresh token is missing"));

            var result = await _authServices.RefreshTokenAsync(refreshToken);

            if (!result.IsAuthenticated)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, result.Message));

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }

        #endregion

        #region Revoke Token
        /******************************** Revoke Token ********************************/
        [HttpPost("RevokeToken")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest model)
        {
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Token is required"));

            var result = await _authServices.RevokeTokenAsync(token);

            if (!result)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Token is invalid"));

            await _signInManager.SignOutAsync();

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Token revoked"));
        }

        #endregion

        #region Verify OTP
        /******************************** Verify OTP ********************************/
        [HttpPost("VerifyOTP")]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTP model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "Invalid Email"));

            // Retrieve the OTP record for the user
            var otpRecord = await _unitOfWork.Repository<UserOtpVerifications>()
                .GetByConditionAsync(o => o.ApplicationUserId == user.Id &&
                                     o.OtpType == model.OtpType &&
                                     o.OtpCode == model.OtpCode &&
                                     !o.IsVerified &&
                                     o.ExpiresOn > DateTime.UtcNow);

            if (otpRecord is null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid OTP or OTP has expired."));

            otpRecord.IsVerified = true;
            _unitOfWork.Repository<UserOtpVerifications>().Update(otpRecord);
            await _unitOfWork.CompleteAsync();

            if (model.OtpType == OtpType.EmailVerification)
            {
                // If OTP is for email verification, update the user's email confirmation status
                user.EmailConfirmed = true;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Failed to verify OTP"));

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Email verified successfully"));
            }

            return Ok(new { Message = "OTP verified successfully." });

        }

        #endregion

        #region Resend OTP
        /******************************** Resend OTP ********************************/
        [EnableRateLimiting("OtpRateLimit")]
        [HttpPost("ResendOTP")]
        public async Task<IActionResult> ResendOTP([FromBody] ResendOTP model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "Invalid Email"));

            if (user.EmailConfirmed && model.OtpType == OtpType.EmailVerification)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Email is already verified."));

            var spec = new UserOtpVerificationsSpec(user.Id, model.OtpType);
            var latestOtp = await _unitOfWork.Repository<UserOtpVerifications>().GetWithSpecsAsync(spec);

            if (latestOtp != null && latestOtp.IsVerified == false && latestOtp.ExpiresOn > DateTime.Now)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "You have already requested a new OTP. Please check your email."));

            var OTP = await GenerateAndSaveOtp(user, model.OtpType);

            var email = new Email()
            {
                Subject = "Here’s Your New OTP Code",
                Recipients = model.Email,
                Body = EmailTemplateService.GetResendOtpEmailBody(user.Email, OTP)
            };
            var reuslt = await _emailService.SendEmailAsync(email);
            if (!reuslt)
                return StatusCode(500, new ApiResponse(500, "Failed to send new OTP code"));

            return Ok(new ApiResponse(StatusCodes.Status200OK, "New OTP sent successfully"));
        }

        #endregion

        #region Forgot Password
        /******************************** Forgot Password ********************************/
        [EnableRateLimiting("PasswordLimiter")]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Email not found"));

            // check if the user already has a non-expired, non-verified OTP of type PasswordReset
            var otpRecord = await _unitOfWork.Repository<UserOtpVerifications>()
                .GetByConditionAsync(o => o.ApplicationUserId == user.Id &&
                                     o.OtpType == OtpType.PasswordReset &&
                                     !o.IsVerified &&
                                     o.ExpiresOn > DateTime.UtcNow);

            if (otpRecord != null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "You have already requested a password reset. Please check your email for the OTP code."));

            var OTP = await GenerateAndSaveOtp(user, OtpType.PasswordReset);

            var email = new Email()
            {
                Subject = "Password Reset Code (OTP)",
                Recipients = request.Email,
                Body = EmailTemplateService.GetForgotPasswordOtpBody(user.Email, OTP)
            };
            var reuslt = await _emailService.SendEmailAsync(email);
            if (!reuslt)
                return StatusCode(500, new ApiResponse(500, "Failed to send new OTP code"));

            return Ok(new ApiResponse(StatusCodes.Status200OK, "OTP sent successfully"));
        }

        #endregion

        #region Reset Password
        /******************************** Reset Password ********************************/
        [EnableRateLimiting("PasswordLimiter")]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Email not found"));

            // Validate OTP
            var spec = new UserOtpVerificationsSpec(user.Id, OtpType.PasswordReset);
            var latestOtp = await _unitOfWork.Repository<UserOtpVerifications>().GetWithSpecsAsync(spec);

            if (latestOtp is null || !latestOtp.IsVerified || latestOtp.ExpiresOn < DateTime.UtcNow)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid OTP or OTP has expired."));

            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, request.Password);
            await _userManager.UpdateAsync(user);

            // Ensures one-time use and Avoid replay attacks
            latestOtp.IsVerified = false;
            _unitOfWork.Repository<UserOtpVerifications>().Update(latestOtp);
            await _unitOfWork.CompleteAsync();

            return Ok(new { message = "Password reset successfully" });
        }

        #endregion

        #region Change Password
        /******************************** Change Password ********************************/
        [Authorize]
        [EnableRateLimiting("PasswordLimiter")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordDto request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return Unauthorized(new ApiResponse(StatusCodes.Status401Unauthorized, "User is unauthorized"));


            var isPasswordValid = await _signInManager.CheckPasswordSignInAsync(user, request.OldPassword, true);
            if (!isPasswordValid.Succeeded)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid Password"));

            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

            if(!result.Succeeded)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Failed to change password"));

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Password changed successfully"));
        }

        #endregion

        #region Private Methods
        /******************************** Private Method ********************************/
        private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
        {
            var cookiOption = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires.ToLocalTime(),
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookiOption);
        }

        private async Task<string> GenerateAndSaveOtp(AppUser user , OtpType OtpType)
        {
            var OTP = new Random().Next(100000, 999999).ToString();
            var userOTP = new UserOtpVerifications()
            {
                OtpCode = OTP,
                OtpType = OtpType,
                ExpiresOn = DateTime.Now.AddMinutes(5),
                IsVerified = false,
                ApplicationUserId = user.Id
            };
            await _unitOfWork.Repository<UserOtpVerifications>().AddWithSaveAsync(userOTP);

            return OTP;
        }

        #endregion
    }
}
