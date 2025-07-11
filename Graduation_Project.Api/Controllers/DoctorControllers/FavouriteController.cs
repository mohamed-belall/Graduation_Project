using Graduation_Project.Core.IServices;
using Graduation_Project.Core;
using Graduation_Project.Core.Specifications.FavouriteSpecifications;
using Graduation_Project.Core.Specifications.PatientSpecifications;
using Graduation_Project.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Graduation_Project.Api.Attributes;
using Graduation_Project.Api.ErrorHandling;
using Graduation_Project.Api.Filters;
using Graduation_Project.Core.Constants;
using System.Security.Claims;
using AutoMapper;
using Graduation_Project.Api.Helpers;
using Graduation_Project.Core.Specifications.DoctorSpecifications;
using Graduation_Project.Api.DTO.Doctors;
using Microsoft.AspNetCore.SignalR;

namespace Graduation_Project.Api.Controllers.DoctorControllers
{
    public class FavouriteController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly INotificationService notificationService;
        private readonly IMapper _mapper;

        public FavouriteController(IUnitOfWork unitOfWork
                                , IUserService userService 
                                , INotificationService notificationService
                                , IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.notificationService = notificationService;
            _mapper = mapper;
        }

        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpGet("{id:int}")]
        [ServiceFilter(typeof(ExistingIdFilter<Doctor>))]
        public async Task<ActionResult<bool>> GetFavourite(int id)
        {
            // get current patient Id
            var patientId = int.Parse(User.FindFirstValue(Identifiers.PatientId));

            // get favourite by (doctorId , patientId)
            // check if is it exist or not 
            var favouriteSpecs = new FavouriteSpecs(id, patientId);
            var favouriteDoctor = await unitOfWork.Repository<Favorite>().GetWithSpecsAsync(favouriteSpecs);
            if (favouriteDoctor == null)
                return Ok(false);

            return Ok(true);
        }


        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpGet]
        public async Task<ActionResult<Pagination<SortingDoctorDto>>> GetAllFavourites([FromQuery] FavrouiteDoctorSpecParams favrouiteDoctorSpecParams , string? lang = "en")
        {
            if (lang.ToLower() != "ar" && lang.ToLower() != "en")
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Language Not Supported"));

            // get current patient
            var patientId = int.Parse(User.FindFirstValue(Identifiers.PatientId));

            // get favourite record where patientid == patient.id includes doctor
            var specs = new FavouriteSpecs(patientId, favrouiteDoctorSpecParams);
            var favorites = await unitOfWork.Repository<Favorite>().GetAllWithSpecAsync(specs); // part of all records according to pagination 

            if (favorites is null || !favorites.Any())
                return NotFound(new ApiResponse(StatusCodes.Status404NotFound, "No Favourite Doctors Found"));

            // get doctors from the previous query
            IReadOnlyList<Doctor> favouriteDoctors = favorites.Select(f => f.Doctor).ToList();

            // get Count of all favourite Doctors
            var countSpecs = new FavouriteSpecs(patientId);
            var count = await unitOfWork.Repository<Favorite>().GetCountAsync(countSpecs);

            // mapping to SortingDoctorDto
            var data = _mapper.Map<IReadOnlyList<Doctor>, IReadOnlyList<SortingDoctorDto>>(favouriteDoctors, opts =>
            {
                opts.Items["lang"] = lang ?? "en";
                opts.Items["AvailabilityFilter"] = null;
            });

            return Ok(new Pagination<SortingDoctorDto>(favrouiteDoctorSpecParams.PageIndex, favrouiteDoctorSpecParams.PageSize, count, data));
        }

        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpPost("{id:int}")]
        [ServiceFilter(typeof(ExistingIdFilter<Doctor>))]
        public async Task<ActionResult<bool>> AddFavourite(int id)
        {
            // get current patient Id
            var patientId = int.Parse(User.FindFirstValue(Identifiers.PatientId));

            // get favourite to this patientId , DoctorId to check if this is already exist or not
            var specs = new FavouriteSpecs(id, patientId);
            var favouriteFromDb = await unitOfWork.Repository<Favorite>().GetWithSpecsAsync(specs);
            if (favouriteFromDb != null)
                return BadRequest(new ApiResponse(StatusCodes.Status400BadRequest, "Doctor is already Favourite for the current patient"));

            // add favourite record
            var favourite = new Favorite
            {
                DoctorId = id,
                PatientId = patientId,
            };

            await unitOfWork.Repository<Favorite>().AddWithSaveAsync(favourite);

            // Push New Notification for the doctor
            var doctor = await unitOfWork.Repository<Doctor>().GetAsync(id);

            await notificationService.SendNotificationAsync(doctor.ApplicationUserId, "A New Patient Add you to favourite", "New Favourite");

            return Ok(new ApiResponse(StatusCodes.Status201Created,"Created Successfully"));
        }

        [Authorize(Roles = nameof(UserRoleType.Patient))]
        [HttpDelete("{id:int}")]
        [ServiceFilter(typeof(ExistingIdFilter<Doctor>))]
        public async Task<ActionResult<bool>> RemoveFavourite(int id)
        {
            // get current patient Id
            var patientId = int.Parse(User.FindFirstValue(Identifiers.PatientId));

            // get favourite to this patientId , DoctorId to check if this is already exist or not
            var specs = new FavouriteSpecs(id, patientId);
            var favourite = await unitOfWork.Repository<Favorite>().GetWithSpecsAsync(specs);
            if (favourite == null)
                return NotFound(new ApiResponse(StatusCodes.Status400BadRequest, "Doctor is not Favourite for the current patient"));
            // remove favourite record

            unitOfWork.Repository<Favorite>().Delete(favourite);
            await unitOfWork.Repository<Favorite>().SaveAsync();

            return Ok(new ApiResponse(StatusCodes.Status200OK, "Removed Successfully"));
        }


     
    }
}
