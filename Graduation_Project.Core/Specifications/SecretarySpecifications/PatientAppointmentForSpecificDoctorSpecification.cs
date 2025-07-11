using Graduation_Project.Core.Models.Patients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Project.Core.Specifications.AppointmentSpecs
{
    /*
        
        Purpose: Filters appointments by PatientId, DoctorId.

        Relation: Inherits from BaseSpecifications<Appointment>. 
        Likely used to check if a specific patient already has a booking on a given day with a given doctor.

     */
    public class PatientAppointmentForSpecificDoctorSpecification : BaseSpecifications<Appointment>
    {
        public PatientAppointmentForSpecificDoctorSpecification(int AppointmentId)
            : base(a => a.Id == AppointmentId)

            
        {
            Includes.Add(a => a.Patient); // Include patient details
            AddOrderBy(a => a.AppointmentDate); // Order by date
            AddThenOrderBy(a => a.AppointmentTime); // Then order by time

        }
    }
}
