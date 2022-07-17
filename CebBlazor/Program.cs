//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//     Author:  
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// Programme Blazor CompteEstBon


using CebBlazor.Code;
using CompteEstBon;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

var builder = WebApplication.CreateBuilder(args);
var svc = builder.Services;

// Add services to the container.
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
SyncfusionLicenseProvider.RegisterLicense(builder.Configuration["license"]);


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
#pragma warning disable CRR0029
app.Run();
