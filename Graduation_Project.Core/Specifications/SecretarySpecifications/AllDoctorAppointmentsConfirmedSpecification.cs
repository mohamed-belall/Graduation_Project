using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Project.Core.Specifications.SecretarySpecifications
{
    /*
     * Purpose: Gets all appointments for a doctor for the current day only.

       Relation: Useful for the secretary's dashboard or doctor’s agenda for today.

       Includes:
     
       -Patient
     
       Ordering: By date and then time.


     */
    public class AllDoctorAppointmentsConfirmedSpecification :BaseSpecifications<Appointment>
    {
        public AllDoctorAppointmentsConfirmedSpecification(int Doctorid) : base(A =>

        A.AppointmentDate == DateOnly.FromDateTime(DateTime.Now) &&
        A.DoctorId == Doctorid && A.Status != AppointmentStatus.Completed ) // مواعيد النهاردة  بس ال لسه مدخلتش للدكتور
        {
            Includes.Add(a => a.Patient); // Include patient details
            AddOrderBy(a => a.AppointmentDate); // Order by date
            AddThenOrderBy(a => a.AppointmentTime); // Then order by time
        }
    }
}
