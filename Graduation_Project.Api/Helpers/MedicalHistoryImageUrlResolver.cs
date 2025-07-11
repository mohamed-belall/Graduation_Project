using AutoMapper;
using Graduation_Project.Api.DTO.Patients;

namespace Graduation_Project.Api.Helpers
{
    public class MedicalHistoryImageUrlResolver : IValueResolver<MedicalHistory, MedicalHistoryFormDto, string>
    {
        private readonly IConfiguration _configuration;

        public MedicalHistoryImageUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Resolve(MedicalHistory source, MedicalHistoryFormDto destination, string destMember, ResolutionContext context)
        {
            var imageUrl = source.medicalHistoryImage?.ImageUrl;

            if (string.IsNullOrEmpty(imageUrl))
                return null;

            return imageUrl.StartsWith("/")
                ? $"{_configuration["AzureStorageUrl"]}{imageUrl}"
                : $"{_configuration["AzureStorageUrl"]}/{imageUrl}";
        }
    }
}
