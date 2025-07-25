﻿using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.Api.DTO.Account
{
    public class VerifyOTP
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "The Email field is not a valid e-mail address.")]
        public string Email { get; set; }


        [Required(ErrorMessage = "OTP Code is required.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be exactly 6 digits.")]
        public string OtpCode { get; set; }

        [Required(ErrorMessage = "OTP Type is required.")]
        [EnumDataType(typeof(OtpType), ErrorMessage = "Invalid OTP Type.")]
        public OtpType OtpType { get; set; }
    }
}
