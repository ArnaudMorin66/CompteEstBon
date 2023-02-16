//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using CebBlazor.Code;
using CebBlazor.Properties;

using CompteEstBon;

using Syncfusion.Blazor;
using Syncfusion.Licensing;

var builder = WebApplication.CreateBuilder(args);
var svc = builder.Services;

svc.AddRazorPages();
svc.AddServerSideBlazor();
svc.AddSyncfusionBlazor();

svc.AddSingleton(
    new CebSetting
    {
        MongoDb = bool.TryParse(builder.Configuration["mongodb:actif"], out var mdb) && mdb,
        MongoDbConnectionString = builder.Configuration["mongodb:server"],
        AutoCalcul = bool.TryParse(builder.Configuration["AutoCalcul"], out var res) && res
    });
svc.AddScoped<CebTirage>();
SyncfusionLicenseProvider.RegisterLicense(licenseKey: Resources.Licence);


var app = builder.Build();
if(!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/index");
await app.RunAsync();