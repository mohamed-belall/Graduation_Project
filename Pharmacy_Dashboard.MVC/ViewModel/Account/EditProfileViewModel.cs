using Graduation_Project.Core.Models.Pharmacies;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_Dashboard.MVC.ViewModel.Account
{
    public class EditProfileViewModel
    {
        public string Email { get; set; }

        [StringLength(30, ErrorMessage = "Pharmacy name must be less than 100 characters")]
        [Display(Name = "Pharmacy Name")]
        public string PharmacyName { get; set; }
        public List<PharmacyContactViewModel>? PharmacyContacts { get; set; } = new();

        [StringLength(50, ErrorMessage = "Address must be less than 200 characters")]
        public string Address { get; set; }

        [Range(-90, 90, ErrorMessage = "Invalid latitude")]
        public double? Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Invalid longitude")]
        public double? Longitude { get; set; }
        public string? PictureUrl { get; set; }
        public IFormFile? ImageFile { get; set; }

    }
}
