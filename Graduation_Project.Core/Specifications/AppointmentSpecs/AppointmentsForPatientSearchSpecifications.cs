using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Graduation_Project.Core.Specifications.AppointmentSpecs
{


    /*
     Purpose: Fetches appointments for a patient from a specific date up to a month later.

     Includes:
     
     -Doctor
     
     -Doctor's specialty (nested include)
     
     Ordering: By date and then time.
     
     Relation: Extends BaseSpecifications<Appointment> and used when showing upcoming appointments for patients.


     
     */
    public class AppointmentsForPatientSearchSpecifications: BaseSpecifications<Appointment>
    {
        public AppointmentsForPatientSearchSpecifications(int patientId, DateOnly date)
            : base(a =>
                a.PatientId == patientId
            )
        {
            Includes.Add(a => a.Doctor); // ✅ Includes Doctor
            ThenIncludes.Add(q => q.Include(d => d.Doctor.Specialty)); // ✅ Includes Specialty inside Doctor
            AddOrderBy(a => a.AppointmentDate); // Order by date
            AddThenOrderBy(a => a.AppointmentTime); // Then order by time
        }
    }
}
