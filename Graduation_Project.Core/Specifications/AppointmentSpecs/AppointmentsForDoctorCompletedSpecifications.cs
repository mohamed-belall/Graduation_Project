using Graduation_Project.Core.Models.Doctors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Project.Core.Specifications.AppointmentSpecs
{
    public class AppointmentsForDoctorCompletedSpecifications : BaseSpecifications<Appointment>
    {
        public AppointmentsForDoctorCompletedSpecifications(int doctorId, DateOnly today, bool getCancelled)
            : base(a =>
                a.DoctorId == doctorId
                &&
                (
                    a.AppointmentDate == today || a.AppointmentDate == today.AddDays(1)
                )
                &&
                (
                    getCancelled
                    ? a.Status == AppointmentStatus.Cancelled
                    : a.Status == AppointmentStatus.Completed
                )
            )
        {
            Includes.Add(a => a.Patient);
            AddOrderBy(a => a.AppointmentDate);
            AddThenOrderBy(a => a.AppointmentTime);
        }
    }
}
