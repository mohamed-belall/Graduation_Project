using AutoMapper;
using Graduation_Project.Api.DTO.Doctors;

namespace Graduation_Project.Api.Helpers.Resolvers
{
    public class SpecialtyNameResolverForAppointmentDetails : IValueResolver<Doctor, DoctorDetailsDto, string>
    {
        public string Resolve(Doctor source, DoctorDetailsDto destination, string destMember, ResolutionContext context)
        {
            var lang = context.Items.ContainsKey("lang") ? context.Items["lang"].ToString().ToLower() : "en";
            if (source.Specialty == null) return null;
            return (lang == "en") ? source.Specialty.Name_en : source.Specialty.Name_ar;

        }
    }
}
