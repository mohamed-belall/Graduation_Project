using System.ComponentModel.DataAnnotations;
using Graduation_Project.Api.Attributes;

namespace Graduation_Project.Api.DTO.Doctors
{
    public class DoctorDetailsDto
    {
        public string FullName { get; set; } 

        public string? Description { get; set; }

        public decimal ConsultationFees { get; set; }

        public string? Specialty { get; set; }

        public double? Rating { get; set; }


        public string? MedicalLicensePictureUrl { get; set; }

        public string? PictureUrl { get; set; }

        public int? ExperianceYears { get; set; }

        public int? NumberOfPatients { get; set; }

        public bool? IsFavourite { get; set; }
    }
}
