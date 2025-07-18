using Graduation_Project.Core;
using Graduation_Project.Core.IServices;
using Graduation_Project.Core.Models.Clinics;
using Graduation_Project.Core.Models.Doctors;
using Graduation_Project.Core.Models.Identity;
using Graduation_Project.Core.Models.Patients;
using Graduation_Project.Core.Specifications.AppointmentSpecs;
using Graduation_Project.Core.Specifications.SecretarySpecifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Secretary_Dashboard.MVC.Models;
using System.Diagnostics;
using System.Numerics;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Secretary_Dashboard.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly INotificationService _notification;

        public HomeController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager,SignInManager<AppUser> signInManager , INotificationService notification)
        {
            _unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.signInManager = signInManager;
            _notification = notification;
        }
    
        public async Task<IActionResult> Index()
        {
            

            var email = User.FindFirstValue(ClaimTypes.Email);
            var user =await userManager.FindByEmailAsync(email);
            var id = user.Id; // from identity table

            var spec = new SecretaryByAppuserIdSpecification(id);
            var secretary = await _unitOfWork.Repository<Secretary>().GetWithSpecsAsync(spec);

            var doctor = secretary.clinic.Doctor;



            // Get Today Appointments For Table
            var specs = new AllDoctorAppointmentsSpecification(doctor.Id);

            var appointments = await _unitOfWork.Repository<Appointment>().GetAllWithSpecAsync(specs); // included with patients details

            // Get Today ConfirmedAppoitments FOR Queue
            var ConfirmedAppoitmentsSPEC = new AllDoctorAppointmentsConfirmedSpecification(doctor.Id);

            var  OnlyConfirmedAppoitments = await _unitOfWork.Repository<Appointment>().GetAllWithSpecAsync(ConfirmedAppoitmentsSPEC);

            ViewData["AllTodayappointments"] = appointments;  // for table of today appointments
            return View(OnlyConfirmedAppoitments); 
        }
        public async Task<IActionResult> Exit(int Id)
        {

            //Get Appointment From DB included with patient
            var PatientAppointmentForSpecificDoctorSPEC = new PatientAppointmentForSpecificDoctorSpecification(Id);
            
            Appointment ConfirmedAppointment = await _unitOfWork.Repository<Appointment>().GetWithSpecsAsync(PatientAppointmentForSpecificDoctorSPEC);
            // change Status of appointment and save into DB 
            ConfirmedAppointment.Status = AppointmentStatus.Completed;
            var CompletedAppointment = ConfirmedAppointment;
            // Save to DB
            await _unitOfWork.Repository<Appointment>().SaveAsync();
            return RedirectToAction("index");
        }
        public async Task<IActionResult> SendReminder(string ApplicationUserId)
        {

            if (ApplicationUserId == null)
            {
                TempData["Error"] = "Patient information not found.";
                return RedirectToAction("Index");
            }


            await _notification.SendNotificationAsync(ApplicationUserId, "Your Turn Is the Next", "Appointment Reminder");
            TempData["Success"] = "Notification sent successfully.";
            return RedirectToAction("index");
        }

        public async Task<IActionResult> Cancel(int AppointmentId)
        {

            var _Appoinmtent = await _unitOfWork.Repository<Appointment>().GetAsync(AppointmentId);

            if (_Appoinmtent is null)
            {
                TempData["Error"] = "No confirmed appointment found for today.";
                return RedirectToAction("Index");
            }
            _Appoinmtent.Status = AppointmentStatus.Cancelled;

            await _unitOfWork.CompleteAsync();

            TempData["Success"] = "Appointment cancelled successfully.";
            return RedirectToAction("Index");

        }

    }
}
