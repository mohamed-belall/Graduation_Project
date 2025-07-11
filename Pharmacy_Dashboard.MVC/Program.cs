using Graduation_Project.Core.Models.Identity;
using Graduation_Project.Repository.Data;
using Graduation_Project.Repository.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pharmacy_Dashboard.MVC.Extensions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pharmacy_Dashboard.MVC
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            /****************************** Add Services to the container ********************************/
            builder.Services.AddControllersWithViews();

            #region Connection String

            /****************************** Connection String ********************************/

            //builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //{
            //    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            //});

            //builder.Services.AddDbContext<AppIdentityDbContext>(options =>
            //{
            //    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
            //});


            /****************************** Global Connection String ********************************/
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DeploymentDbGlobal"));
            });

            builder.Services.AddDbContext<AppIdentityDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DeploymentIdentityDbGlobal"));
            });

            #endregion


            /****************************** Add Application Services ********************************/
            builder.Services.AddApplicationServices();

            builder.Services.AddIdentityServices(builder.Configuration);

            builder.Services.AddSignalR();


            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"]))
                };
            });
            var app = builder.Build();

            app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

            #region Update-Database auto 
            var scope = app.Services.CreateScope();

            var services = scope.ServiceProvider;

            var applicationDbContext = services.GetRequiredService<ApplicationDbContext>();
            var _identityDbContext = services.GetRequiredService<AppIdentityDbContext>();


            //Create Object from ApplicationDbContext using CLR Exiplicitly

            var factoryLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            //Create Object from ILoggerFactory using CLR Exiplicitly

            try
            {
                await applicationDbContext.Database.MigrateAsync(); // for automatically update database
                await ApplicationDbContextSeed.SeedAsync(applicationDbContext); // for seeding entered data

                await _identityDbContext.Database.MigrateAsync(); // for automatically update database


                var _userManager = services.GetRequiredService<UserManager<AppUser>>(); // Ask CLR to create object from UserManager Explicitly
                await AppIdentityDbContextSeed.SeedUserAsync(_userManager); // for seeding entered data


                var _roleDbContext = services.GetRequiredService<RoleManager<IdentityRole>>();
                await RoleSeed.RoleSeedAsync(_roleDbContext); // for seeding entered role
            }
            catch (Exception ex)
            {
                var logger = factoryLogger.CreateLogger<Program>();
                logger.LogError(ex, "An Error Occured While update Database..");
            }
            #endregion

            #region Configure kestrel Middlewares

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            #region Auth
            app.UseSession();


            // rebuild User from JWT in session
            app.Use(async (context, next) =>
            {
                var token = context.Session.GetString("JWTToken");
                if (!string.IsNullOrEmpty(token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);

                    var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                    var principal = new ClaimsPrincipal(identity);

                    context.User = principal;
                }

                await next();
            });

            app.UseAuthentication();
            app.UseAuthorization(); 
            #endregion



            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}");

            #endregion

            app.Run();
        }
    }
}
