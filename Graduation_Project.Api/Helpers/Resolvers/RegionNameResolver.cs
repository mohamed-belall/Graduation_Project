using AutoMapper;
using Graduation_Project.Api.DTO.Doctors;

namespace Graduation_Project.Api.Helpers.Resolvers
{
    public class RegionNameResolver : IValueResolver<Doctor, SortingDoctorDto, string>
    {
        public string Resolve(Doctor source, SortingDoctorDto destination, string destMember, ResolutionContext context)
        {
            var lang = context.Items.ContainsKey("lang") ? context.Items["lang"].ToString().ToLower() : "en";
            if (source.Clinic == null) return null;
            return (lang == "en") ? source.Clinic.Region.Name_en : source.Clinic.Region.Name_ar;

        }
    }
}
