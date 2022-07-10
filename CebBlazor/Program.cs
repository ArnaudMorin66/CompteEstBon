// Programme Blazor CompteEstBon
using CompteEstBon;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

var builder = WebApplication.CreateBuilder(args);
var svc = builder.Services;

// Add services to the container.
svc.AddRazorPages();
svc.AddServerSideBlazor();
svc.AddSyncfusionBlazor();
#pragma warning disable CS8604
svc.AddSingleton(new CebSetting {
    MongoDb = bool.Parse( builder.Configuration["mongodb:actif"]),
    MongoDbConnectionString = builder.Configuration["mongodb:server"]
});
svc.AddScoped<CebTirage>();
SyncfusionLicenseProvider.RegisterLicense(builder.Configuration["syncfusion:license"]);


var app = builder.Build();
// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
} else {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await app.RunAsync();

public class CebSetting {
    public bool MongoDb { get; set; }
    public string? MongoDbConnectionString { get; set; }
}