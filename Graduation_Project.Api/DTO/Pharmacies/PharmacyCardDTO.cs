﻿using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.Api.DTO.Pharmacies
{
    public class PharmacyCardDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? PictureUrl { get; set; }

        public string Distance { get; set; }

        public string? Address { get; set; }

        public string Location { get; set; }

        public ICollection<PharmacyContactReturnDTO>? Contacts { get; set; }
    }
}
