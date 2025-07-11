using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Project.Core.Specifications.AppointmentSpecs
{
    public class AppointmentsForDoctorSearchSpecifications: BaseSpecifications<Appointment>
    {
        public AppointmentsForDoctorSearchSpecifications(AppointmentSpecParams specParams)
            : base(a =>
                a.DoctorId == specParams.Id
            &&
                (string.IsNullOrEmpty(specParams.Search) ||
                    (
                        !string.IsNullOrEmpty(specParams.FirstNameSearch) &&
                        (
                            (string.IsNullOrEmpty(specParams.LastNameSearch) &&
                                (a.Patient.FirstName.ToLower().Contains(specParams.FirstNameSearch) || a.Patient.LastName.ToLower().Contains(specParams.FirstNameSearch))
                            )
                            ||
                            (!string.IsNullOrEmpty(specParams.LastNameSearch) &&
                                a.Patient.FirstName.ToLower().Contains(specParams.FirstNameSearch) &&
                                a.Patient.LastName.ToLower().Contains(specParams.LastNameSearch)
                            )
                        )
                    )
                ) 
            &&
                (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Completed) // Only booked appointments
            )
        {
            Includes.Add(a => a.Patient); // Include patient details
            AddOrderBy(a => a.AppointmentDate); // Order by date
            AddThenOrderBy(a => a.AppointmentTime); // Then order by time
        }
    }
}
