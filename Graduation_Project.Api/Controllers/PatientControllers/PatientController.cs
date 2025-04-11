﻿using System.Security.Claims;
using AutoMapper;
using Graduation_Project.Api.DTO.FeedBacks;
using Graduation_Project.Api.DTO.Patients;
using Graduation_Project.Api.ErrorHandling;
using Graduation_Project.Core;
using Graduation_Project.Core.Constants;
using Graduation_Project.Core.IRepositories;
using Graduation_Project.Core.IServices;
using Graduation_Project.Core.Models.Doctors;
using Graduation_Project.Core.Models.Shared;
using Graduation_Project.Core.Specifications.DoctorSpecifications;
using Graduation_Project.Core.Specifications.PatientSpecifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_Project.Api.Controllers.PatientControllers
{
    public class PatientController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileUploadService _fileUploadService;
        private readonly IPatientService _patientService;
        private readonly IConfiguration _configuration;

        public PatientController(UserManager<AppUser> userManager,
                                IMapper mapper,
                                IUnitOfWork unitOfWork , 
                                IFileUploadService fileUploadService ,
                                IPatientService patientService , IConfiguration configuration)
        {
            _patientService = patientService;
            _configuration = configuration;
            _userManager = userManager;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _fileUploadService = fileUploadService;
        }

        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpGet("GetProfile")]
        public async Task<ActionResult<object>> GetProfile()
        {

            int patientId = int.Parse(User.FindFirstValue(Identifiers.PatientId));
            string? email = User.FindFirstValue(ClaimTypes.Email);

            if(string.IsNullOrEmpty(email))
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Email Not Found"));

            var patient = await _patientService.GetInfo(patientId, email);
            if(patient is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound));

            return Ok(patient);
        }

        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpPut("EditProfile")]
        public async Task<ActionResult<PatientForProfileToReturnDto>> EditProfile(PatientForProfileDto patientProfileFromRequest)
        {
            // Get current user id
            var patientId = int.Parse(User.FindFirstValue(Identifiers.PatientId));

            // Get current patient from business DB
            var patient = await _unitOfWork.Repository<Patient>().GetAsync(patientId);
            if (patient == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound));

            // mapping for Patient 
            patient = _mapper.Map(patientProfileFromRequest, patient);


            // upload Paitent Profile Picture

            var uploadedPictureUrl = await _fileUploadService.UploadFileAsync(patientProfileFromRequest.PictureFile, "Patient/ProfilePicture", User);

            patient.PictureUrl = uploadedPictureUrl;
            
            // Update Patient 
            _unitOfWork.Repository<Patient>().Update(patient);
            await _unitOfWork.Repository<Patient>().SaveAsync();

            var data = _mapper.Map<Patient, PatientForProfileToReturnDto>(patient);
            data.Email = User.FindFirstValue(ClaimTypes.Email) ?? "";

            return Ok(data);
        }

        /****************************************** Medicl Category ******************************************/
        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpGet("GetMedicalCategory")]
        public async Task<ActionResult<IReadOnlyList<MedicalCategoryDto>>> GetAllMediclCategory([FromQuery] string? lang = "ar")
        {
            if (lang.ToLower() != "ar" && lang.ToLower() != "en")
            {
                return BadRequest(new ApiResponse(400, "Invalid Language"));
            }

            var spec = new MedicalCategorySpecification();
            var MediclaCategories = await _unitOfWork.Repository<MedicalCategory>().GetAllWithSpecAsync(spec,mc => new MedicalCategoryDto
            {
                Id = mc.Id,
                Name = lang.ToLower() == "ar" ? mc.Name_ar : mc.Name_en,
            });

            if (MediclaCategories is null)
            {
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Medical Categories Not Found"));
            }

            return Ok(MediclaCategories);
        }

        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpGet("{Id:int}")]
        public async Task<ActionResult<MedicalCategoryDto>> GetMediclCategory(int Id)
        {
            var MedicalCategory = await _unitOfWork.Repository<MedicalCategory>().GetAsync(Id);
            if(MedicalCategory is null)
            {
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Medical Category Not Found"));
            }
            return Ok(_mapper.Map<MedicalCategory, MedicalCategoryDto>(MedicalCategory));
        }

        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpGet("Points")]
        public async Task<ActionResult<int>> GetPatientPoints()
        {
            var patientId = int.Parse(User.FindFirstValue(Identifiers.PatientId));
            var patient = await _unitOfWork.Repository<Patient>().GetAsync(patientId);
            var points = patient.Points;

            if (points == null)
                return NoContent();

            return Ok(points);
        }
    }
}
