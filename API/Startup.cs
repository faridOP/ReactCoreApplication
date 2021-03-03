using API.Extensions;
using API.Middleware;
using API.SignalR;
using Application.Activities;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
                //
            services.AddControllers(opt=>{
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                opt.Filters.Add(new AuthorizeFilter(policy));
            }) //
            .AddFluentValidation(config =>
            config.RegisterValidatorsFromAssemblyContaining<Create>()
            );
            services.AddApplicationServices(Configuration);
            services.AddIdentityServices(Configuration);
           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseMiddleware<ExceptionMiddleware>();
            // if (env.IsDevelopment())
            // {
            //     app.UseDeveloperExceptionPage();
            // }

            // app.UseHttpsRedirection();
            

            app.UseXContentTypeOptions();

            app.UseReferrerPolicy(opt=>opt.NoReferrer());

            app.UseXXssProtection(opt=>opt.EnabledWithBlockMode());

            app.UseXfo(opt=>opt.Deny());

            app.UseCsp(opt=>opt.BlockAllMixedContent()
            .StyleSources(x=>x.Self().CustomSources("https://fonts.googleapis.com","sha256-2aahydUs+he2AO0g7YZuG67RGvfE9VXGbycVgIwMnBI="))
            .FontSources(s=>s.Self().CustomSources("https://fonts.gstatic.com","data:"))
            .FormActions(f=>f.Self())
            .ImageSources(i=>i.Self().Self().CustomSources("https://res.cloudinary.com","https://www.facebook.com","data:","https://platform-lookaside.fbsbx.com/"))
            .ScriptSources(s=>s.Self().CustomSources("sha256-nmPlCby3y/E/OHNr4hli4CG+i6G5FdrCP7Yzm5zxmXc=","https://connect.facebook.net","sha256-UQc3PyCIYx5gcby7Xb5JAO250UddjMeYC8nfTn3/Gx0="))
            );

            if (env.EnvironmentName!="Development"){
                app.Use(async (context, next)=>{
                    context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
                    await next.Invoke();
                });
            }

            app.UseRouting();

            app.UseDefaultFiles();

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseCors("CorsPolicy");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chat"); //3rd step,adding an endpoint, 4th step is in IdentityServiceExtensions
                endpoints.MapFallbackToController("Index","Fallback");
            });

            // app.Run(async context=>{
            //     await context.Response.WriteAsync(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            // });
        }
    }
}
