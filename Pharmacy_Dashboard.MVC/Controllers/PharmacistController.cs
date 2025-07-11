using AutoMapper;
using Graduation_Project.Core;
using Graduation_Project.Core.Constants;
using Graduation_Project.Core.Enums;
using Graduation_Project.Core.IServices;
using Graduation_Project.Core.Models.Identity;
using Graduation_Project.Core.Models.Pharmacies;
using Graduation_Project.Core.Models.SendingEmail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Pharmacy_Dashboard.MVC.ViewModel.Account;
using System.Security.Claims;


namespace Pharmacy_Dashboard.MVC.Controllers
{
    public class PharmacistController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IFileUploadService _fileUploadService;
        private readonly SignInManager<AppUser> _signInManager;

        public PharmacistController(IUnitOfWork unitOfWork , UserManager<AppUser> userManager , IMapper mapper , IFileUploadService fileUploadService , SignInManager<AppUser> signInManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _fileUploadService = fileUploadService;
            this._signInManager = signInManager;
        }

        /****************************************** Edit Profile Info ******************************************/
        [Authorize(Roles = nameof(UserRoleType.Pharmacist))]
        public async Task<IActionResult> EditProfile()
        {
            var pharmacistId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(pharmacistId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch pharmacist details from the database using pharmacistId
            var pharmacy = await _unitOfWork.Repository<Pharmacy>().GetByConditionAsync(p => p.ApplicationUserID == pharmacistId);
            var pharmacyContacts = await _unitOfWork.Repository<PharmacyContact>().GetManyByConditionAsync(p => p.PharmacyId == pharmacy.Id);


            var model = _mapper.Map<Pharmacy, EditProfileViewModel>(pharmacy);
            model.Email = User.FindFirstValue(ClaimTypes.Email);

            return View(model);
        }

        [Authorize(Roles = nameof(UserRoleType.Pharmacist))]
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var pharmacistId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Fetch the existing pharmacy from the database
                var pharmacy = await _unitOfWork.Repository<Pharmacy>().GetByConditionAsync(p => p.ApplicationUserID == pharmacistId);
                if (pharmacy == null)
                {
                    ModelState.AddModelError(string.Empty, "Pharmacy not found.");
                    return View(model);
                }

                // upload Pharmacy Picture and save its relative path in database
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {

                    var (uploadSuccess, uploadMessage, uploadedPicUrlFilePath) = await _fileUploadService.UploadFileAsync(model.ImageFile, "Pharmacy/ProfilePic", User);
                    if (!uploadSuccess)
                    {
                        ModelState.AddModelError(string.Empty, uploadMessage);
                        return View(model);
                    }

                    model.PictureUrl = uploadedPicUrlFilePath;
                }
                else
                {
                    model.PictureUrl = pharmacy.ProfilePictureUrl;
                }

                var pharmacyToUpdate = _mapper.Map(model, pharmacy);
                
                /******** handle if user remove contact numbers ******/

                // 1- get all existing contacts
                var existingContacts = await _unitOfWork.Repository<PharmacyContact>().GetManyByConditionAsync(c => c.PharmacyId == pharmacy.Id);
                if(existingContacts != null && existingContacts.Any())
                {
                    // 2- put all contacts in a list that comes from the view
                    var contactsFromView = model.PharmacyContacts?.Select(c => c.Id).ToList() ?? new List<int>();

                    // 3- remove all contacts that are not in the list
                    foreach (var contact in existingContacts)
                    {
                        if (!contactsFromView.Contains(contact.Id))
                        {
                            _unitOfWork.Repository<PharmacyContact>().Delete(contact);
                        }
                    }
                }

                // Update pharmacy contacts
                if (model.PharmacyContacts != null)
                {
                    foreach (var contact in model.PharmacyContacts)
                    {
                        var existingContact = await _unitOfWork.Repository<PharmacyContact>().GetAsync(contact.Id);
                        if (existingContact != null)
                        {
                            existingContact.PhoneNumber = contact.PhoneNumber;
                            _unitOfWork.Repository<PharmacyContact>().Update(existingContact);
                        }
                        else
                        {
                            // If the contact doesn't exist, create a new one
                            var newContact = new PharmacyContact
                            {
                                PhoneNumber = contact.PhoneNumber,
                                PharmacyId = pharmacy.Id
                            };
                            await _unitOfWork.Repository<PharmacyContact>().AddAsync(newContact);
                        }
                    }
                }

                // Save changes
                _unitOfWork.Repository<Pharmacy>().Update(pharmacy);
                var UpdateResult = await _unitOfWork.CompleteAsync();
                if (UpdateResult <= 0)
                {
                    ModelState.AddModelError(string.Empty, "Failed to update profile. Please try again.");
                    return View(model);
                }
                
                TempData["ProfileSaved"] = "Profile updated successfully!";
                return RedirectToAction("EditProfile");
            }
            return View(model);
        }
    }
}
