using acme_resolver.Repository;
using acme_resolver.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace acme_resolver
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IChallengeTokenRepository,TokenMemoryRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                var repository = app.ApplicationServices.GetRequiredService<IChallengeTokenRepository>();               

                endpoints.MapGet("/.well-known/acme-challenge/{token}",async context =>{
                    var token = context.Request.RouteValues["token"].ToString();
                    var acmeRequest = await repository.GetKeyByToken(token);
                    Log.Debug("Received challenge token {@token}",token);
                    await context.Response.WriteAsync(acmeRequest?.Key??"");
                });

                 endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Acme Resolver");
                });
            });
        }
    }
}
