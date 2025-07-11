using AutoMapper;
using Graduation_Project.Core;
using Graduation_Project.Core.IRepositories;
using Graduation_Project.Core.IServices;
using Graduation_Project.Repository;
using Graduation_Project.Service;
using Microsoft.AspNetCore.Mvc;
using Pharmacy_Dashboard.MVC.Helpers;

namespace Pharmacy_Dashboard.MVC.Extensions
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

            /****************************** add services for AutoMapper ********************************/
            services.AddAutoMapper(typeof(MappingProfiles));

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            

            services.AddScoped(typeof(IEmailService), typeof(EmailService));
            //services.AddScoped(typeof(INotificationService), typeof(NotificationService));

            services.AddScoped(typeof(IFileUploadService), typeof(AzureFileUploadService));

            services.AddScoped<INotificationService,NotificationService>();

            services.AddScoped<IFcmService, FcmNotificationService>();

            return services;
        }
    }
}
