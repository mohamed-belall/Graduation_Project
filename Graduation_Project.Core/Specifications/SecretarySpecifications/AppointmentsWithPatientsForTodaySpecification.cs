using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Project.Core.Specifications.SecretarySpecifications
{
    public class AppointmentsWithPatientsForTodaySpecification : BaseSpecifications<Appointment>
    {
        public AppointmentsWithPatientsForTodaySpecification(DateOnly date)
            : base(a => a.AppointmentDate == date)
        {
            Includes.Add(a => a.Patient);
           
        }
    }

}
