using AutoMapper;
using Graduation_Project.Api.DTO.Pharmacies;
using Graduation_Project.Service.HelperModels;

namespace Graduation_Project.Api.Helpers
{
    public class PharmacyPictureUrlResolver : IValueResolver<PharmacyWithDistances, PharmacyCardDTO, string>
    {
        private readonly IConfiguration _configuration;

        public PharmacyPictureUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(PharmacyWithDistances source, PharmacyCardDTO destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.pharmacy.ProfilePictureUrl))
            {
                return source.pharmacy.ProfilePictureUrl[0] == '/' ? $"{_configuration["AzureStorageUrl"]}{source.pharmacy.ProfilePictureUrl}" : $"{_configuration["AzureStorageUrl"]}/{source.pharmacy.ProfilePictureUrl}";
            }
            return null;
        }
    }
}

