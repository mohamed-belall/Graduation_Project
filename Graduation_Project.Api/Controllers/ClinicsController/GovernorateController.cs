using AutoMapper;
using Graduation_Project.Api.DTO.Clinics;
using Graduation_Project.Api.ErrorHandling;
using Graduation_Project.Core.IRepositories;
using Graduation_Project.Core.Models.Clinics;
using Graduation_Project.Core.Specifications.ClinicsSpecifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_Project.Api.Controllers.ClinicsController
{

    public class GovernorateController : BaseApiController
    {
        private readonly IGenericRepository<Governorate> _governorateRepo;
        private readonly IMapper _mapper;

        public GovernorateController(
            IGenericRepository<Governorate> governorateRepo,
            IMapper mapper
            )
        {
            _governorateRepo = governorateRepo;
            this._mapper = mapper;
        }
        /***************************** End point to get governorate with its Regions *****************************/
        [HttpGet("GovernorateWithRegions")]
        public async Task<ActionResult<IReadOnlyList<GovernorateDTO>>> GetAllGovernorateWithRegions([FromQuery] string? lang = "ar")
        {
            if (lang.ToLower() != "ar" && lang.ToLower() != "en")
            {
                return BadRequest(new ApiResponse(400, "Invalid Language"));
            }
            var spec = new GovernorateWithRegionsSpecification();
            var governates = await _governorateRepo.GetAllWithSpecAsync(spec , g => new GovernorateDTO
            {
                Id = g.Id,
                Name = lang.ToLower() == "ar"? g.Name_ar : g.Name_en,
                Regions = g.regions.Select(r => new RegionDTO
                {
                    Id = r.Id,
                    Name = lang.ToLower() == "ar" ? r.Name_ar : r.Name_en
                }).ToList()
            });
            return Ok(governates);
        }


    }
}
