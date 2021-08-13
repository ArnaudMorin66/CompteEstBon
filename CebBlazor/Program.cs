// Programme Blazor CompteEstBon
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Syncfusion.Blazor;
using CompteEstBon;
/*
namespace CompteEstBon {
    public class Program {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => {
                webBuilder.UseStartup<Startup>();

            }); 
    }
}
*/
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<CebTirage>();
builder.Services.AddSyncfusionBlazor();
// builder.Services.AddSingleton<WeatherForecastService>();
var cnf = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(cnf.GetSection("syncfusion").GetValue<string>("license"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
