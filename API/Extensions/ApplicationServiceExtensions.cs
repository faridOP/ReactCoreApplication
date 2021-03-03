using System;
using Application.Activities;
using Application.Core;
using Application.Interfaces;
using Infrastructure.Photos;
using Infrastructure.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<DataContext>(options =>
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                string connStr;

                // Depending on if in development or production, use either Heroku-provided
                // connection string, or development connection string from env var.
                if (env == "Development")
                {
                    // Use connection string from file.
                    connStr = config.GetConnectionString("DefaultConnection");
                }
                else
                {
                    // Use connection string provided at runtime by Heroku.
                    var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

                    // Parse connection URL to connection string for Npgsql
                    // connUrl = connUrl.Replace("postgres://", string.Empty);
                    // var pgUserPass = connUrl.Split("@")[0];
                    // var pgHostPortDb = connUrl.Split("@")[1];
                    // var pgHostPort = pgHostPortDb.Split("/")[0];
                    // var pgDb = pgHostPortDb.Split("/")[1];
                    // var pgUser = pgUserPass.Split(":")[0];
                    // var pgPass = pgUserPass.Split(":")[1];
                    // var pgHost = pgHostPort.Split(":")[0];
                    // var pgPort = pgHostPort.Split(":")[1];

                    connStr = "postgres://bxonhiovogwzrb:092ac0e130ce0d18f85aef0ef9a8773ccd37e3d748fa10a77bddb7c33fa38568@ec2-54-209-43-223.compute-1.amazonaws.com:5432/d5q7cp3m51h2ch";
                }

                // Whether the connection string came from the local development configuration file
                // or from the environment variable from Heroku, use it to set up your DbContext.
                options.UseNpgsql(connStr);
            });

            services.AddCors(options =>
                options.AddPolicy("CorsPolicy", policy =>
                    {
                        policy.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials() //5th and final step, to prevent CORS errors.
                        .WithOrigins("http://localhost:3000");
                    }

                )
            );
            services.AddMediatR(typeof(List.Handler).Assembly);
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);
            services.AddScoped<IUserAccessor, UserAccessor>();
            services.Configure<CloudinarySettings>(config.GetSection("Cloudinary"));
            services.AddScoped<IPhotoAccessor, PhotoAccessor>();
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            }); //2nd step after Creating ChatHub, 3rd step is in Startup.cs


            return services;
        }
    }
}