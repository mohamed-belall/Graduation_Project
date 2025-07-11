using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Project.Core.Specifications.DoctorSpecifications
{
    public class DoctorWithCardsSpecs : BaseSpecifications<Doctor>
    {
        public DoctorWithCardsSpecs(int doctorId) : base(d => d.Id == doctorId)
        {
            Includes.Add(d => d.Favorites);
            Includes.Add(d => d.Appointments);
            Includes.Add(d => d.Feedbacks);
        }
    }
}
