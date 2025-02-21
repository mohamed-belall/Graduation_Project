﻿using Graduation_Project.Api.ErrorHandling;
using Graduation_Project.Core;
using Graduation_Project.Core.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Graduation_Project.Api.Filters
{
    public class ExistingIdFilter<T> : IAsyncActionFilter where T : BaseEntity
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExistingIdFilter(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionArguments.TryGetValue("id", out var idObj) && idObj is int id && id > 0)
            {
                var entity = await _unitOfWork.Repository<T>().GetAsync(id);
                if (entity == null)
                {
                    context.Result = new NotFoundObjectResult(new ApiResponse(StatusCodes.Status404NotFound, $"{typeof(T).Name} with ID {id} not found."));
                    return;
                }
            }
            else
            {
                context.Result = new BadRequestObjectResult(new ApiResponse(StatusCodes.Status400BadRequest, "Invalid ID format."));
                return;
            }

            await next();
        }
    }
}
