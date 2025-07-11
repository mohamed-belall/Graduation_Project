using AutoMapper;
using Graduation_Project.Api.DTO.Pharmacies;
using Graduation_Project.Api.DTO.Shared;
using Graduation_Project.Api.ErrorHandling;
using Graduation_Project.Api.Filters;
using Graduation_Project.Core;
using Graduation_Project.Core.Constants;
using Graduation_Project.Core.IRepositories;
using Graduation_Project.Core.IServices;
using Graduation_Project.Core.Models.Patients;
using Graduation_Project.Core.Models.Shared;
using Graduation_Project.Core.Specifications.DoctorSpecifications;
using Graduation_Project.Core.Specifications.MedicineSpecifications;
using Graduation_Project.Core.Specifications.PatientSpecifications;
using Graduation_Project.Core.Specifications.PrescriptionSpecifications;
using Graduation_Project.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.Security.Claims;
using Talabat.API.Dtos.Account;


namespace Graduation_Project.Api.Controllers.Shared
{
    public class PrescriptionController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly IPatientService _patientService;

        public PrescriptionController(IUnitOfWork unitOfWork, IMapper mapper
                                      ,INotificationService notificationService, IUserService userService
                                      ,IPatientService patientService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _userService = userService;
            _patientService = patientService;
        }

        [Authorize(Roles = nameof(UserRoleType.Doctor))]
        [HttpPost("DoctorAdd")]
        public async Task<ActionResult<PrescriptionResponseDTO>> AddPrescription(PrescriptionFromUserDto prescriptionFromUser)
        {
            var DoctorId = int.Parse(User.FindFirstValue(Identifiers.DoctorId));
            var doctor = await _unitOfWork.Repository<Doctor>().GetAsync(DoctorId);

            var prescription = new Prescription
            {
                PatientId = prescriptionFromUser.PatientId,
                Diagnoses = prescriptionFromUser.Diagnoses
            };
            prescription.IssuedDate = DateTime.Now;
            prescription.DoctorId = DoctorId;

            try
            {
                var createdPrescription = await _unitOfWork.Repository<Prescription>().AddWithSaveAsync(prescription);

                if (!prescriptionFromUser.MedicinePrescriptions.IsNullOrEmpty())
                {
                    var medicinePrescriptions = prescriptionFromUser.MedicinePrescriptions.Select(medicinePrescription => new MedicinePrescription
                    {
                        PrescriptionId = createdPrescription.Id,
                        MedicineId = medicinePrescription.MedicineId,
                        Details = medicinePrescription.Details
                    }).ToList();

                    await _unitOfWork.Repository<MedicinePrescription>().AddRangeAsync(medicinePrescriptions);
                    var result = await _unitOfWork.CompleteAsync();
                    if (result <= 0)
                        return BadRequest(new ApiResponse(400, "Failed to add medicine prescriptions."));
                }

                if(!prescriptionFromUser.PrescriptionImages.IsNullOrEmpty())
                {
                    var prescriptionImages = prescriptionFromUser.PrescriptionImages.Select(
                        prescriptionImage => new PrescriptionImage()
                        {
                            PrescriptionId = createdPrescription.Id,
                            Name = prescriptionImage.Name,
                            ImageUrl = prescriptionImage.ImageUrl
                        }).ToList();

                    await _unitOfWork.Repository<PrescriptionImage>().AddRangeAsync(prescriptionImages);
                    var result = await _unitOfWork.CompleteAsync();
                    if(result <= 0)
                        return BadRequest(new ApiResponse(400, "Failed to add prescription images."));
                }

                var electornicPrescriptionResponse = _mapper.Map<Prescription, PrescriptionResponseDTO>(createdPrescription);
                // Update Points
                await _patientService.UpdatePoints(prescriptionFromUser.PatientId, Points.CompletedAppointment);

                // push notifications
                await _notificationService.SendNotificationAsync(createdPrescription.Patient.ApplicationUserId,
                                                                 $"New Prescription From Doctor {doctor.FirstName + " " + doctor.LastName}",
                                                                 "New Prescription");

                await _notificationService.SendNotificationAsync(createdPrescription.Patient.ApplicationUserId, $"New {Points.CompletedAppointment} Points", "New Points");

                return Ok(electornicPrescriptionResponse);
            }
            catch(Exception ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message));
            }

        }

        [Authorize(Roles = nameof(UserRoleType.Doctor))]
        [HttpPut("DoctorEdit/{id:int}")]
        [ServiceFilter(typeof(ExistingIdFilter<Prescription>))]
        public async Task<ActionResult<PrescriptionEditFormDto>> EditPrescription(PrescriptionEditFormDto updateDto, int id)
        {
            var spec = new PrescriptionWithMedicinePrescriptionsAndImageSpec(id);
            var prescriptionFromDB = await _unitOfWork.Repository<Prescription>().GetWithSpecsAsync(spec);
            if(prescriptionFromDB is null)
                return NotFound(new ApiResponse(404, "Prescription not found."));

            //check if the doctor who wrote it is the same person who is editing
            var DoctorId = int.Parse(User.FindFirstValue(Identifiers.DoctorId));
            if(prescriptionFromDB.DoctorId != DoctorId)
                return Unauthorized(new ApiResponse(401, "This Doctor is not Authorized to Edit this Prescription."));

            var currentDoctor = await _unitOfWork.Repository<Doctor>().GetAsync(DoctorId);
            var currentPatient = await _unitOfWork.Repository<Patient>().GetAsync(prescriptionFromDB.PatientId);

            if ((DateTime.UtcNow - prescriptionFromDB.IssuedDate).TotalMinutes > 15)
            {
                return BadRequest(new ApiResponse(400, "You cannot edit this prescription after 15 minutes of creation."));
            }

            //prescriptionFromUser.PatientId = prescriptionFromDB.Id;
            prescriptionFromDB.Diagnoses = updateDto.Diagnoses;
            
            var existingMedicinePrescriptions = prescriptionFromDB.MedicinePrescriptions?.ToList();

            if (existingMedicinePrescriptions.Any())
            {
                var newMedicinePrescriptions = updateDto.MedicinePrescriptions
                    .Where(mp => !existingMedicinePrescriptions.Any(emp => emp.MedicineId == mp.MedicineId))
                    .Select(mp => new MedicinePrescription
                    {
                        PrescriptionId = id,
                        MedicineId = mp.MedicineId,
                        Details = mp.Details
                    }).ToList();
                var medicineToBeRemoved = existingMedicinePrescriptions
                    .Where(emp => !updateDto.MedicinePrescriptions.Any(mp => mp.MedicineId == emp.MedicineId))
                    .ToList();

                if (medicineToBeRemoved.Any())
                {
                    _unitOfWork.Repository<MedicinePrescription>().DeleteRange(medicineToBeRemoved);
                }

                if (newMedicinePrescriptions.Any())
                {
                    await _unitOfWork.Repository<MedicinePrescription>().AddRangeAsync(newMedicinePrescriptions);
                }
            }

            try
            {
                _unitOfWork.Repository<Prescription>().Update(prescriptionFromDB);
                
                var hasChanges = _unitOfWork.HasChanges();
                var result = await _unitOfWork.CompleteAsync();
                
                if (hasChanges && result == 0)
                    return BadRequest(new ApiResponse(400, "Failed to update prescription."));
                
                var electornicPrescriptionResponse = _mapper.Map<Prescription, PrescriptionResponseDTO>(prescriptionFromDB);
                return Ok(electornicPrescriptionResponse);
            }
            catch(Exception ex)
            {
                return BadRequest(new ApiResponse(400, ex.Message));
            }

        }


        [Authorize(Roles = nameof(UserRoleType.Doctor))]
        [HttpDelete("DoctorDelete/{id:int}")]
        [ServiceFilter(typeof(ExistingIdFilter<Prescription>))]
        public async Task<ActionResult> DeletePrescription(int id)
        {
            var spec = new PrescriptionWithMedicinePrescriptionsAndImageSpec(id);
            var prescriptionFromDB = await _unitOfWork.Repository<Prescription>().GetWithSpecsAsync(spec);
            if(prescriptionFromDB is null)
                return NotFound(new ApiResponse(404, "Prescription not found."));

            var DoctorId = int.Parse(User.FindFirstValue(Identifiers.DoctorId));
            if (prescriptionFromDB.DoctorId != DoctorId)
                return Unauthorized(new ApiResponse(401, "This Doctor is not Authorized to Delete this Prescription."));
            

            if ((DateTime.UtcNow - prescriptionFromDB.IssuedDate).TotalMinutes > 15)
                return BadRequest(new ApiResponse(400, "You cannot delete this prescription after 15 minutes of creation."));
            

            if (prescriptionFromDB.MedicinePrescriptions.Any())
            {
                _unitOfWork.Repository<MedicinePrescription>().DeleteRange(prescriptionFromDB.MedicinePrescriptions);
            }
            _unitOfWork.Repository<Prescription>().Delete(prescriptionFromDB);

            var result = await _unitOfWork.CompleteAsync();
            if(result <= 0)
                return BadRequest(new ApiResponse(400, "Failed to delete prescription."));

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Prescription deleted Successfully"));
        }


        [Authorize(Roles = $"{nameof(UserRoleType.Doctor)},{nameof(UserRoleType.Patient)}")]
        [HttpGet("GetById/{id:int}")]
        public async Task<ActionResult<Prescription>> GetPrescriptionById(int id)
        {
            var spec = new PrescriptionWithMedicinePrescriptionsAndImageSpec(id);
            var prescriptionFromDB = await _unitOfWork.Repository<Prescription>().GetWithSpecsAsync(spec);
            if(prescriptionFromDB is null)
                return NotFound(new ApiResponse(404, "Prescription not found."));

            return Ok(_mapper.Map<Prescription, PrescriptionResponseDTO>(prescriptionFromDB));
        }


        
        
        [Authorize(Roles = nameof(UserRoleType.Doctor))]
        [HttpGet("GetAllForDoctorsPatient/{id:int}")]
        [ServiceFilter(typeof(ExistingIdFilter<Patient>))]
        public async Task<ActionResult<IReadOnlyList<PrescriptionListViewFormDto>>> GetAllPrescriptionForPatient(int id)
        {
            var doctorId = int.Parse(User.FindFirstValue(Identifiers.DoctorId));
            var spec = new AllPrescriptionsForPatientWithDoctorIntersectionSpec(id, doctorId);
            var prescriptionsFromDB = await _unitOfWork.Repository<Prescription>().GetAllWithSpecAsync(spec);
            if (prescriptionsFromDB.IsNullOrEmpty())
                return NotFound(new ApiResponse(404, "No prescriptions found for this patient."));

            return Ok(_mapper.Map<IReadOnlyList<Prescription>, IReadOnlyList<PrescriptionListViewFormDto>>(prescriptionsFromDB));
        }


        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpGet("GetAllPrescriptions")]
        public async Task<ActionResult<IReadOnlyList<PrescriptionListViewFormForPatientDto>>> GetAllPrescriptions()
        {
            var patientId = int.Parse(User.FindFirstValue(Identifiers.PatientId));
            var spec = new AllPrescriptionsForPatientSpec(patientId);
            var prescriptionsFromDB = await _unitOfWork.Repository<Prescription>().GetAllWithSpecAsync(spec);
            if (prescriptionsFromDB.IsNullOrEmpty())
                return NotFound(new ApiResponse(404, "No prescriptions found for this patient."));

            return Ok(_mapper.Map<IReadOnlyList<Prescription>, IReadOnlyList<PrescriptionListViewFormForPatientDto>>(prescriptionsFromDB));
        }
    }
}
