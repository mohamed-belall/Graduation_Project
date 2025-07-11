using Graduation_Project.Api.ErrorHandling;
using Graduation_Project.APIs.Helpers;
using Graduation_Project.Core;
using Graduation_Project.Core.IRepositories;
using Graduation_Project.Core.IServices;
using Graduation_Project.Repository;
using Graduation_Project.Service;
using System.Threading.RateLimiting;


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
            services.AddScoped<IFileUploadService, AzureFileUploadService>();

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

            /****************************** Email Service ********************************/
            services.AddTransient<IEmailService, EmailService>();

            /****************************** Rate Limiting Middleware ********************************/
            services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    var response = new { Message = "Too many requests, please try again later."  };
                    await context.HttpContext.Response.WriteAsJsonAsync(response, token);
                };

                options.AddPolicy("OtpRateLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        key => new FixedWindowRateLimiterOptions 
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(10)
                        }));


                options.AddPolicy("LoginRateLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        key => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,  
                            Window = TimeSpan.FromMinutes(15) 
                        }));

                options.AddPolicy("PasswordLimiter", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        key => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,  
                            Window = TimeSpan.FromMinutes(10) 
                        }));

            });

            return services;
        }
    }
}
