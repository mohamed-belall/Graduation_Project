using AutoMapper;
using Graduation_Project.Api.DTO;
using Graduation_Project.Api.DTO.Clinics;
using Graduation_Project.Api.DTO.Doctors;
using Graduation_Project.Api.DTO.FeedBacks;
using Graduation_Project.Api.DTO.Notification;
using Graduation_Project.Api.DTO.Orders;
using Graduation_Project.Api.DTO.Patients;
using Graduation_Project.Api.DTO.Pharmacies;
using Graduation_Project.Api.DTO.Shared;
using Graduation_Project.Api.Helpers;
using Graduation_Project.Api.Helpers.Resolvers;
using Graduation_Project.Core.Models.Doctors;
using Graduation_Project.Service.HelperModels;
using Microsoft.Extensions.Configuration;
using System.Globalization;
namespace Graduation_Project.APIs.Helpers
{
    public class MappingProfiles : Profile
    {
        private readonly IConfiguration _configuration;

        public MappingProfiles(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public MappingProfiles()
        {
            CreateMap<Person, PersonToReturnDTO>()
                .ForMember(d => d.PictureUrl, O => O.MapFrom<PictureUrlResolver<Person, PersonToReturnDTO>>());

            /****************************************** Mapping for Doctor Profile ******************************************/

            CreateMap<Doctor, DoctorForProfileDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    src.FirstName + ' ' + src.LastName
                ))
                .ForMember(dest => dest.PictureUrl, opt => opt.MapFrom<PictureUrlResolver<Doctor, DoctorForProfileDto>>());

            CreateMap<Doctor, DoctorForProfileToReturnDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    src.FirstName + ' ' + src.LastName
                )).ForMember(dest => dest.PictureUrl, opt => opt.MapFrom<PictureUrlResolver<Doctor, DoctorForProfileToReturnDto>>());


            CreateMap<DoctorForProfileDto, Doctor>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src =>
                    src.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]
                ))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src =>
                    src.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 2
                    ? string.Join(" ", src.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1))
                    : src.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]
                ))
                .ForMember(dest => dest.PictureUrl, opt =>
                {
                    opt.MapFrom(src => src.PictureUrl);
                    opt.Condition(src => !string.IsNullOrEmpty(src.PictureUrl));
                })
                ;

            /****************************************** Mapping for Patient Profile ******************************************/

            CreateMap<Patient, PatientForProfileDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    src.FirstName + " " + src.LastName
                ))
                .ForMember(dest => dest.PictureUrl, opt => opt.MapFrom<PictureUrlResolver<Patient, PatientForProfileDto>>());

            CreateMap<Patient, PatientForProfileToReturnDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    src.FirstName + " " + src.LastName
                ))
                .ForMember(dest => dest.PictureUrl, opt => opt.MapFrom<PictureUrlResolver<Patient, PatientForProfileToReturnDto>>());


            CreateMap<PatientForProfileDto, PatientForProfileToReturnDto>();

            CreateMap<PatientForProfileDto, Patient>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src =>
                    src.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0]
                ))

                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src =>
                    src.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 2
                    ? string.Join(" ", src.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1))
                    : src.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]
                ))
                .ForMember(dest => dest.PictureUrl, opt =>
                {
                    opt.MapFrom(src => src.PictureUrl);
                    opt.Condition(src => !string.IsNullOrEmpty(src.PictureUrl));
                })
                ;

            /****************************************** Mapping for Home ******************************************/
            CreateMap<Doctor, SortingDoctorDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    src.FirstName + ' ' + src.LastName
                ))
                .ForMember(dest => dest.Specialty, opt => opt.MapFrom<SpecialtyNameResolver>())
                .ForMember(dest => dest.Governorate, opt => opt.MapFrom<GovernNameResolver>())
                .ForMember(dest => dest.Region, opt => opt.MapFrom<RegionNameResolver>())
                .ForMember(dest => dest.PictureUrl, opt => opt.MapFrom<PictureUrlResolver<Doctor, SortingDoctorDto>>())
                .ForMember(dest => dest.Availability, opt => opt.MapFrom<AvailabilityResolver>());

            /****************************************** Mapping for Doctor From Patient ******************************************/

            CreateMap<Doctor, DoctorDetailsDto>()
               .ForMember(dest => dest.Specialty, opt => opt.MapFrom<SpecialtyNameResolverForAppointmentDetails>())
               .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                    src.FirstName + ' ' + src.LastName
                ))
               .ForMember(dest => dest.PictureUrl, opt => opt.MapFrom<PictureUrlResolver<Doctor, DoctorDetailsDto>>());

            /****************************************** Mapping for Education& Clinic ******************************************/
            CreateMap<Education, EducationDto>();

            CreateMap<EducationDto, Education>();

            CreateMap<ClinicAboutDto, DoctorAboutDto>();

            CreateMap<Education, DoctorAboutDto>();

            /****************************************** Clinic ******************************************/
            CreateMap<Clinic, ClinicInfoToReturnDTO>()
                .ForMember(dest => dest.RegionName, opt => opt.MapFrom(src => src.Region.Name_en))
                .ForMember(dest => dest.GovernorateName, opt => opt.MapFrom(src => src.Governorate.Name_en))
                .ForMember(dest => dest.GovernorateId, opt => opt.MapFrom(src => src.Governorate.Id))
                .ForMember(dest => dest.contactNumbers, opt => opt.MapFrom(src => src.ContactNumbers != null ? src.ContactNumbers.Select(cn => cn.PhoneNumber).ToList() : new List<string>()))
                .ForMember(dest => dest.ClinicPictures, opt => opt.MapFrom<ClinicPictureUrlResolver>());

            CreateMap<ClinicEditDTO, Clinic>();
            //.ForMember(dest => dest.GovernorateId, opt => opt.Ignore()); // GovernorateId might be inferred from Region, or handled manually

            CreateMap<ContactNumber, ContactNumberDTO>();

            /****************************************** Mapping for Medicl History ******************************************/
            CreateMap<MedicalHistory, MedicalHistoryDto>()
                .ForMember(dest => dest.MedicalImage, opt => opt.MapFrom<MedicalHistoryDTOImageUrlResolver>())
                .ForMember(dest => dest.Date, O => O.MapFrom(src => src.Date.ToString("yyyy-MM-dd")));

            CreateMap<MedicalHistoryFormDto, MedicalHistory>();

            CreateMap<MedicalHistory, MedicalHistoryFormDto>()
                 .ForMember(dest => dest.MedicalImage, opt => opt.MapFrom<MedicalHistoryImageUrlResolver>());

            /****************************************** Mapping for Feedback ******************************************/
            CreateMap<FeedbackDto, Feedback>();
            CreateMap<Feedback, FeedbackInfoDto>();
            CreateMap<Feedback, FeedbackWithIdToReturnDto>();



            CreateMap<Feedback, FeedbackToReturnDto>()
                    .ForMember(dest => dest.FullName, opt => opt.MapFrom(src =>
                        src.patient.FirstName + ' ' + src.patient.LastName
                    ))
                    .ForMember(dest => dest.Age, opt => opt.MapFrom(src =>
                        src.patient.DateOfBirth.HasValue
                            ? (DateTime.Today.Year - src.patient.DateOfBirth.Value.Year) -
                              (DateTime.Today.DayOfYear < src.patient.DateOfBirth.Value.DayOfYear ? 1 : 0)
                            : (int?)null
                    ));

            /****************************************** Governorate ******************************************/

            CreateMap<Governorate, GovernorateDTO>();

            /****************************************** Region ******************************************/

            CreateMap<Region, RegionDTO>();

            /****************************************** Prescription ******************************************/
            CreateMap<Prescription, PrescriptionEditFormDto>()
                .ForMember(dest => dest.MedicinePrescriptions, opt => opt.MapFrom(src => src.MedicinePrescriptions));

            CreateMap<Prescription, PrescriptionListViewFormDto>()
                .ForMember(dest => dest.IssuedDate,
               opt => opt.MapFrom(src => src.IssuedDate.ToString("dd MMM yyyy hh:mm tt")));

            CreateMap<Prescription, PrescriptionListViewFormForPatientDto>()
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src =>
                    src.Doctor.FirstName + ' ' + src.Doctor.LastName
                ))
                .ForMember(dest => dest.Specialty, opt => opt.MapFrom(src =>
                    src.Doctor.Specialty != null ? src.Doctor.Specialty.Name_en : null
                ))
                .ForMember(dest => dest.IssuedDate,
               opt => opt.MapFrom(src => src.IssuedDate.ToString("dd MMM yyyy hh:mm tt")));

            CreateMap<PrescriptionImageDTO, PrescriptionImage>();

            /****************************************** Medicine ******************************************/
            CreateMap<MedicinePrescriptionDto, MedicinePrescription>()
            .ForMember(dest => dest.PrescriptionId, opt => opt.Ignore()) // Ignore PrescriptionId
            .ForMember(dest => dest.Prescription, opt => opt.Ignore())   // Ignore Prescription
            .ForMember(dest => dest.Medicine, opt => opt.Ignore())       // Ignore Medicine
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details))
            .ReverseMap();


            CreateMap<Prescription, PrescriptionResponseDTO>()
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId))
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.FirstName + " " + src.Patient.LastName))
                .ForMember(dest => dest.PatientAge, opt => opt.MapFrom(src => src.Patient.DateOfBirth.HasValue ? (int?)(DateTime.Now.Year - src.Patient.DateOfBirth.Value.Year) : null))
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.FirstName + " " + src.Doctor.LastName))
                .ForMember(dest => dest.IssuedDate, opt => opt.MapFrom(src => src.IssuedDate.ToString("dd MMM yyyy hh:mm tt")));


            CreateMap<PrescriptionImage, PrescriptionImageDTO>();

            CreateMap<MedicinePrescription, MedicinePrescriptionResponseDTO>()
             .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine.Name_en))
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Medicine.Id))
             .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details));

            /****************************************** WorkSchedule ******************************************/
            CreateMap<WorkScheduleFromUserDto, WorkSchedule>();
            CreateMap<WorkSchedule, WorkScheduleFromDatabaseDto>();

            /****************************************** ScheduleException ******************************************/
            CreateMap<ScheduleExceptionFromUserDto, ScheduleException>();
            CreateMap<ScheduleException, ScheduleExceptionFromDatabaseDto>();

            /****************************************** Appointment ******************************************/
            CreateMap<BookAppointmentDto, Appointment>()
                .ForMember(dest => dest.AppointmentTime, opt => opt.Ignore()); ;

            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => $"{src.Patient.FirstName} {src.Patient.LastName}"))
                .ForMember(dest => dest.AppointmentTime, opt => opt.MapFrom(src => src.AppointmentTime.ToString("HH:mm:ss")))
                .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src => src.AppointmentDate.ToString("yyyy-MM-dd")))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString())) // Convert enum to string
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src =>
                        src.Patient.DateOfBirth.HasValue
                            ? (DateTime.Today.Year - src.Patient.DateOfBirth.Value.Year) -
                              (DateTime.Today.DayOfYear < src.Patient.DateOfBirth.Value.DayOfYear ? 1 : 0)
                            : (int?)null
                ))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Patient.Gender.ToString()));

            CreateMap<Appointment, AppointmentForPatientDto>()
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => $"{src.Doctor.FirstName} {src.Doctor.LastName}"))
                .ForMember(dest => dest.AppointmentTime, opt => opt.MapFrom(src => src.AppointmentTime.ToString("HH:mm:ss")))
                .ForMember(dest => dest.AppointmentDate, opt => opt.MapFrom(src => src.AppointmentDate.ToString("yyyy-MM-dd")))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString())) // Convert enum to string
                .ForMember(dest => dest.Specialty, opt => opt.MapFrom(src =>
                    src.Doctor.Specialty != null ? src.Doctor.Specialty.Name_ar : null
                ))
                .ForMember(dest => dest.ConsultationFees, opt => opt.MapFrom(src => src.Doctor.ConsultationFees))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Doctor.Rating))
                .ForMember(dest => dest.PictureUrl, opt => opt.MapFrom<DoctorPictureUrlResolver>());

            /****************************************** Pharmacy ******************************************/
            CreateMap<PharmacyWithDistances, PharmacyCardDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.pharmacy.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.pharmacy.Name))
                .ForMember(dest => dest.PictureUrl, opt => opt.MapFrom<PharmacyPictureUrlResolver>())
                .ForMember(dest => dest.Distance, opt => opt.MapFrom(src => $"{src.Distance:F1} Km"))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => $"https://www.google.com/maps?q={src.pharmacy.Latitude},{src.pharmacy.Longitude}"))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.pharmacy.Address))
                .ForMember(dest => dest.Contacts, opt => opt.MapFrom(src => src.pharmacy.pharmacyContacts.Count == 0
                    ? null
                    : src.pharmacy.pharmacyContacts.Select(p => new PharmacyContactReturnDTO { PhoneNumber = p.PhoneNumber })));

            CreateMap<MedicinePrescription, SearchMedicinesResponseDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.MedicineId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Medicine.Name_en))
                .ForMember(dest => dest.DosageForm, opt => opt.MapFrom(src => src.Medicine.DosageForm))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Medicine.Price));

            /****************************************** Order ******************************************/
            CreateMap<Pharmacy, OrderViewDTO>()
                .ForMember(dest => dest.PharmacyName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.PharmacyAddress, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.PharmacyPhoneNumber, opt => opt.MapFrom(src => src.pharmacyContacts.Select(p => p.PhoneNumber).ToList()))
                .ForMember(dest => dest.PharamcyPictureUrl, opt => opt.MapFrom<PictureUrlResolver<Pharmacy, OrderViewDTO>>());


            /****************************************** Notification ******************************************/
            CreateMap<NotificationRecipient, NotificationDto>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Notification.Title))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Notification.Message))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.Notification.CreatedDate))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => src.IsRead));

        }
    }
}