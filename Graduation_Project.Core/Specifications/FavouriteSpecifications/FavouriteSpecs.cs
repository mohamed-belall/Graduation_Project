using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graduation_Project.Core.Specifications.DoctorSpecifications;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Core.Specifications.FavouriteSpecifications
{
    public class FavouriteSpecs : BaseSpecifications<Favorite>
    {
        public FavouriteSpecs(int doctorId,int patientId) 
                            : base (f => f.PatientId == patientId && f.DoctorId == doctorId)
        {
            
        }
        public FavouriteSpecs(int patientId) : base(f => f.PatientId == patientId)
        {

        }

        public FavouriteSpecs(int patientId, FavrouiteDoctorSpecParams favrouiteDoctorSpecParams) : base (f => f.PatientId == patientId)
        {
            ThenIncludes.Add(f => f.Include(f => f.Doctor).ThenInclude(d => d.WorkSchedules));
            ThenIncludes.Add(f => f.Include(f => f.Doctor).ThenInclude(d => d.ScheduleExceptions));
            ThenIncludes.Add(f => f.Include(f => f.Doctor).ThenInclude(d => d.Specialty));
            ThenIncludes.Add(f => f.Include(f => f.Doctor).ThenInclude(d => d.Clinic).ThenInclude(c => c.Region));
            ThenIncludes.Add(f => f.Include(f => f.Doctor).ThenInclude(d => d.Clinic).ThenInclude(c => c.Governorate));

            ApplyPagination((favrouiteDoctorSpecParams.PageIndex - 1) * favrouiteDoctorSpecParams.PageSize, favrouiteDoctorSpecParams.PageSize);
        }

    }
}
