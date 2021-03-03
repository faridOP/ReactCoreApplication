using System.Text;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config){
                services.AddIdentityCore<AppUser>(options=>options.Password.RequireNonAlphanumeric=false)
                .AddEntityFrameworkStores<DataContext>()
                .AddSignInManager<SignInManager<AppUser>>();
                
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt=>{
                    opt.TokenValidationParameters= new TokenValidationParameters{
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,
                        ValidateIssuer = false,
                        ValidateAudience =false
                    };

                    opt.Events = new JwtBearerEvents //4th step, mapping access token to the parameter received from the url query, next is at ApplicationServiceExtensions, to prevent CORS policy error
                    {
                        OnMessageReceived = context=>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if(!string.IsNullOrEmpty(accessToken) && (path.StartsWithSegments("/chat")))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask; 
                        }
                    };
                });
                services.AddScoped<TokenService>();
                services.AddAuthorization(options=>options.AddPolicy("IsActivityHost", policy=>
                policy.Requirements.Add(new IsHostRequirement())));
                services.AddTransient<IAuthorizationHandler, IsHostRequirementHandler>();
                return services;
        }
    }
}