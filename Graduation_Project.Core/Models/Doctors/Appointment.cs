using Graduation_Project.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduation_Project.Core.Models.Doctors
{
    public class Appointment : BaseEntity
    {
        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        [ForeignKey("Patient")]
        public int PatientId { get; set; } // Assuming a Patient entity exists
        public Patient Patient { get; set; }

        [Required]
        public DateOnly AppointmentDate { get; set; } // Stores the exact date of the appointment

        [Required]
        public TimeOnly AppointmentTime { get; set; } // Stores the booked slot time
        public int RescheduleCount { get; set; } = 0;  // Track the number of times the appointment has been rescheduled

        public AppointmentStatus Status { get; set; } // Track the status of the appointment (Pending, Confirmed, etc.)

        [ForeignKey("Policy")]
        public int PolicyId { get; set; }  // 🔗 Links to the DoctorPolicy
        public DoctorPolicy Policy { get; set; } = null!;
    }

    public enum AppointmentStatus
    {
        Pending   = 0,    // Waiting for payment
        Confirmed = 1,    // Approved by payment
        Cancelled = 2,    // Canceled by patient or doctor
        Completed = 3,    // Appointment was successfully completed

    }
}
