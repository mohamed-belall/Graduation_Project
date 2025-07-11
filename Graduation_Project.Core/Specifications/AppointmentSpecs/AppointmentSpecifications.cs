using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Project.Core.Specifications.AppointmentSpecs
{
    /**
     * 
     * 
     * Purpose: Basic spec to get appointments for a doctor starting from a certain date.

       Relation: Very similar to AppointmentsForSearchSpecifications but more general
       (no status filtering, no includes).

     **/


    public class AppointmentSpecifications : BaseSpecifications<Appointment>
    {
        public AppointmentSpecifications(int doctorId, DateOnly fromDate)
            : base(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate >= fromDate
            )
        {

        }
    }
}
