using System.Security.Claims;
using AutoMapper;
using Graduation_Project.Api.DTO;
using Graduation_Project.Api.DTO.Doctors;
using Graduation_Project.Api.DTO.FeedBacks;
using Graduation_Project.Api.ErrorHandling;
using Graduation_Project.Api.Filters;
using Graduation_Project.Api.Helpers;
using Graduation_Project.APIs.Helpers;
using Graduation_Project.Core;
using Graduation_Project.Core.Constants;
using Graduation_Project.Core.IRepositories;
using Graduation_Project.Core.IServices;
using Graduation_Project.Core.Models.Doctors;
using Graduation_Project.Core.Specifications.DoctorSpecifications;
using Graduation_Project.Core.Specifications.FavouriteSpecifications;
using Graduation_Project.Core.Specifications.FeedBackSpecifications;
using Graduation_Project.Core.Specifications.PatientSpecifications;
using Graduation_Project.Repository;
using Graduation_Project.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Talabat.API.Dtos.Account;

namespace Graduation_Project.Api.Controllers.DoctorControllers
{
    public class DoctorController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IFileUploadService _fileUploadService;
        private readonly IPatientService _patientService;
        private readonly IUnitOfWork _unitOfWork;

        public DoctorController(UserManager<AppUser> userManager,
                                IUnitOfWork unitOfWork,
                                IMapper mapper,
                                IFileUploadService fileUploadService,
                                IPatientService patientService
            )
        {
            _userManager = userManager;
            _mapper = mapper;
            _fileUploadService = fileUploadService;
            _patientService = patientService;
            _unitOfWork = unitOfWork;
        }

        [Authorize(Roles = nameof(UserRoleType.Doctor))]
        [HttpGet("GetProfile")]
        public async Task<ActionResult<DoctorForProfileToReturnDto>> GetDoctorProfile()
        {
            // Get Current Doctor Id
            var DoctorId = int.Parse(User.FindFirstValue(Identifiers.DoctorId));
            //Get Doctor From Doctor Table in business DB
            var doctor = await _unitOfWork.Repository<Doctor>().GetAsync(DoctorId);
            if (doctor == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound));

            var email = User.FindFirstValue(ClaimTypes.Email);
            // Map to DoctorForProfileDto
            var doctorForProfileToReturnDto = new DoctorForProfileToReturnDto()
            {
                Email = email,
            };

            doctorForProfileToReturnDto = _mapper.Map(doctor, doctorForProfileToReturnDto);

            return Ok(doctorForProfileToReturnDto);
        }

        [Authorize(Roles = nameof(UserRoleType.Doctor))]
        [HttpPut("EditProfile")]
        public async Task<ActionResult<DoctorForProfileToReturnDto>> EditDoctorProfile(DoctorForProfileDto doctorDtoFromRequest)
        {
            // Get Current Doctor Id
            var doctorId = int.Parse(User.FindFirstValue(Identifiers.DoctorId));

            //Get Doctor From Doctor Table in business DB
            var doctor = await _unitOfWork.Repository<Doctor>().GetAsync(doctorId);
            if (doctor == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound));

            
            // upload Doctor Picture and save its relative path in database
            if (doctorDtoFromRequest.PictureFile != null )
            {

                var (uploadSuccess, uploadMessage, uploadedPicUrlFilePath) = await _fileUploadService.UploadFileAsync(doctorDtoFromRequest.PictureFile, "Doctor/ProfilePic", User);
                if(!uploadSuccess)
                {
                    return BadRequest(new ApiResponse(400, uploadMessage));
                }

                doctorDtoFromRequest.PictureUrl = uploadedPicUrlFilePath;
            }

            // mapping 
            doctor = _mapper.Map(doctorDtoFromRequest, doctor);

            // Update Business DB
            _unitOfWork.Repository<Doctor>().Update(doctor);
            await _unitOfWork.Repository<Doctor>().SaveAsync();

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail is null || string.IsNullOrEmpty(userEmail))
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "User Not Found"));

            var appUser = await _userManager.FindByEmailAsync(userEmail);
            if(appUser is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "User not found"));

            appUser.FullName = doctorDtoFromRequest.FullName;
            await _userManager.UpdateAsync(appUser);


           var  doctorForProfileToReturnDto = _mapper.Map<Doctor, DoctorForProfileToReturnDto>(doctor);

            doctorForProfileToReturnDto.Email = userEmail;

            return Ok(doctorForProfileToReturnDto);

        }

        [Authorize(Roles = nameof(UserRoleType.Doctor))]
        [HttpGet("GetReviewsAndRating")]
        public async Task<ActionResult<RatingAndReviews>> GetReviewsAndRating()
        {
            // Get Current Doctor Id
            var DoctorId = int.Parse(User.FindFirstValue(Identifiers.DoctorId));

            var specs = new DoctorWithReviewsSpecs(DoctorId);
            var doctor = await _unitOfWork.Repository<Doctor>().GetWithSpecsAsync(specs);
            if (doctor == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound));

            var ratingAndReviews = new RatingAndReviews() { Rating = doctor.Rating , Reviews = doctor.Feedbacks?.Count()};

            return Ok(ratingAndReviews);
        }

        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpGet("DoctorWithFilter")]
        public async Task<ActionResult<Pagination<SortingDoctorDto>>> GetDoctorsAsync([FromQuery] DoctorSpecParams specParams, [FromQuery] string? lang = "en")
        {
            if (lang.ToLower() != "ar" && lang.ToLower() != "en")
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Language Not Supported"));
            

            IReadOnlyList<Doctor>? doctors;
            int count;
            if (specParams.RegionId.HasValue || specParams.GovernorateId.HasValue)
            {
                // filter the doctors based on the region & governorate 
                if (!specParams.GovernorateId.HasValue && specParams.RegionId.HasValue)
                    return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "RegionId Must Have GovernorateId"));

                // apply pagination after fetching data from the dataBase that have the includes
                var doctorSpecs = new SortingDoctorWithSpecificaitonWithOutPagination(specParams);
                doctors = await _unitOfWork.Repository<Doctor>().GetAllWithSpecAsync(doctorSpecs);
                doctors = FilteredDoctors(doctors, specParams.RegionId, specParams.GovernorateId);
                count = doctors.Count;
                // apply pagination 
                doctors = doctors.Skip((specParams.PageIndex - 1) * specParams.PageSize).Take(specParams.PageSize).ToList();
            }
            else
            {
                var doctorSpecification = new SortingDoctorWithSpecificaiton(specParams);
                doctors = await _unitOfWork.Repository<Doctor>().GetAllWithSpecAsync(doctorSpecification);
                var countSpec = new DoctorWithFilterCountSpecification(specParams);
                count = await _unitOfWork.Repository<Doctor>().GetCountAsync(countSpec);
            }

            var data = _mapper.Map<IReadOnlyList<SortingDoctorDto>>(doctors, opts =>
            {
                opts.Items["lang"] = lang ?? "en";
                opts.Items["AvailabilityFilter"] = specParams.Availability;
            });

            return Ok(new Pagination<SortingDoctorDto>(specParams.PageIndex, specParams.PageSize, count, data));
        }

        //**************************************************** Doctor From Patient ****************************************************//

        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpGet("GetDetailsDuringAppointment/{id:int}")]
        [ServiceFilter(typeof(ExistingIdFilter<Doctor>))]
        public async Task<ActionResult<DoctorDetailsDto>> GetDoctorDetailsDuringAppointment(int id , [FromQuery] string? lang = "en")
        {
            if (lang.ToLower() != "ar" && lang.ToLower() != "en")
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Language Not Supported"));


            //Get Doctor From Doctor Table in business DB
            DoctorDetailsSpecs doctorSpecification = new DoctorDetailsSpecs(id);
            var doctor = await _unitOfWork.Repository<Doctor>().GetWithSpecsAsync(doctorSpecification);
            if (doctor == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound));

            // Get Current Patient To Know the doctor is Favourite or not
            var PatientId = int.Parse(User.FindFirstValue(Identifiers.PatientId));

            // Map to doctorDetailsDto
            var doctorDetailsDto = new DoctorDetailsDto()
            {
                NumberOfPatients = doctor.Appointments.Count(),
                IsFavourite = await CheckFavouriteDoctor(PatientId, doctor.Id)  // check in favourite table 
            };

            doctorDetailsDto = _mapper.Map(doctor, doctorDetailsDto ,opts => opts.Items["lang"] = lang ?? "en");

            return Ok(doctorDetailsDto);
        }


        // check in favourite table 
        private async Task<bool> CheckFavouriteDoctor(int patientid , int docId)
        {
            var favouriteSpecs = new FavouriteSpecs(docId, patientid);
            var favouriteDoctor = await _unitOfWork.Repository<Favorite>().GetWithSpecsAsync(favouriteSpecs);
            if (favouriteDoctor == null)
                return false;

            return true;
        }

        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpGet("GetAbout/{id:int}")]
        [ServiceFilter(typeof(ExistingIdFilter<Doctor>))]
        public async Task<ActionResult<DoctorAboutDto>> GetDoctorAbout(int id)
        {
            // Get Doctor From Db include  education , clinics
            var specs = new DoctorWithEducationAndClinicsSpecs(id);
            var doctorFromDb = await _unitOfWork.Repository<Doctor>().GetWithSpecsAsync(specs);

            // Check if The doctor Is Exist ??
            if (doctorFromDb == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound));

            if (doctorFromDb.Clinic == null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound,"Doctor clinic not found"));
            // Map To DoctorAboutDto
            var doctorAboutDto = new DoctorAboutDto();

            // Assign Doctor Info (Description & Subspeciality)
            doctorAboutDto.Description = doctorFromDb.Description;
            doctorAboutDto.DoctorSubspeciality = doctorFromDb.DoctorSubspeciality.Select(d => d.SubSpecialities.Name_en).ToList();

            // Assign Doctor Education
            doctorAboutDto = _mapper.Map(doctorFromDb.Education, doctorAboutDto);

            // Assign Doctor Clinics List
            //doctorAboutDto = _mapper.Map(doctorFromDb.Clinic, doctorAboutDto);
            //doctorAboutDto = _mapper.Map(doctorFromDb.Clinic, doctorAboutDto.Clinic);
            doctorAboutDto.Name = doctorFromDb.Clinic.Name;
            doctorAboutDto.Location = doctorFromDb.Clinic.Address;
            doctorAboutDto.LocationLink = doctorFromDb.Clinic.LocationLink;

            doctorAboutDto.PictureUrls = doctorFromDb.Clinic.ClinicPictures is not null ?
                doctorFromDb.Clinic.ClinicPictures.Select(p => p.ImageUrl).ToList() :
                new List<string>();
                

            return Ok(doctorAboutDto);
        }

        [Authorize(Roles = nameof(UserRoleType.Doctor))]
        [HttpGet("PatientInfo/{patientId:int}")]
        public async Task<ActionResult<object>> GetPatientInfo(int patientId)
        {
            var patient = await _patientService.GetInfo(patientId, null);
            if (patient is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound));

            return Ok(patient);
        }

        [Authorize(Roles = nameof(UserRoleType.Doctor))]
        [HttpGet("HomeCards")]
        public async Task<ActionResult<DoctorCountDto>> GetDoctorCards()
        {
            var doctorId = int.Parse(User.FindFirstValue(Identifiers.DoctorId));

            var spec = new DoctorWithCardsSpecs(doctorId);
            var doctor = await _unitOfWork.Repository<Doctor>().GetWithSpecsAsync(spec);
            if (doctor is null)
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "Doctor not found"));

            var doctorCountDto = new DoctorCountDto
            {
                TotalFavourite = doctor.Favorites.Count(),
                TotalReviews = doctor.Feedbacks.Count(),
                TotalAppointments = doctor.Appointments.Count()
            };

            return Ok(doctorCountDto);
        }

        private IReadOnlyList<Doctor>? FilteredDoctors(IReadOnlyList<Doctor> doctors, int? regionId, int? governorateId)
        {
            if (regionId == null && governorateId == null)
                return doctors;

            if (governorateId.HasValue && !regionId.HasValue)
                doctors = doctors.Where(d => d.Clinic?.GovernorateId == governorateId).ToList();
            else
                doctors = doctors.Where(d => (d.Clinic?.GovernorateId == governorateId) && (d.Clinic?.RegionId == regionId)).ToList();

            return doctors;
        }

    }

}


