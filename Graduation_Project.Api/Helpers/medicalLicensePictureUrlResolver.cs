using AutoMapper;
using AutoMapper.Execution;

namespace Graduation_Project.Api.Helpers
{
    public class medicalLicensePictureUrlResolver<TSource, TDestination> : IValueResolver<TSource, TDestination, string>
    {
        private readonly IConfiguration _configuration;

        public medicalLicensePictureUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(TSource source, TDestination destination, string destMember, ResolutionContext context)
        {
            // Get the PictureUrl property dynamically

            var MedicalLicensePictureUrlProperty = typeof(TSource).GetProperty("MedicalLicensePictureUrl");


            if (MedicalLicensePictureUrlProperty != null)
            {
                var medicalLicensePictureUrl = MedicalLicensePictureUrlProperty.GetValue(source) as string;
                if (!string.IsNullOrEmpty(medicalLicensePictureUrl))
                    return medicalLicensePictureUrl[0] == '/' ? $"{_configuration["AzureStorageUrl"]}{medicalLicensePictureUrl}" : $"{_configuration["AzureStorageUrl"]}/{medicalLicensePictureUrl}";
            }

            return string.Empty;
        }
    }
}
