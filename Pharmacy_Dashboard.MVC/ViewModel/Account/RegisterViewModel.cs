using Graduation_Project.Core.Models.Pharmacies;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_Dashboard.MVC.ViewModel.Account
{
	public class RegisterViewModel
	{
		// enter full name 
		[Required(ErrorMessage = "Full Name is required.")]
		[StringLength(100, ErrorMessage = "Full Name cannot exceed 30 characters.")]
		[Display(Name = "Name")]
		public string FullName { get; set; }

		[Required(ErrorMessage = "Email is required")]
		[EmailAddress(ErrorMessage = "Invalid email address")]
        [RegularExpression("^(?!\\d)[a-zA-Z0-9._%+-]{3,}@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$",
		ErrorMessage = "Email must not start with a number and must have at least 3 characters before '@'")]
        public string Email { get; set; }

		[Required(ErrorMessage = "Password is required.")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{8,15}$",
			ErrorMessage = "Password must be between 8 and 15 characters and contain at least one lowercase letter," +
			" one uppercase letter, one digit and one special character.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
		[Compare(nameof(Password) ,ErrorMessage = "Confirm Password does not match Password "  )]
		[DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

		[Required(ErrorMessage = "Pharmacy name is required.")]
		[StringLength(100, ErrorMessage = "Pharmacy name cannot exceed 100 characters.")]
        [Display(Name = "Pharmacy Name")]
        public string PharmacyName { get; set; }

		[Required(ErrorMessage = "Pharmacy address is required.")]
        public string Address { get; set; }

		
		[Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
		public double Latitude { get; set; }

		[Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
		public double Longitude { get; set; }
		public string? LicenseImageUrl { get; set; }

        [Required(ErrorMessage = "Pharmacy license Image is required.")]
        [DataType(DataType.Upload)]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Pharmacy Contact")]
        public PharmacyContact? PharmacyContact { get; set; }

	}
}
