using AutoMapper;
using Graduation_Project.Core.Models.Pharmacies;
using Pharmacy_Dashboard.MVC.ViewModel.Account;
using Pharmacy_Dashboard.MVC.ViewModel.OrderViewModels;
using Pharmacy_Dashboard.MVC.ViewModel.Stock;

namespace Pharmacy_Dashboard.MVC.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            /****************************************** Mapping for Pharmacy Register ******************************************/
            CreateMap<RegisterViewModel, Pharmacy>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.PharmacyName))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude))
                .ForMember(dest => dest.LicenseImageUrl, opt => opt.MapFrom(src => src.LicenseImageUrl));

            /****************************************** Mapping for Pharmacy Edit info ******************************************/
            CreateMap<Pharmacy, EditProfileViewModel>()
                .ForMember(dest => dest.PharmacyName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.PictureUrl, opt => opt.MapFrom<PictureUrlResolver<Pharmacy, EditProfileViewModel>>())
                .ForMember(dest => dest.PharmacyContacts, opt => opt.MapFrom(src => src.pharmacyContacts.Select(c => new PharmacyContactViewModel
                {
                    Id = c.Id,
                    PhoneNumber = c.PhoneNumber,
                })));


            CreateMap<EditProfileViewModel, Pharmacy>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.PharmacyName))
                .ForMember(dest => dest.ProfilePictureUrl, opt => opt.MapFrom(src => src.PictureUrl))
                .ForMember(dest => dest.pharmacyContacts, opt => opt.Ignore());




            /****************************************** Mapping for Stock ******************************************/
            CreateMap<PharmacyMedicineStock, PharmacyStockViewModel>()
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine.Name_en))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Medicine.Price));

            // ========================================== Pharmacy Order ==========================================
            CreateMap<PharmacyOrder, OrderContent>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.FirstName + " " + src.Patient.LastName));

            CreateMap<MedicinePharmacyOrder, OrderMedicine>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.MedicineId))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Medicine.Price))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Medicine.Name_en));

            CreateMap<PharmacyOrder, OrderCardViewModel>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.FirstName + " " + src.Patient.LastName))
                .ForMember(dest => dest.PatientPhone, opt => opt.MapFrom(src => src.Patient.PhoneNumber))
                .ForMember(dest => dest.OrderMedicines, opt => opt.MapFrom(src => src.MedicinePharmacyOrders));
        }
    }
}