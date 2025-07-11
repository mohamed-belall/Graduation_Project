namespace Graduation_Project.Repository.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Appointment> Appointment { get; set; }
        public DbSet<Favorite> Favorite { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<Prescription> Prescription { get; set; }

        public DbSet<UserOtpVerifications> UserOtpVerifications { get; set; }
        /*********************Doctors************************/
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Education> Educations { get; set; }
        public DbSet<WorkSchedule> WorkSchedule { get; set; }
        public DbSet<ScheduleException> scheduleExceptions { get; set; }
        public DbSet<DoctorPolicy> DoctorPolicies { get; set; }
        //public DbSet<DoctorClinic> DoctorClinics { get; set; }
        public DbSet<DoctorSubspeciality> DoctorSubspecialities { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<SubSpecialities> SubSpecialities { get; set; }


        /* --------------------- Pharmacies Models -----------------  */
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<Pharmacy> Pharmacies { get; set; }
        public DbSet<PharmacyOrder> PharmacyOrders { get; set; }
        public DbSet<PharmacyContact> PharmacyContacts { get; set; }
        // table created from M-M relationship
        public DbSet<PharmacyMedicineStock> PharmacyMedicineStocks { get; set; }
        public DbSet<MedicinePharmacyOrder> MedicinePharmacyOrders { get; set; }

        /* --------------------- Clinics Models -----------------  */
        public DbSet<Clinic> clinics { get; set; }

        public DbSet<ClinicPictures> clinicPictures { get; set; }
        //public DbSet<ClincSecretary> clincSecretaries { get; set; }
        public DbSet<ContactNumber> contactNumbers { get; set; }
        public DbSet<Secretary> secretaries { get; set; }
        public DbSet<TherapySession> therapySessions { get; set; }
        public DbSet<Governorate> governorates { get; set; }
        public DbSet<Region> regions { get; set; }


        /* --------------------- Notifications Models -----------------  */
        public DbSet<Notification> notifications { get; set; }
        public DbSet<NotificationRecipient> notificationRecipients { get; set; }

        /*********************Patients************************/
        public DbSet<AI_QuickDiagnosis> AI_QuickDiagnoses { get; set; }
        public DbSet<MedicalCategory> MedicalCategories { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<RadiologyReport> RadiologyReports { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClincSecretary>()
                .HasKey(cs => new { cs.ClincId, cs.SecretaryId });

            modelBuilder.Entity<ContactNumber>()
                .HasIndex(cn => new {cn.PhoneNumber })
                .IsUnique();
                

            //modelBuilder.Entity<DoctorClinic>()
            //    .HasKey(dc => new { dc.DoctorId, dc.ClinicId });

            modelBuilder.Entity<DoctorSubspeciality>()
                .HasKey(ds => new { ds.DoctorId, ds.SubSpecialitiesId });

            


            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.Property(d => d.ConsultationFees)
                      .HasColumnType("decimal(18,2)"); // specify precision and scale
            });


            modelBuilder.Entity<Clinic>()
                .HasMany(c => c.ClinicPictures)
                .WithOne(p => p.Clinic)
                .HasForeignKey(p => p.ClinicId);

            ///************** Relation with Identity Table **************/
            //modelBuilder.Entity<Doctor>()
            //.HasOne(d => d.ApplicationUser)
            //.WithOne()
            //.HasForeignKey<Doctor>(d => d.UserId);

            //modelBuilder.Entity<Patient>()
            //.HasOne(d => d.ApplicationUser)
            //.WithOne()
            //.HasForeignKey<Patient>(d => d.UserId);

            //modelBuilder.Entity<Pharmacist>()
            //.HasOne(d => d.ApplicationUser)
            //.WithOne()
            //.HasForeignKey<Pharmacist>(d => d.UserId);

            //modelBuilder.Entity<Secretary>()
            //.HasOne(d => d.ApplicationUser)
            //.WithOne()
            //.HasForeignKey<Secretary>(d => d.UserId);

            //Important Relationship
            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.WorkSchedules)
                .WithOne(ws => ws.Doctor)
                .HasForeignKey(ws => ws.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.ScheduleExceptions)
                .WithOne(se => se.Doctor)
                .HasForeignKey(se => se.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Appointments)
                .WithOne(a => a.Doctor)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Patient>()
                .HasMany(p => p.Appointments)
                .WithOne(a => a.Patient)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorPolicy>()
                .HasOne(dps => dps.Doctor)
                .WithMany(d => d.Policies)
                .HasForeignKey(dps => dps.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorPolicy>()
                .Property(dp => dp.PartialRefundPercentage)
                .HasPrecision(5, 2); // Adjust precision & scale as needed

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Policy)
                .WithMany()
                .HasForeignKey(a => a.PolicyId)
                .OnDelete(DeleteBehavior.SetNull); // Prevents cascade delete

            // Doctor-Active Policy Relationship
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.ActivePolicy)
                .WithMany()
                .HasForeignKey(d => d.ActivePolicyId)
                .OnDelete(DeleteBehavior.NoAction); // If policy is deleted, doctor should not break

            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Policies)
                .WithOne(dp => dp.Doctor)
                .HasForeignKey(dp => dp.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorPolicy>().HasData(
               new DoctorPolicy
               {
                   Id = 1,  // Default policy always has Id = 1
                   IsDefault = true,

                   // Cancellation Rules
                   AllowPatientCancellation = true,
                   MinCancellationHours = 24,
                   AllowLateCancellationReschedule = true,
                   MaxRescheduleAttempts = 1,

                   // Rescheduling Rules
                   AllowRescheduling = true,
                   MinRescheduleHours = 12,

                   // Refund & Payment Rules
                   AllowFullRefund = true,
                   AllowPartialRefund = true,
                   PartialRefundPercentage = 50,
                   RequirePrePayment = true,
                   UnpaidReservationTimeoutMinutes = 30,

                   // Doctor Availability
                   AllowMultipleBookingsPerDay = false,
                   MaxBookingsPerPatientPerDay = 1,
                   AllowLastMinuteBooking = true,
                   MinBookingAdvanceHours = 2,

                   // Use a static date to avoid model changes on each migration
                   CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
               }
            );


            /* --------------------------------------------- pharmacy ------------------------------*/
            //modelBuilder.Entity<PharmacyMedicineStock>()
            //    .HasKey(pm => new {pm.PharmacyId , pm.MedicineId});

            modelBuilder.Entity<PharmacyMedicineStock>()
                .HasOne(pm => pm.Pharmacy)
                .WithMany(p => p.pharmacyMedicineStocks)
                .HasForeignKey(pm => pm.PharmacyId);

            modelBuilder.Entity<PharmacyMedicineStock>()
                .HasOne(pm => pm.Medicine)
                .WithMany(m => m.pharmacyMedicineStocks)
                .HasForeignKey(pm => pm.MedicineId);

            modelBuilder.Entity<MedicinePharmacyOrder>()
                .HasOne(mpo => mpo.Medicine)
                .WithMany(m => m.MedicinePharmacyOrders)
                .HasForeignKey(mpo => mpo.MedicineId);

            modelBuilder.Entity<MedicinePharmacyOrder>()
               .HasOne(mpo => mpo.PharmacyOrder)
               .WithMany(m => m.MedicinePharmacyOrders)
               .HasForeignKey(mpo => mpo.PharmacyOrderId);




            base.OnModelCreating(modelBuilder);
        }

    }
}

