using Graduation_Project.Core;
using Graduation_Project.Core.Enums;
using Graduation_Project.Core.IServices;
using Graduation_Project.Core.Models.Doctors;
using Graduation_Project.Core.Models.Identity;
using Graduation_Project.Core.Models.Patients;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Project.Service
{
    public class PatientServcie : IPatientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;

        public PatientServcie(IUnitOfWork unitOfWork , IConfiguration configuration, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _userManager = userManager;
        }
        public async Task<object?> GetInfo(int Id, string? Email)
        {
            var patient = await _unitOfWork.Repository<Patient>().GetAsync(Id);
            if (patient is null)
                return null;

            if (!string.IsNullOrEmpty(patient.PictureUrl))
                patient.PictureUrl = patient.PictureUrl[0] == '/' ? $"{_configuration["AzureStorageUrl"]}{patient.PictureUrl}" : $"{_configuration["AzureStorageUrl"]}/{patient.PictureUrl}";

            if (Email is null)
            {
                var user = await _userManager.FindByIdAsync(patient.ApplicationUserId);
                if (user is null)
                    return null;

                Email = user.Email;

                // to can return patient id in the Endpoint of Get patient info in Doctor Controller
                var patientInfoFromDoctor = new
                {
                    patientId = Id,
                    fullName = patient.FirstName + " " + patient.LastName,
                    email = Email,
                    phoneNumber = patient.PhoneNumber,
                    gender = patient.Gender,
                    dateOfBirth = patient.DateOfBirth,
                    pictureUrl = patient.PictureUrl,
                    bloodType = patient.BloodType
                };
                return patientInfoFromDoctor;
            }
            
            var patientInfo = new
            {
                fullName = patient.FirstName + " " + patient.LastName,
                email = Email,
                phoneNumber = patient.PhoneNumber,
                gender = patient.Gender,
                dateOfBirth = patient.DateOfBirth,
                pictureUrl = patient.PictureUrl,
                bloodType = patient.BloodType
            };
            return patientInfo;
        }

        public async Task UpdatePoints(int PatientId, int Points)
        {
            // get patient from db
            var patient = await _unitOfWork.Repository<Patient>().GetAsync(PatientId);
            patient.Points += Points;
            // Update its points
            _unitOfWork.Repository<Patient>().Update(patient);
            await _unitOfWork.Repository<Patient>().SaveAsync();
        }
    }
}