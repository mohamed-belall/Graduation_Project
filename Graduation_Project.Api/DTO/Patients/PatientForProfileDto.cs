using Graduation_Project.Api.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.Api.DTO.Patients
{
    public class PatientForProfileDto
    {
        [FullName(ErrorMessage = "Full name must contain at least two words.")] // Fname + Lname
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(maximumLength: 100, MinimumLength = 3, ErrorMessage = "Full Name must be at least 3 characters and at most 100 characters.")]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }

        [PastDate(ErrorMessage = "Date of Birth must be in the past.")]
        public DateOnly? DateOfBirth { get; set; }

        public string? PictureUrl { get; set; }

        public IFormFile? PictureFile { get; set; }


        [ValidEnumValue<BloodType>(ErrorMessage = "Invalid value for BloodTrpe.")]
        public BloodType? BloodType { get; set; }
    }
}
