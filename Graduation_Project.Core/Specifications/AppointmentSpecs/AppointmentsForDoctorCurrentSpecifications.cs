using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Project.Core.Specifications.AppointmentSpecs
{
    public class AppointmentsForDoctorCurrentSpecifications : BaseSpecifications<Appointment>
    {
        public AppointmentsForDoctorCurrentSpecifications(int doctorId, DateOnly todayDate, TimeOnly nowTime)
            : base(a =>
                a.DoctorId == doctorId 
                &&
                a.AppointmentTime >= nowTime 
                &&
                (
                    a.AppointmentDate == todayDate || a.AppointmentDate == todayDate.AddDays(1)
                )
            )
        {
            Includes.Add(a => a.Patient);
            AddOrderBy(a => a.AppointmentDate);
            AddThenOrderBy(a => a.AppointmentTime);
        }
    }
}
