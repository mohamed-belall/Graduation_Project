using System.ComponentModel.DataAnnotations;

namespace Pharmacy_Dashboard.MVC.ViewModel.Account
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "New Password is required.")]
        [Display(Name = "New Password")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{8,15}$",
    ErrorMessage = "Password must be between 8 and 15 characters and contain at least one lowercase letter," +
    " one uppercase letter, one digit and one special character.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Display(Name = "Confirm New Password")]
        [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

    }
}
