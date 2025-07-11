
using Graduation_Project.Core.IServices;
using Graduation_Project.Repository.Identity;
using System.Text;
using Graduation_Project.Service;
using Graduation_Project.Core.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace Pharmacy_Dashboard.MVC.Extensions
{
    public static class IdentityServicesExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(IAuthService), typeof(AuthServices));
            services.AddHttpContextAccessor();
            services.AddScoped(typeof(IUserService), typeof(UserService));
            
            services.AddScoped(typeof(IPatientService), typeof(PatientServcie));

            // add Identity Services configuration (UserManager , SigninManager , RoleManager)
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

            })
                    .AddEntityFrameworkStores<AppIdentityDbContext>()
                    .AddDefaultTokenProviders()
                    .AddTokenProvider<DataProtectorTokenProvider<AppUser>>("Email");

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Home/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(14); 
                options.SlidingExpiration = true;
            });

            return services;
        }
    }
}
