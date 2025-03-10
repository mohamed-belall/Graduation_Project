﻿using AutoMapper;
using Graduation_Project.Api.DTO;
using Graduation_Project.Api.ErrorHandling;
using Graduation_Project.Core;
using Graduation_Project.Core.Specifications.SpecialitySpecifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Graduation_Project.Api.Controllers
{
    public class SpecialityController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public SpecialityController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }
        /***************************** End point to get speciality with its sub speciality *****************************/
        [HttpGet("GetAllSpecialityWithSubspeciality")]
        public async Task<ActionResult<IReadOnlyList<SpecialityDTO>>> GetAllSpecialityWithSubspeciality([FromQuery]string? lang = "ar")
        {
            if(lang.ToLower() != "ar" && lang.ToLower() != "en")
            {
                return BadRequest(new ApiResponse(400, "Invalid Language"));
            }

            var spec = new SpecialityWithSubSpecialitySpecification();
            var specialities = await unitOfWork.Repository<Specialty>().GetAllWithSpecAsync(spec , s => new SpecialityDTO
            {
                Id = s.Id,
                Name = lang.ToLower() == "ar" ? s.Name_ar : s.Name_en,
                SubSpecialities = s.SubSpecialities.Select(ss => new SubSpecialityDTO
                {
                    Id = ss.Id,
                    Name = lang.ToLower() == "ar" ? ss.Name_ar : ss.Name_en
                }).ToList()
            });
            return Ok(specialities);
        }
    }
}
