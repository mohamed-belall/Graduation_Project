﻿using Graduation_Project.Api.ErrorHandling;
using Graduation_Project.APIs.Helpers;
using Graduation_Project.Core;
using Graduation_Project.Core.IRepositories;
using Graduation_Project.Core.IServices;
using Graduation_Project.Repository;
using Graduation_Project.Service;

namespace Graduation_Project.Api.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            services.AddLogging(config =>
            {
                config.AddConsole(); // Enables console logging
                config.AddDebug();   // Enables debug output
            });

            /****************************** Generic Respository Register ********************************/
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            //unitOfWork replaces GenericRepository
            //this line is equivalent to the following line

            //services.AddScoped(typeof(GenericNoBaseEntityRepository<>));
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            /****************************** notification Services ********************************/
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IFileUploadService, FileUploadService>();

            /****************************** Pharmacy Services ********************************/
            services.AddScoped<IPharmacyService, PharmacyService>();
            /****************************** add services for Fcm ********************************/
            services.AddScoped<IFcmService,FcmNotificationService>();

            /****************************** add services for AutoMapper ********************************/
            services.AddAutoMapper(typeof(MappingProfiles)); 
            //Can Use That: services.AddAutoMapper(M => M.AddProfile(MappingProfiles));

            /****************************** add services for Validation Error ********************************/
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = ActionContext =>
                {
                    var errors = ActionContext.ModelState
                                              .Where(p => p.Value.Errors.Count() > 0)
                                              .SelectMany(p => p.Value.Errors)
                                              .Select(e => e.ErrorMessage).ToArray();
                    var ValidationErrorResponse = new ApiValidationErrorResponse()
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(ValidationErrorResponse);
                };
            });

            return services;
        }
    }
}
