
using Graduation_Project.Core.IServices;
using Graduation_Project.Repository.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Graduation_Project.Service;

namespace Graduation_Project.Api.Extensions
{
    public static class IdentityServicesExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(IAuthService), typeof(AuthServices));
            services.AddHttpContextAccessor();
            services.AddScoped(typeof(IUserService), typeof(UserService));

            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IScheduleService, ScheduleService>();

            // add Identity Services configuration (UserManager , SigninManager , RoleManager)
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                //options.Password.RequiredUniqueChars = 2;
                //options.Password.RequireNonAlphanumeric = true;
                //options.Password.RequireUppercase = true;
                //options.Password.RequireLowercase = true;
                options.SignIn.RequireConfirmedEmail = true;

            }).AddEntityFrameworkStores<AppIdentityDbContext>();

            // AddAuthentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = true,
                    ValidAudience = configuration["JWT:ValidAudience"],
                    ValidateIssuer = true,
                    ValidIssuer = configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"])),
                    ValidateLifetime = true,
                    //ClockSkew = TimeSpan.FromDays(double.Parse(configuration["JWT:DurationInDays"])),
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // Allow JWT token via WebSockets
                        if (!string.IsNullOrEmpty(accessToken) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/Hubs/NotificationHub"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }
    }
}
