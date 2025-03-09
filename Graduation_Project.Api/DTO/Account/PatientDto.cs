﻿namespace Graduation_Project.Api.DTO.Account
{
    public class PatientDto
    {
        public string FullName { get; set; }

        public string Email { get; set; }

        public string Token { get; set; }

        public string Role { get; set; }

        public string? PictureUrl { get; set; }
    }
}
