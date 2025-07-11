using Graduation_Project.Core.Models.Patients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Project.Core.Specifications.AppointmentSpecs
{
    /*
        
        Purpose: Filters appointments by PatientId, DoctorId, and exact AppointmentDate.

        Relation: Inherits from BaseSpecifications<Appointment>. 
        Likely used to check if a specific patient already has a booking on a given day with a given doctor.

     */
    public class AppointmentByPatientDoctorDateSpec : BaseSpecifications<Appointment>
    {
        public AppointmentByPatientDoctorDateSpec(int patientId, int doctorId, DateOnly date)
            : base(a => a.PatientId == patientId
                 && a.DoctorId == doctorId
                 && a.AppointmentDate == date
            )
        {

        }
    }
}
