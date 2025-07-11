using Graduation_Project.Core;
using Graduation_Project.Core.Models.Doctors;
using Graduation_Project.Core.Models.Patients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Secretary_Dashboard.MVC.ViewModel;

namespace Secretary_Dashboard.MVC.Controllers
{


    public class AddPatientController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddPatientController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;   
        }
        public IActionResult Index()
        {

            return View(new ConsultationFormVM());
        }


        [HttpPost]
        public async Task<IActionResult> Index(ConsultationFormVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var NewPatient = new Patient
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                DateOfBirth = vm.DateOfBirth,
                Gender = (Graduation_Project.Core.Enums.Gender)vm.Gender,
            
                PhoneNumber = vm.PhoneNumber
            };
            await _unitOfWork.Repository<Patient>().AddAsync(NewPatient);
            await _unitOfWork.Repository<Patient>().SaveAsync();

            if (vm.AppointmentDate.HasValue && vm.AppointmentTime.HasValue)
            {
                var appointmentDate = vm.AppointmentDate.Value;
                var appointmentTime = vm.AppointmentTime.Value;

                var NewAppointment = new Appointment
                {
                    AppointmentDate = appointmentDate,
                    AppointmentTime = appointmentTime,
                    PatientId = NewPatient.Id
                };

                await _unitOfWork.Repository<Appointment>().AddAsync(NewAppointment);
                await _unitOfWork.Repository<Appointment>().SaveAsync();
            }

            return RedirectToAction(nameof(ThankYou));
        }

        public IActionResult ThankYou()
        {
            return View();
        }

    }
}
