using Graduation_Project.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Secretary_Dashboard.MVC.ViewModel
{
    public class ConsultationFormVM
    {
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateOnly? DateOfBirth { get; set; }

        public Gender? Gender { get; set; }

        //[EmailAddress]
        //public string? Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? AppointmentDate { get; set; }

        public TimeOnly? AppointmentTime { get; set; }
    }

}

