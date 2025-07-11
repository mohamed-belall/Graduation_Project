using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Project.Core.Specifications.DoctorSpecifications
{
    public class DoctorWithClinicSpecification : BaseSpecifications<Doctor>
    {
        public DoctorWithClinicSpecification(int doctorId) : base (d => d.Id == doctorId)
        {
            Includes.Add(d => d.Clinic);
        }
    }
}
