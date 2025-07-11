using AutoMapper.Configuration.Conventions;
using Graduation_Project.Api.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Talabat.API.Dtos.Account
{
    public class DoctorRegisterDTO
    {
        [FullName(ErrorMessage = "Full name must contain at least two words.")] // Fname + Lname
        [Required(ErrorMessage = "FullName is required.")]
        [StringLength(maximumLength: 100, MinimumLength = 3, ErrorMessage = "Full Name must be at least 3 characters and at most 100 characters.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [RegularExpression("^(?!\\d)[a-zA-Z0-9._%+-]{3,}@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$",
        ErrorMessage = "Email must not start with a number and must have at least 3 characters before '@'")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{8,15}$",
            ErrorMessage = "Password must be between 8 and 15 characters and contain at least one lowercase letter," +
            " one uppercase letter, one digit and one special character.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [RegularExpression("^(Male|Female)$", ErrorMessage = "Gender must be either 'Male' or 'Female'.")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Specialty is required.")]
        public int SpecialtyId { get; set; }

        [Required(ErrorMessage = "ClinicName is required.")]
        public string ClinicName { get; set; }

        [Required(ErrorMessage = "Governorate is required.")]
        public int GovernorateId { get; set; }

        [Required(ErrorMessage = "Region is required.")]
        public int RegionId { get; set; }

        //[Required(ErrorMessage = "MedicalLicensePicture is required.")]
        public string? MedicalLicensePictureUrl { get; set; }

        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Consultation fees are required.")]
        [Range(0.0, 10000.0, ErrorMessage = "Consultation fees must be a positive number.")]
        public decimal ConsultationFees { get; set; }
    }
}
