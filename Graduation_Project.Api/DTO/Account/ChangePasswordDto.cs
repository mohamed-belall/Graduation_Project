﻿using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.Api.DTO.Account
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Old Password is required.")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New Password is required.")]
        [DataType(DataType.Password)]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^\\da-zA-Z]).{8,15}$",
            ErrorMessage = "Password must be between 8 and 15 characters and contain at least one lowercase letter," +
            " one uppercase letter, one digit and one special character.")]
        public string NewPassword { get; set; }
    }
}
