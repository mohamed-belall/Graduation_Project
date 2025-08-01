﻿namespace Graduation_Project.Api.DTO.Clinics
{
    public class ClinicInfoToReturnDTO
    {

        //public int Id { get; set; }
        public string Name { get; set; } // Name of the clinic
        //public string? PictureUrl { get; set; }
        public string? LocationLink { get; set; } //  locationLink of the clinic
        //public double Latitude { get; set; }  // Latitude coordinate
        //public double Longitude { get; set; }  // Longitude coordinate

        public string? Address { get; set; }     //  Address or location of the clinic

        
        public ClinicType? Type { get; set; }
     
        public int RegionId { get; set; }
        public String RegionName { get; set; }


        public int GovernorateId { get; set; }
        public string GovernorateName { get; set; }

        public ICollection<string>? ClinicPictures { get; set; }

        public ICollection<string>? contactNumbers { get; set; }

    }
}
