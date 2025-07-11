using AutoMapper;
using Graduation_Project.Api.DTO.Doctors;
using Graduation_Project.Api.ErrorHandling;
using Graduation_Project.Api.Helpers;
using Graduation_Project.Core;
using Graduation_Project.Core.Constants;
using Graduation_Project.Core.IServices;
using Graduation_Project.Core.Models.Shared;
using Graduation_Project.Core.Specifications.DoctorSpecifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project.Api.Controllers.DoctorControllers
{
    public class PreviouslyBookedDoctorsController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly INotificationService notificationService;
        private readonly IMapper _mapper;

        public PreviouslyBookedDoctorsController(IUnitOfWork unitOfWork
                                , IUserService userService
                                , INotificationService notificationService
                                , IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.notificationService = notificationService;
            _mapper = mapper;
        }

        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpGet]
        public async Task<ActionResult<Pagination<SortingDoctorDto>>> getPreviouslyBookedDoctorsForCurrentPatient([FromQuery] FavrouiteDoctorSpecParams favrouiteDoctorSpecParams ,[FromQuery] string? lang = "en")
        {
            if(lang.ToLower() != "ar" && lang.ToLower() != "en")
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Language Not Supported"));

            // 1. get current patientId
            var patientId = int.Parse(User.FindFirstValue(Identifiers.PatientId));


            // 2. join patient , Appointment , doctor table and filter it
            var spec = new PreviouslyBookedDoctorsForCurrentPatientSpecification(patientId, favrouiteDoctorSpecParams);
            var appointmentRepo = unitOfWork.Repository<Appointment>();
            var appointments = await appointmentRepo.GetAllWithSpecAsync(spec);

            if (appointments is null || !appointments.Any())
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "No Previously Booked Doctors Found"));


            IReadOnlyList<Doctor> bookedDoctors = appointments
                 .Select(a => a.Doctor)
                 .DistinctBy(d => d.Id)
                 .ToList();


            // get Count of all Previously Booked Doctors Doctors
            var countSpecs = new PreviouslyBookedDoctorsForCurrentPatientSpecification(patientId);
            var appointmentsForCount = await appointmentRepo.GetAllWithSpecAsync(countSpecs);
            var count = appointmentsForCount.Select(a => a.Doctor)
                 .DistinctBy(d => d.Id)
                 .Count();


            var data = _mapper.Map<IReadOnlyList<Doctor>, IReadOnlyList<SortingDoctorDto>>(bookedDoctors, opts =>
            {
                opts.Items["lang"] = lang ?? "en";
                opts.Items["AvailabilityFilter"] = null;
            });

            return Ok(new Pagination<SortingDoctorDto>(favrouiteDoctorSpecParams.PageIndex, favrouiteDoctorSpecParams.PageSize, count, data));

        }
    }
}
