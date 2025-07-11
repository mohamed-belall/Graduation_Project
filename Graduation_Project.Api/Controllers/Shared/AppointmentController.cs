using AutoMapper;
using Graduation_Project.Api.DTO.Doctors;
using Graduation_Project.Api.DTO.Shared;
using Graduation_Project.Api.ErrorHandling;
using Graduation_Project.Api.Helpers;
using Graduation_Project.Api.Filters;
using Graduation_Project.Core;
using Graduation_Project.Core.Common;
using Graduation_Project.Core.Constants;
using Graduation_Project.Core.IServices;
using Graduation_Project.Core.Models.Doctors;
using Graduation_Project.Core.Specifications.AppointmentSpecs;
using Graduation_Project.Core.Specifications.DoctorPolicySpecs;
using Graduation_Project.Core.Specifications.DoctorSpecifications;
using Graduation_Project.Core.Specifications.WorkScheduleSpecs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace Graduation_Project.Api.Controllers.Shared
{
    public class AppointmentController : BaseApiController
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AppointmentController(IAppointmentService appointmentService, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _appointmentService = appointmentService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        //[HttpGet("available-slots/{doctorId:int}")]
        //public async Task<ActionResult<Dictionary<DateOnly, List<TimeOnly>>>> GetAvailableSlots(int doctorId)
        //{
        //    try
        //    {
        //        // Step 1: Retrieve doctor and check existence
        //        var doctor = await _unitOfWork.Repository<Doctor>()
        //            .GetWithSpecsAsync(new DoctorWithWorkScheduleSpecifications(doctorId));
        //        if (doctor == null)
        //        {
        //            return NotFound(new ApiResponse(404, "Doctor not found"));
        //        }

        //        // Step 4: Get booked appointments for the selected week
        //        var result = await _appointmentService.GetAvailableSlotsAsync(doctor);

        //        if (!result.IsSuccess)
        //            return NotFound(new ApiResponse(404, result.ErrorMessage));

        //        var formattedResult = result.Data.ToDictionary(
        //            kvp => kvp.Key.ToString("yyyy-MM-dd"),
        //            kvp => kvp.Value.Select(t => t.ToString("hh:mm tt")).ToList()
        //        );
        //        return Ok(formattedResult);
        //    }

        //    catch (Exception ex)
        //    {
        //        return BadRequest(new ApiResponse(400, $"Error: {ex.Message}"));
        //    }
        //}

        [HttpGet("available-slots/{doctorId:int}")]
        public async Task<ActionResult<Dictionary<DateOnly, List<TimeOnly>>>> GetAvailableSlots(int doctorId)
        {
            try
            {
                // Step 1: Retrieve doctor and check existence
                var doctor = await _unitOfWork.Repository<Doctor>()
                    .GetWithSpecsAsync(new DoctorWithWorkScheduleSpecifications(doctorId));
                if (doctor == null)
                {
                    return NotFound(new ApiResponse(404, "Doctor not found"));
                }

                // Step 4: Get booked appointments for the selected week
                var result = await _appointmentService.GetAvailableSlotsAsync(doctor);

                if (!result.IsSuccess)
                    return NotFound(new ApiResponse(404, result.ErrorMessage));

                var formattedResult = result.Data.ToDictionary(
                    kvp => kvp.Key.ToString("yyyy-MM-dd"),
                    kvp => kvp.Value.Select(t => new { time = t.Time.ToString("hh:mm tt"), t.IsAvailable }).ToList()
                );
                return Ok(formattedResult);
            }

            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, $"Error: {ex.Message}"));
            }
        }

        //[Authorize(Roles = nameof(UserRoleType.Patient))]
        //[HttpPost("book")]
        //public async Task<ActionResult> BookAppointment([FromBody] BookAppointmentDto request)
        //{
        //    // 1️⃣ Validate Doctor Exists
        //    var doctor = await _unitOfWork.Repository<Doctor>()
        //        .GetWithSpecsAsync(new DoctorWithWorkScheduleSpecifications(request.DoctorId));
        //    if (doctor == null)
        //        return NotFound(new ApiResponse(404, "Doctor not found"));

        //    // 2️⃣ Patient Exists
        //    var PatientId = int.Parse(User.FindFirstValue(Identifiers.PatientId));
        //    var patient = await _unitOfWork.Repository<Patient>().GetAsync(PatientId);

        //    // All of the upcoming code should be a common service
        //    var dpSpec = new DoctorPolicySpecifications(request.DoctorId);
        //    var doctorPolicies = await _unitOfWork.Repository<DoctorPolicy>().GetAllWithSpecAsync(dpSpec);

        //    // If no custom policies exist, return the default policy
        //    if (!doctorPolicies.Any())
        //    {
        //        var dpfSpec = new DefaultDoctorPolicySpecifications();
        //        doctorPolicies = await _unitOfWork.Repository<DoctorPolicy>().GetAllWithSpecAsync(dpfSpec);

        //        if (doctorPolicies == null || !doctorPolicies.Any())
        //        {
        //            // Handle missing policy gracefully
        //            return NotFound(new ApiResponse(404, "Default Doctor's policy not found"));
        //        }
        //    }
        //    // This is either the default or the latest custom
        //    var latestPolicy = doctorPolicies.OrderByDescending(p => p.CreatedAt).First();
        //    // End of service

        //    // 3️⃣ Check if the doctor is available on the given day
        //    var workSchedules = doctor.WorkSchedules.Where(ws => ws.Day == request.AppointmentDate.DayOfWeek);
        //    if (!workSchedules.Any())
        //        return NotFound(new ApiResponse(404, "Doctor doesn't work that day")); // Doctor doesn't work that day

        //    // 4️⃣ Check if the selected time falls within any shift
        //    bool isTimeValid = workSchedules.Any(ws => request.AppointmentTime >= ws.StartTime && request.AppointmentTime < ws.EndTime);
        //    if (!isTimeValid)
        //        return NotFound(new ApiResponse(404, "Time is outside of work schedule")); // Time is outside of work schedule

        //    // Step 5: Check if the appointment is in the generated slots
        //    var availableSlotsResult = await _appointmentService.GetAvailableSlotsAsync(doctor);

        //    // Check if the result was successful.
        //    if (!availableSlotsResult.IsSuccess)
        //    {
        //        return NotFound(new ApiResponse(404, availableSlotsResult.ErrorMessage));
        //    }

        //    // Check if the doctor has work schedules (to ensure slots are generated).
        //    if (!availableSlotsResult.Data.Any())
        //    {
        //        return NotFound(new ApiResponse(404, "No available slots for the doctor To Book."));
        //    }

        //    // get appointments for the same doctor and patient today
        //    var appointmentSpec = new AppointmentByPatientDoctorDateSpec(PatientId, request.DoctorId, request.AppointmentDate);
        //    var existingAppointmentForPatient = await _unitOfWork.Repository<Appointment>().GetWithSpecsAsync(appointmentSpec);
        //    // ignore if cancelled

        //    if (existingAppointmentForPatient is not null &&
        //        (existingAppointmentForPatient.Status == AppointmentStatus.Confirmed || existingAppointmentForPatient.Status == AppointmentStatus.Pending))
        //    {
        //        return BadRequest(new ApiResponse(400, "You already have an appointment with this doctor on the same day."));
        //    }

        //    // Step 2: Check if the selected time is available
        //    var availableSlots = availableSlotsResult.Data.TryGetValue(request.AppointmentDate, out var daySlots)
        //             && daySlots.Any(slot => slot == request.AppointmentTime);

        //    if (!availableSlots)
        //    {
        //        return BadRequest(new ApiResponse(400, "The appointment time is not available."));
        //    }

        //    // Apply policy checks such as prepayment, cancellation window, etc.

        //    // Step 7: Create and Save Appointment

        //    var appointment = _mapper.Map<BookAppointmentDto, Appointment>(request);
        //    appointment.RescheduleCount = 0;
        //    appointment.PolicyId = latestPolicy.Id;
        //    appointment.PatientId = PatientId;

        //    // Check if payment Service is done
        //    var payed = true;
        //    if (_appointmentService.CheckIfPatientPayedVisita(patient, payed))
        //    {
        //        // Assume that patient payed so the Confirmation is Done
        //        appointment.Status = AppointmentStatus.Confirmed;
        //    }
        //    else
        //    {
        //        // Check if not paying is allowed
        //        if (latestPolicy.RequirePrePayment)
        //        {
        //            return NotFound(new ApiResponse(404, "Patient Need to Pay"));
        //        }
        //        else
        //        {
        //            appointment.Status = AppointmentStatus.Pending; // Assuming the doctor approves first
        //        }
        //    }

        //    await _unitOfWork.Repository<Appointment>().AddAsync(appointment);
        //    var result = await _unitOfWork.CompleteAsync();
        //    if (result == 0)
        //    {
        //        return BadRequest(new ApiResponse(400));
        //    }

        //    return Ok(new ApiResponse(200, "Appointment booked successfully!"));
        //}

        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpPost("book")]
        public async Task<ActionResult> BookAppointment([FromBody] BookAppointmentDto request)
        {
            if (!DateTime.TryParseExact(request.AppointmentTime, "hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDateTime))
            {
                return BadRequest(new ApiResponse(400, "Invalid time format. Expected format: hh:mm AM/PM"));
            }

            var parsedTime = TimeOnly.FromDateTime(parsedDateTime);

            // 1️⃣ Validate Doctor Exists
            var doctor = await _unitOfWork.Repository<Doctor>()
                .GetWithSpecsAsync(new DoctorWithWorkScheduleSpecifications(request.DoctorId));
            if (doctor == null)
                return NotFound(new ApiResponse(404, "Doctor not found"));

            // 2️⃣ Patient Exists
            var PatientId = int.Parse(User.FindFirstValue(Identifiers.PatientId));
            var patient = await _unitOfWork.Repository<Patient>().GetAsync(PatientId);
            if (patient == null)
                return NotFound(new ApiResponse(404, "Patient not found"));

            // All of the upcoming code should be a common service
            var dpSpec = new DoctorPolicySpecifications(request.DoctorId);
            var doctorPolicies = await _unitOfWork.Repository<DoctorPolicy>().GetAllWithSpecAsync(dpSpec);

            // If no custom policies exist, return the default policy
            if (!doctorPolicies.Any())
            {
                var dpfSpec = new DefaultDoctorPolicySpecifications();
                doctorPolicies = await _unitOfWork.Repository<DoctorPolicy>().GetAllWithSpecAsync(dpfSpec);

                if (doctorPolicies == null || !doctorPolicies.Any())
                {
                    // Handle missing policy gracefully
                    return NotFound(new ApiResponse(404, "Default Doctor's policy not found"));
                }
            }
            // This is either the default or the latest custom
            var latestPolicy = doctorPolicies.OrderByDescending(p => p.CreatedAt).First();
            // End of service

            // 3️⃣ Check if the doctor is available on the given day
            var workSchedules = doctor.WorkSchedules.Where(ws => ws.Day == request.AppointmentDate.DayOfWeek);
            if (!workSchedules.Any())
                return NotFound(new ApiResponse(404, "Doctor doesn't work that day")); // Doctor doesn't work that day

            // 4️⃣ Check if the selected time falls within any shift
            bool isTimeValid = workSchedules.Any(ws => parsedTime >= ws.StartTime && parsedTime < ws.EndTime);
            if (!isTimeValid)
                return NotFound(new ApiResponse(404, "Time is outside of work schedule")); // Time is outside of work schedule

            // Step 5: Check if the appointment is in the generated slots
            var availableSlotsResult = await _appointmentService.GetAvailableSlotsAsync(doctor);

            // Check if the result was successful.
            if (!availableSlotsResult.IsSuccess)
            {
                return NotFound(new ApiResponse(404, availableSlotsResult.ErrorMessage));
            }

            // Step 2: Check if the selected time is available
            var isSlotAvailable = availableSlotsResult.Data.TryGetValue(request.AppointmentDate, out var daySlots)
                && daySlots.Any(slot => slot.Time == parsedTime && slot.IsAvailable);

            if (!isSlotAvailable)
            {
                return BadRequest(new ApiResponse(400, "The appointment time is not available."));
            }

            // get appointments for the same doctor and patient today
            var appointmentSpec = new AppointmentByPatientDoctorDateSpec(PatientId, request.DoctorId, request.AppointmentDate);
            var existingAppointmentForPatient = await _unitOfWork.Repository<Appointment>().GetWithSpecsAsync(appointmentSpec);
            // ignore if cancelled

            if (existingAppointmentForPatient is not null &&
                (existingAppointmentForPatient.Status == AppointmentStatus.Confirmed || existingAppointmentForPatient.Status == AppointmentStatus.Pending))
            {
                return BadRequest(new ApiResponse(400, "You already have an appointment with this doctor on the same day."));
            }

            // Apply policy checks such as prepayment, cancellation window, etc.

            // Step 7: Create and Save Appointment

            var appointment = _mapper.Map<BookAppointmentDto, Appointment>(request);
            appointment.AppointmentTime = parsedTime;
            appointment.RescheduleCount = 0;
            appointment.PolicyId = latestPolicy.Id;
            appointment.PatientId = PatientId;

            // Check if payment Service is done
            var payed = true;
            if (_appointmentService.CheckIfPatientPayedVisita(patient, payed))
            {
                // Assume that patient payed so the Confirmation is Done
                appointment.Status = AppointmentStatus.Confirmed;
            }
            else
            {
                // Check if not paying is allowed
                if (latestPolicy.RequirePrePayment)
                {
                    return NotFound(new ApiResponse(404, "Patient Need to Pay"));
                }
                else
                {
                    appointment.Status = AppointmentStatus.Pending; // Assuming the doctor approves first
                }
            }

            await _unitOfWork.Repository<Appointment>().AddAsync(appointment);
            var result = await _unitOfWork.CompleteAsync();
            if (result == 0)
            {
                return BadRequest(new ApiResponse(400));
            }

            return Ok(new ApiResponse(200, "Appointment booked successfully!"));
        }

        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpDelete("cancel-booking/{id:int}")]
        public async Task<ActionResult> CancelAppointment(int id)
        {
            // Step 1: Get the appointment and related entities
            var appointment = await _unitOfWork.Repository<Appointment>()
                .GetWithSpecsAsync(new AppointmentWithPolicySpecifications(id));
            if (appointment == null)
            {
                return NotFound(new ApiResponse(404, "Appointment not found"));
            }

            var PatientId = int.Parse(User.FindFirstValue(Identifiers.PatientId));
            if (appointment.PatientId != PatientId)
            {
                return Unauthorized(new ApiResponse(401, "This Appointment Doesnt belong to this Patient"));
            }

            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                BadRequest(new ApiResponse(400, "Appointment Already Cancelled"));
            }

            // Get Policy For checking cancel logic for Doctor
            var doctorPolicy = appointment.Policy;

            // Step 2: Check if cancellation is allowed based on the policy and the time difference
            var currentTime = DateTime.UtcNow;
            var appointmentTime = appointment.AppointmentDate.ToDateTime(appointment.AppointmentTime);

            var hoursBeforeAppointment = (appointmentTime - currentTime).TotalHours;

            if (hoursBeforeAppointment < doctorPolicy.MinCancellationHours)
            {
                // If the cancellation is too late
                if (doctorPolicy.AllowLateCancellationReschedule)
                {
                    // Option to reschedule (if allowed in the policy)
                    return Ok(new ApiResponse(200, "Late cancellation allowed. You may reschedule."));
                }
                else
                {
                    // No cancellation allowed, refund, or reschedule options
                    return BadRequest(new ApiResponse(400, "Late cancellations are not allowed."));
                }
            }

            //// Step 3: Refund logic based on cancellation time
            //bool isRefundEligible = latestPolicy.AllowFullRefund;
            //decimal refundAmount = isRefundEligible ? appointment.AmountPaid : 0; // Adjust based on policy

            //// Step 4: Process refund if eligible and update the appointment status
            //if (refundAmount > 0)
            //{
            //    // Handle refund logic
            //    // E.g., calling a payment service to refund the amount
            //}

            // Step 5: Mark the appointment as canceled
            try
            {
                appointment.Status = AppointmentStatus.Cancelled;
                _unitOfWork.Repository<Appointment>().Update(appointment);

                var result = await _unitOfWork.CompleteAsync();
                if (result == 0)
                {
                    return BadRequest(new ApiResponse(400, "Error canceling the appointment"));
                }

                return Ok(new ApiResponse(200, "Appointment successfully cancelled and refunded."));
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, $"Error: {ex.Message}"));
            }
        }

        [Authorize(Roles = nameof(UserRoleType.Doctor))]
        [HttpGet("get-by-name")]
        public async Task<ActionResult<Dictionary<string, List<AppointmentDto>>>> GetPatientAppointmentsAsync(string name)
        {
            // get doctor id
            var doctorId = int.Parse(User.FindFirstValue(Identifiers.DoctorId));
            var appointmentSpec = new AppointmentsForDoctorSearchSpecifications
            (
                new AppointmentSpecParams
                {
                    Id = doctorId,
                    Search = name // This automatically sets FirstNameSearch and LastNameSearch
                }
            );
            var appointments = await _unitOfWork.Repository<Appointment>().GetAllWithSpecAsync(appointmentSpec);

            if (appointments.IsNullOrEmpty())
            {
                return NotFound(new ApiResponse(404, "No appointments found for this Doctor."));
            }

            // Convert to DTOs for cleaner response
            var appointmentDtos = _mapper.Map<List<AppointmentDto>>(appointments);

            var groupedAppointments = appointmentDtos
                .GroupBy(a => a.AppointmentDate)
                .OrderByDescending(g => g.Key)
                .ToDictionary(
                    g => g.Key, // Format date as string
                    g => g.OrderBy(a => a.AppointmentTime)
                        .ThenBy(a => a.Status == "Pending" ? 0 :
                                    a.Status == "Confirmed" ? 1 :
                                    a.Status == "Completed" ? 2 : 3)
                        .ToList()
                );

            return Ok(groupedAppointments);
        }


        [Authorize(Roles = nameof(UserRoleType.Doctor))]
        [HttpGet("get-todays-appointment")]
        public async Task<ActionResult<List<AppointmentDto>>> GetTodayAppointmentsAsync(DateOnly? day = null)
        {
            var doctorId = int.Parse(User.FindFirstValue(Identifiers.DoctorId));
            // Define the query specification for today's appointments
            DateOnly requiredDay;
            if (day.HasValue)
            {
                requiredDay = day.Value;
            }
            else
            {
                requiredDay = DateHelper.GetTodayInEgypt();
            }
            var appointmentSpec = new AppointmentsForSearchSpecifications(doctorId, requiredDay);
            var appointments = await _unitOfWork.Repository<Appointment>().GetAllWithSpecAsync(appointmentSpec);

            if (appointments.IsNullOrEmpty())
            {
                return NotFound(new ApiResponse(404, "No appointments found for this Day."));
            }

            // Convert to DTOs for cleaner response
            var appointmentDtos = _mapper.Map<List<AppointmentDto>>(appointments);

            var groupedAppointments = appointmentDtos
                .OrderBy(a => a.AppointmentTime)
                .ThenBy(appt => appt.Status == "Pending" ? 0 :
                                appt.Status == "Confirmed" ? 1 :
                                appt.Status == "Completed" ? 2 : 3)
                .ToList();

            return Ok(groupedAppointments);
        }

        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpGet("get-all-appointments")]
        public async Task<ActionResult<Dictionary<string, List<AppointmentDto>>>> GetAllAppointmentsAsync()
        {
            var patientId = int.Parse(User.FindFirstValue(Identifiers.PatientId));
            // Define the query specification for today's appointments
            DateOnly requiredDay = requiredDay = DateHelper.GetTodayInEgypt();
            var appointmentSpec = new AppointmentsForPatientSearchSpecifications(patientId, requiredDay);
            var appointments = await _unitOfWork.Repository<Appointment>().GetAllWithSpecAsync(appointmentSpec);

            if (appointments.IsNullOrEmpty())
            {
                return NotFound(new ApiResponse(404, "No appointments found for this Day."));
            }

            // Convert to DTOs for cleaner response
            var appointmentDtos = _mapper.Map<List<AppointmentForPatientDto>>(appointments);

            var groupedAppointments = appointmentDtos
                .GroupBy(a => a.AppointmentDate)
                .ToDictionary(
                    dateGroup => dateGroup.Key,
                    dateGroup => dateGroup
                        .GroupBy(a => a.AppointmentTime)
                        .ToDictionary(
                            timeGroup => timeGroup.Key,
                            timeGroup => timeGroup
                                .OrderBy(appt => appt.Status == "Pending" ? 0 :
                                                 appt.Status == "Confirmed" ? 1 :
                                                 appt.Status == "Completed" ? 2 : 3)
                                .ToList()
                        )
                );

            return Ok(groupedAppointments);
        }

        [Authorize(Roles = nameof(UserRoleType.Doctor))]
        [HttpGet("get-current")]
        public async Task<ActionResult<List<AppointmentForPatientDto>>> GetCurrentPatient()
        {
            var doctorId = int.Parse(User.FindFirstValue(Identifiers.DoctorId));
            DateOnly today = DateHelper.GetTodayInEgypt();

            var appointments = await _unitOfWork.Repository<Appointment>().GetAllWithSpecAsync
            (
                new AppointmentsForDoctorCurrentSpecifications(doctorId, today, TimeOnly.FromDateTime(DateHelper.GetNowInEgypt()))
            );

            if (appointments.IsNullOrEmpty())
            {
                return NotFound(new ApiResponse(404, "No appointments found for this Day."));
            }
            var appointmentDtos = _mapper.Map<List<AppointmentDto>>(appointments);

            var sortedAppointments = appointmentDtos
                .OrderBy(a => a.AppointmentTime)
                .ThenBy(appt => appt.Status == "Pending" ? 0 : 1)
                .ToList();

            return Ok(sortedAppointments);
        }

        [Authorize(Roles = nameof(UserRoleType.Doctor))]
        [HttpGet("get-completed-and-cancelled")]
        public async Task<ActionResult<List<AppointmentDto>>> GetCompleteAndCancelledPatient([FromQuery]bool cancelled = false)
        {
            var doctorId = int.Parse(User.FindFirstValue(Identifiers.DoctorId));
            DateOnly today = DateHelper.GetTodayInEgypt();

            var appointments = await _unitOfWork.Repository<Appointment>().GetAllWithSpecAsync
            (
                new AppointmentsForDoctorCompletedSpecifications(doctorId, today, cancelled)
            );

            if (appointments.IsNullOrEmpty())
            {
                return NotFound(new ApiResponse(404, cancelled
                    ? "No cancelled appointments found."
                    : "No completed appointments found."));
            }
            var appointmentDtos = _mapper.Map<List<AppointmentDto>>(appointments);
            var sortedAppointments = appointmentDtos
                .OrderByDescending(a => a.AppointmentTime) // descending by time
                .ToList();

            return Ok(sortedAppointments);
        }
    }
}
