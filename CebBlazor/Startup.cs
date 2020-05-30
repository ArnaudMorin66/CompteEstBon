using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Ini;
using Microsoft.Extensions.FileProviders;
using Syncfusion.Blazor;
using System;

namespace CompteEstBon {
    public class Startup {
        static Startup() {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(configuration.GetSection("syncfusion").GetValue<string>("license"));
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            services.AddRazorPages(); 
            services.AddScoped<CompteEstBon.CebTirage>();
            services.AddServerSideBlazor();
            services.AddSyncfusionBlazor();
            // RegisterLicense();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints => {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
