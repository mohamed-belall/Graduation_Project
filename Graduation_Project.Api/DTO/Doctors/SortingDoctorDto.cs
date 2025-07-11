using System.ComponentModel.DataAnnotations;

namespace Graduation_Project.Api.DTO.Doctors
{
    public class SortingDoctorDto
    {
        public int Id { get; set; }      //Doctor: id
        public string FullName { get; set; } 
        public decimal ConsultationFees { get; set; }
        public string Specialty { get; set; }
        public string? PictureUrl { get; set; }
        public double Rating { get; set; }
        public string? Region { get; set; }
        public string Governorate { get; set; }
        public string? Availability { get; set; }
    }
}
