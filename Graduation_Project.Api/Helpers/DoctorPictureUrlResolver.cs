using AutoMapper;
using Graduation_Project.Api.DTO.Shared;
using Graduation_Project.Core.Models;

namespace Graduation_Project.Api.Helpers
{
    public class DoctorPictureUrlResolver: IValueResolver<Appointment, AppointmentForPatientDto, string>
    {
        private readonly IConfiguration _configuration;

        public DoctorPictureUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(Appointment source, AppointmentForPatientDto destination, string destMember, ResolutionContext context)
        {
            if (source.Doctor != null && string.IsNullOrEmpty(source.Doctor.PictureUrl))
                return string.Empty;

            // Get Doctor's PictureUrl and MedicalLicensePictureUrl dynamically
            var pictureUrl = source.Doctor.PictureUrl;

            if (!string.IsNullOrEmpty(pictureUrl))
                return pictureUrl[0] == '/' ? $"{_configuration["AzureStorageUrl"]}{pictureUrl}" : $"{_configuration["AzureStorageUrl"]}/{pictureUrl}";

            return string.Empty;
        }
    }
    
}
