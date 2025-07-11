using AutoMapper;
using Graduation_Project.Api.DTO.Patients;
using Graduation_Project.Api.ErrorHandling;
using Graduation_Project.Core;
using Graduation_Project.Core.Constants;
using Graduation_Project.Core.IRepositories;
using Graduation_Project.Core.IServices;
using Graduation_Project.Core.Models.Doctors;
using Graduation_Project.Core.Models.Patients;
using Graduation_Project.Core.Specifications.PatientSpecifications;
using Graduation_Project.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Numerics;
using System.Security.Claims;

namespace Graduation_Project.Api.Controllers.PatientControllers
{
    public class MedicalHistoryController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserService _userService;
        private readonly IFileUploadService _fileUploadService;
        private readonly IPatientService _patientService;
        private readonly INotificationService _notificationService;

        public MedicalHistoryController(IUnitOfWork unitOfWork , IMapper mapper , UserManager<AppUser> userManager 
                                        , IUserService userService ,IFileUploadService fileUploadService,IPatientService patientService
                                        , INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _userService = userService;
            _fileUploadService = fileUploadService;
            _patientService = patientService;
            _notificationService = notificationService;
        }

        /****************************************** Get All Medical History By Category id for Current User ******************************************/
        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpGet("GetUserHistoryByCategory/{medicalCategoryId:int}")]
        public async Task<ActionResult<IReadOnlyList<MedicalHistoryDto>>> GetUserHistoryByCategory(int medicalCategoryId)
        {
            var patientId = int.Parse(User.FindFirstValue(Identifiers.PatientId));

            var patient = await _unitOfWork.Repository<Patient>().GetByConditionAsync(p => p.Id == patientId);  

            if (patient is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "This Patient Not Found"));

            var spec = new MedicalHistoryWithMedicalImage(patientId, medicalCategoryId);
            var medicalHistories = await _unitOfWork.Repository<MedicalHistory>().GetAllWithSpecAsync(spec);

            if (medicalHistories is null || !medicalHistories.Any())
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Medical Histories Not Found"));

            var medicalHistoriesDto = _mapper.Map<IReadOnlyList<MedicalHistory>, IReadOnlyList<MedicalHistoryDto>>(medicalHistories);

            return Ok(medicalHistoriesDto);
        }
        /****************************************** Get All Medical History of specific patient by patient id and category id for the doctor ******************************************/
        [Authorize(Roles = nameof(UserRoleType.Doctor))]
        [HttpGet("GetPatientHistoryByCategory")]
        public async Task<ActionResult<IReadOnlyList<MedicalHistoryDto>>> GetPatientHistoryByCategory([FromQuery]int patientId,[FromQuery] int medicalCategoryId)
        {
            var patient = await _unitOfWork.Repository<Patient>().GetByConditionAsync(p => p.Id == patientId);

            if (patient is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "This Patient Not Found"));

            var spec = new MedicalHistoryWithMedicalImage(patientId, medicalCategoryId);
            var medicalHistories = await _unitOfWork.Repository<MedicalHistory>().GetAllWithSpecAsync(spec);

            if (medicalHistories is null || !medicalHistories.Any())
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Medical Histories Not Found"));

            var medicalHistoriesDto = _mapper.Map<IReadOnlyList<MedicalHistory>, IReadOnlyList<MedicalHistoryDto>>(medicalHistories);

            return Ok(medicalHistoriesDto);
        }


        /****************************************** Add Medicl History ******************************************/
        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpPost("MedicalHistory")]
        public async Task<ActionResult<MedicalHistoryFormDto>> AddMedicalHistory(MedicalHistoryFormDto model)
        {
            var patientId = int.Parse(User.FindFirstValue(Identifiers.PatientId));

            var patient = await _unitOfWork.Repository<Patient>().GetByConditionAsync(p => p.Id == patientId);

            if (patient is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "This Patient Not Found"));

            var medicalHistory = _mapper.Map<MedicalHistoryFormDto, MedicalHistory>(model);
            medicalHistory.PatientId = patient.Id;

            if (model.PictureFile is not null)
            {
                var (uploadSuccess, uploadMessage, uploadedPictureUrlFilePath) = await _fileUploadService.UploadFileAsync(model.PictureFile, "Patient/MedicalHistory", User);

                if (!uploadSuccess)
                    return BadRequest(new ApiResponse(400, uploadMessage));
                
                medicalHistory.medicalHistoryImage = new MedicalHistoryImage
                {
                    ImageUrl = uploadedPictureUrlFilePath,
                    MedicalHistoryId = medicalHistory.Id
                };
            }

            try
            {
                await _unitOfWork.Repository<MedicalHistory>().AddAsync(medicalHistory);

                var result = await _unitOfWork.CompleteAsync();
                if (result <= 0)
                {
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Failed to add Medical History"));
                }
                // Update Points
                await _patientService.UpdatePoints(patientId, Points.MedicalHistoryAdded);
                // push notifications
                await _notificationService.SendNotificationAsync(patient.ApplicationUserId, $"New {Points.CompletedAppointment} Points", "New Points");
                return Ok(_mapper.Map<MedicalHistory, MedicalHistoryFormDto>(medicalHistory));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

        /****************************************** Edit Medicl History ******************************************/
        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpPut("MedicalHistory/{Id:int}")]
        public async Task<ActionResult<MedicalHistoryDto>> EditMedicalHistory(MedicalHistoryFormDto model,int Id)
        {
            var medicalHistory = await _unitOfWork.Repository<MedicalHistory>().GetAsync(Id);
            if (medicalHistory == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Medical History Not Found"));

            var medicalHistoryMapped = _mapper.Map(model, medicalHistory);

            if (model.PictureFile is not null)
            {
                var (uploadSuccess, uploadMessage, uploadedPictureUrlFilePath) = await _fileUploadService.UploadFileAsync(model.PictureFile, "Patient/MedicalHistory", User);

                if (!uploadSuccess)
                    return BadRequest(new ApiResponse(400, uploadMessage));

                var medicalHistoryImage = await _unitOfWork.Repository<MedicalHistoryImage>().GetByConditionAsync(m => m.MedicalHistoryId == medicalHistory.Id);
                if (medicalHistoryImage != null)
                {
                    medicalHistoryImage.ImageUrl = uploadedPictureUrlFilePath;
                    _unitOfWork.Repository<MedicalHistoryImage>().Update(medicalHistoryImage);
                }
                else
                {
                    var newImage = new MedicalHistoryImage
                    {
                        ImageUrl = uploadedPictureUrlFilePath,
                        MedicalHistoryId = medicalHistory.Id
                    };
                    await _unitOfWork.Repository<MedicalHistoryImage>().AddAsync(newImage);
                }
            }

            try
            {
                _unitOfWork.Repository<MedicalHistory>().Update(medicalHistoryMapped);
                var result = await _unitOfWork.CompleteAsync();
                if (result <= 0)
                {
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Failed to update Medical History"));
                }
                return Ok(_mapper.Map<MedicalHistory, MedicalHistoryFormDto>(medicalHistory));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }


        /****************************************** Delete Medical History  ******************************************/
        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpDelete("MedicalHistory/{Id:int}")]
        public async Task<ActionResult> DeleteMedicalHistory(int Id)
        {
            var specs = new MedicalHistorySpecification(Id);
            var medicalHistory = await _unitOfWork.Repository<MedicalHistory>().GetWithSpecsAsync(specs);
            if (medicalHistory is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Medical History Not Found"));

            try
            {
                _unitOfWork.Repository<MedicalHistory>().Delete(medicalHistory);
                var result = await _unitOfWork.CompleteAsync();
                if (result <= 0)
                {
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Failed to delete Medical History"));
                }
                // Update Points
                await _patientService.UpdatePoints(medicalHistory.PatientId, Points.MedicalHistoryAdded);
                // push notifications
                await _notificationService.SendNotificationAsync(medicalHistory.Patient.ApplicationUserId, $"{Points.CompletedAppointment} Points Removed", "Removed Points");

                return Ok(new ApiResponse(StatusCodes.Status200OK, "Medical History Deleted Successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, ex.Message));
            }
        }

    }
}
