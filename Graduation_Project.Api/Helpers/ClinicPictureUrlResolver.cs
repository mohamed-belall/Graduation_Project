using AutoMapper;
using Graduation_Project.Api.DTO.Clinics;
using Graduation_Project.Api.DTO.Shared;

namespace Graduation_Project.Api.Helpers
{
    public class ClinicPictureUrlResolver : IValueResolver<Clinic, ClinicInfoToReturnDTO, ICollection<string>>
    {
        private readonly IConfiguration _configuration;

        public ClinicPictureUrlResolver(IConfiguration configuration)
        {
            this._configuration = configuration;
        }
   

        public ICollection<string> Resolve(Clinic source, ClinicInfoToReturnDTO destination, ICollection<string> destMember, ResolutionContext context)
        {
            if (source.ClinicPictures == null || !source.ClinicPictures.Any())
                return new List<string>();

            return source.ClinicPictures.Select(p => $"{_configuration["AzureStorageUrl"]}/{p.ImageUrl}").ToList();
        }
    }
}
