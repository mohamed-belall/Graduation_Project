using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graduation_Project.Core;
using Graduation_Project.Core.DTOs;
using Graduation_Project.Core.Common;
using Graduation_Project.Core.IServices;
using Graduation_Project.Core.Models.Doctors;
using Graduation_Project.Core.Models.Patients;
using Graduation_Project.Core.Specifications.AppointmentSpecs;
using Microsoft.IdentityModel.Tokens;

namespace Graduation_Project.Service
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppointmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public List<TimeOnly> GenerateTimeSlots(WorkSchedule schedule, int slotDurationMinutes)
        {
            var slots = new List<TimeOnly>();
            TimeOnly currentSlot = schedule.StartTime; // ✅ Keep the original StartTime untouched

            while (currentSlot < schedule.EndTime)
            {
                slots.Add(currentSlot);
                currentSlot = currentSlot.AddMinutes(slotDurationMinutes); // ✅ Modify only the local variable
            }

            return slots;
        }

        public List<SlotDto> GenerateTimeeSlots(WorkSchedule schedule, int slotDurationMinutes)
        {
            var slots = new List<SlotDto>();
            TimeOnly currentSlot = schedule.StartTime;

            while (currentSlot < schedule.EndTime)
            {
                slots.Add(new SlotDto
                {
                    Time = currentSlot,
                    IsAvailable = true
                });
                currentSlot = currentSlot.AddMinutes(slotDurationMinutes);
            }

            return slots;
        }

        //public async Task<ServiceResult<Dictionary<DateOnly, List<TimeOnly>>>> GetAvailableSlotsAsync(Doctor doctor)
        //{

        //    DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        //    int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

        //    var appointmentSpec = new AppointmentSpecifications(doctor.Id, today);
        //    var bookedAppointments = await _unitOfWork.Repository<Appointment>().GetAllWithSpecAsync(appointmentSpec);

        //    if (doctor.WorkSchedules.IsNullOrEmpty() && doctor.ScheduleExceptions.IsNullOrEmpty())
        //    {
        //        return ServiceResult<Dictionary<DateOnly, List<TimeOnly>>>.Failure("No schedules found for this doctor.");
        //    }


        //    // Use schedule exceptions from the doctor entity and handle if Null
        //    var scheduleExceptions = doctor.ScheduleExceptions?
        //        .ToLookup(e => e.Date) ?? (ILookup<DateOnly, ScheduleException>)Array.Empty<ScheduleException>().ToLookup(e => e.Date);

        //    // Convert work schedules to a dictionary for fast lookup
        //    var workScheduleDict = doctor.WorkSchedules.ToLookup(s => s.Day);

        //    // Group booked appointments by date
        //    var bookedAppointmentsDict = bookedAppointments
        //        .GroupBy(a => a.AppointmentDate)
        //        .ToDictionary(g => g.Key, g => g.ToList());


        //    // Initialize available slots dictionary
        //    Dictionary<DateOnly, List<TimeOnly>> availableSlots = new();
        //    _unitOfWork.Repository<Doctor>().Detach(doctor);

        //    // Loop through the whole month
        //    for (int i = 0; i < daysInMonth; i++)
        //    {
        //        DateOnly currentDate = today.AddDays(i);
        //        DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;

        //        // 1️⃣ Check if there's an exception for this specific date
        //        if (scheduleExceptions.Contains(currentDate))
        //        {
        //            var exceptionsForDay = scheduleExceptions[currentDate];

        //            List<TimeOnly> exceptionSlots = new();

        //            foreach (var exception in exceptionsForDay)
        //            {
        //                if (!exception.IsAvailable)
        //                {
        //                    exceptionSlots.Clear(); // If any exception blocks the day, remove all slots
        //                    break;
        //                }

        //                if (exception.StartTime.HasValue && exception.EndTime.HasValue)
        //                {
        //                    var slots = GenerateTimeSlots(new WorkSchedule
        //                    {
        //                        StartTime = exception.StartTime.Value,
        //                        EndTime = exception.EndTime.Value,
        //                        Day = currentDayOfWeek
        //                    }, doctor.SlotDurationMinutes);

        //                    exceptionSlots.AddRange(slots);
        //                }
        //            }
        //            exceptionSlots = RemoveBookedSlots(exceptionSlots, bookedAppointmentsDict, currentDate);

        //            if (exceptionSlots.Any())
        //            {
        //                availableSlots[currentDate] = exceptionSlots;
        //            }

        //            continue; // Move to the next day
        //        }


        //        // 2️⃣ No exception → use default schedule

        //        if (workScheduleDict.Contains(currentDayOfWeek))
        //        {
        //            foreach (var schedule in workScheduleDict[currentDayOfWeek])
        //            {
        //                var slots = GenerateTimeSlots(schedule, doctor.SlotDurationMinutes);

        //                slots = RemoveBookedSlots(slots, bookedAppointmentsDict, currentDate);

        //                if (slots.Any())
        //                {
        //                    if (!availableSlots.ContainsKey(currentDate))
        //                        availableSlots[currentDate] = new List<TimeOnly>();

        //                    availableSlots[currentDate].AddRange(slots);
        //                }
        //            }
        //        }
        //    }


        //    return ServiceResult<Dictionary<DateOnly, List<TimeOnly>>>.Success(availableSlots);
        //}

        public async Task<ServiceResult<Dictionary<DateOnly, List<SlotDto>>>> GetAvailableSlotsAsync(Doctor doctor)
        {

            DateOnly today = DateHelper.GetTodayInEgypt();
            int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

            var appointmentSpec = new AppointmentSpecifications(doctor.Id, today);
            var bookedAppointments = await _unitOfWork.Repository<Appointment>().GetAllWithSpecAsync(appointmentSpec);

            if (doctor.WorkSchedules.IsNullOrEmpty() && doctor.ScheduleExceptions.IsNullOrEmpty())
            {
                return ServiceResult<Dictionary<DateOnly, List<SlotDto>>>.Failure("No schedules found for this doctor.");
            }


            // Use schedule exceptions from the doctor entity and handle if Null
            var scheduleExceptions = doctor.ScheduleExceptions?
                .ToLookup(e => e.Date) ?? (ILookup<DateOnly, ScheduleException>)Array.Empty<ScheduleException>().ToLookup(e => e.Date);

            // Convert work schedules to a dictionary for fast lookup
            var workScheduleDict = doctor.WorkSchedules.ToLookup(s => s.Day);

            // Group booked appointments by date
            var bookedAppointmentsDict = bookedAppointments
                .GroupBy(a => a.AppointmentDate)
                .ToDictionary(g => g.Key, g => g.ToList());


            // Initialize available slots dictionary
            Dictionary<DateOnly, List<SlotDto>> availableSlots = new();
            _unitOfWork.Repository<Doctor>().Detach(doctor);

            // Loop through the whole month
            for (int i = 0; i < daysInMonth; i++)
            {
                DateOnly currentDate = today.AddDays(i);
                DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;

                // 1️⃣ Check if there's an exception for this specific date
                if (scheduleExceptions.Contains(currentDate))
                {
                    var exceptionsForDay = scheduleExceptions[currentDate];

                    List<SlotDto> exceptionSlots = new();

                    foreach (var exception in exceptionsForDay)
                    {
                        if (!exception.IsAvailable)
                        {
                            exceptionSlots.Clear(); // If any exception blocks the day, remove all slots
                            break;
                        }

                        if (exception.StartTime.HasValue && exception.EndTime.HasValue)
                        {
                            var slots = GenerateTimeeSlots(new WorkSchedule
                            {
                                StartTime = exception.StartTime.Value,
                                EndTime = exception.EndTime.Value,
                                Day = currentDayOfWeek
                            }, doctor.SlotDurationMinutes);

                            exceptionSlots.AddRange(slots);
                        }
                    }
                    exceptionSlots = MarkSlotAvailability(exceptionSlots, bookedAppointmentsDict, currentDate);

                    if (exceptionSlots.Any())
                    {
                        availableSlots[currentDate] = exceptionSlots;
                    }

                    continue; // Move to the next day
                }


                // 2️⃣ No exception → use default schedule

                if (workScheduleDict.Contains(currentDayOfWeek))
                {
                    foreach (var schedule in workScheduleDict[currentDayOfWeek])
                    {
                        var slots = GenerateTimeeSlots(schedule, doctor.SlotDurationMinutes);

                        slots = MarkSlotAvailability(slots, bookedAppointmentsDict, currentDate);

                        if (slots.Any())
                        {
                            if (!availableSlots.ContainsKey(currentDate))
                                availableSlots[currentDate] = new List<SlotDto>();

                            availableSlots[currentDate].AddRange(slots);
                        }
                    }
                }
            }


            return ServiceResult<Dictionary<DateOnly, List<SlotDto>>>.Success(availableSlots);
        }

        private List<SlotDto> MarkSlotAvailability(List<SlotDto> allSlots,
                                                   Dictionary<DateOnly, List<Appointment>> bookedAppointmentsDict,
                                                   DateOnly currentDate)
        {
            if (bookedAppointmentsDict.TryGetValue(currentDate, out var bookedAppointmentsForDay))
            {
                var bookedTimes = bookedAppointmentsForDay
                    .Where(a => a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Pending)
                    .Select(a => a.AppointmentTime)
                    .ToHashSet(); // For faster lookup

                foreach (var slot in allSlots)
                {
                    if (bookedTimes.Contains(slot.Time))
                    {
                        slot.IsAvailable = false;
                    }
                }
            }

            return allSlots;
        }


        private List<TimeOnly> RemoveBookedSlots(List<TimeOnly> slots,
                                         Dictionary<DateOnly, List<Appointment>> bookedAppointmentsDict,
                                         DateOnly currentDate)
        {
            if (bookedAppointmentsDict.TryGetValue(currentDate, out var bookedAppointmentsForDay))
            {
                var bookedTimes = bookedAppointmentsForDay
                    .Where(a => a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Pending)
                    .Select(a => a.AppointmentTime)
                    .ToList();

                return slots.Except(bookedTimes).ToList(); // ✅ Return updated slots list
            }

            return slots;
        }

        public bool CheckIfPatientPayedVisita(Patient patient, bool payed)
        {
            //Payment Check Logic
            return payed;
        }
    }
}
