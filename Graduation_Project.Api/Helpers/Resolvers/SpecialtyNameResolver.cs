using AutoMapper;
using Graduation_Project.Api.DTO.Doctors;

namespace Graduation_Project.Api.Helpers.Resolvers
{
    public class SpecialtyNameResolver : IValueResolver<Doctor, SortingDoctorDto, string>
    {
        public string Resolve(Doctor source, SortingDoctorDto destination, string destMember, ResolutionContext context)
        {
            var lang = context.Items.ContainsKey("lang") ? context.Items["lang"].ToString().ToLower() : "en";
            if (source.Specialty == null) return null;
            return lang == "en" ? source.Specialty.Name_en : source.Specialty.Name_ar;

        }
    }
}
