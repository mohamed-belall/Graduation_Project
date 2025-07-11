using Graduation_Project.Core;
using Graduation_Project.Core.Enums;
using Graduation_Project.Core.Models.Clinics;
using Graduation_Project.Core.Models.Doctors;
using Graduation_Project.Core.Models.Identity;
using Graduation_Project.Core.Models.Notifications;
using Graduation_Project.Core.Models.Patients;
using Graduation_Project.Core.Specifications.SecretarySpecifications;
using Graduation_Project.Repository;
using Graduation_Project.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Secretary_Dashboard.MVC.Controllers
{
    public class EmergencyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly NotificationService _notificationService;

        public EmergencyController(IUnitOfWork unitOfWork, NotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public IActionResult index()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SendEmergencyNotice(string message, string recipientGroup, List<string> UserIds)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Message cannot be empty.";
                return RedirectToAction("index");
            }

            var today = DateOnly.FromDateTime(DateTime.Today);

            // إحضار المرضى حسب الاختيار
            List<Patient> patients;

            if (recipientGroup == "Today's Patients")
            {
                var appointmentsSpec = new AppointmentsWithPatientsForTodaySpecification(today);
                var appointments = await _unitOfWork.Repository<Appointment>().GetAllWithSpecAsync(appointmentsSpec); //return appointments included with patients

                var patientsUserIds = appointments.Select(a => a.Patient.ApplicationUserId).Distinct();

                foreach (string id in patientsUserIds)
                {
                    await _notificationService.SendNotificationAsync(id, message,"Appointment Remender");
                }

            }
            else if (recipientGroup == "All Patients")
            {
                patients = (List<Patient>)await _unitOfWork.Repository<Patient>().GetAllAsync();

                var patientsUserIds = patients.Select(p => p.ApplicationUserId).Distinct();

                foreach (string id in patientsUserIds)
                {
                    await _notificationService.SendNotificationAsync(id, message, "Appointment Remender");
                }
            }
            else
            {
                TempData["Error"] = "Please select a valid recipient group.";
                return RedirectToAction("index");
            }

            TempData["Success"] = "Emergency notification sent successfully.";
            return RedirectToAction("index");
        }





    }
}
