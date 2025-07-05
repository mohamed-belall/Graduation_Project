﻿using Graduation_Project.Core;
using Graduation_Project.Core.Models.Clinics;
using Graduation_Project.Core.Models.Doctors;
using Graduation_Project.Core.Models.Identity;
using Graduation_Project.Core.Specifications.SecretarySpecifications;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Secretary_Dashboard.MVC.Controllers
{
    public class QueueController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        public QueueController(IUnitOfWork unitOfWork, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        //public async Task<IActionResult> index()
        //{
        //    var email = User.FindFirstValue(ClaimTypes.Email);
        //    var user = await userManager.FindByEmailAsync(email);
        //    var id = user.Id; // from identity table

        //    var spec = new SecretaryByAppuserIdSpecification(id);
        //    var secretary = await _unitOfWork.Repository<Secretary>().GetWithSpecsAsync(spec);

        //    var doctor = secretary.doctors.FirstOrDefault();



        //    // Get Today Appointments For Table
        //    var specs = new AllDoctorAppointmentsSpecification(doctor.Id);

        //    var Todayappointments = await _unitOfWork.Repository<Appointment>().GetAllWithSpecAsync(specs); // included with patients details

        //    return View(Todayappointments);
        //}
    }
}
