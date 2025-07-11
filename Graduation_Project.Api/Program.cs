using Graduation_Project.Api.Extensions;
using Graduation_Project.Api.Filters;
using Graduation_Project.Api.Middlewares;
using Graduation_Project.Core.IRepositories;
using Graduation_Project.Core.IServices;
using Graduation_Project.Repository;
using Graduation_Project.Repository.Identity;
using Graduation_Project.Service;
using Graduation_Project.Service.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace Graduation_Project.Api
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            #region Logging
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "Logs", "app.log"), rollingInterval: RollingInterval.Day)
                .CreateLogger();
            #endregion

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog();

            #region Configure Service

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials()
                           .SetIsOriginAllowed(origin => true);
                });
            });

            // Add services to the container.
            builder.Services.AddControllers();

            /****************************** Add Swagger Services********************************/
            builder.Services.AddSwaggerServices();

            #region Connection String (local | global)

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

            builder.Services.AddScoped(typeof(ExistingIdFilter<>));

            /****************************** Add Application Services ********************************/
            builder.Services.AddApplicationServices();

            builder.Services.AddIdentityServices(builder.Configuration);

            builder.Services.AddSignalR();

            #endregion

            var app = builder.Build();

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

            //Middlewares of Exception Handling 
            app.UseMiddleware<ExceptionMiddleware>();

            // Not found Endpoint
            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwaggerMiddleware();
            //}

            app.UseStaticFiles();

            app.UseStaticFiles();

          

            app.MapControllers();

            app.MapHub<NotificationHub>("/Hubs/NotificationHub"); // Correct SignalR mapping

            app.UseCors("CorsPolicy");
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            #endregion

            app.Run();
        }
    }
}
