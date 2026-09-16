using Microsoft.AspNetCore.HttpOverrides;
using Webapp;
using QuestPDF.Infrastructure;
using Webapp.Data;

QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<KestrelCustomSettings>(
    builder.Configuration.GetSection(KestrelCustomSettings.SectionName)
);

builder.Services.Configure<KestrelCustomSettings>(
    builder.Configuration.GetSection(KestrelCustomSettings.SectionName)
);
/*builder.Services.AddOptions<KestrelCustomSettings>()
    .Bind(builder.Configuration.GetSection(KestrelCustomSettings.SectionName))
    .ValidateDataAnnotations()
    .Validate(settings => {
        var certs = settings.Certificates?.Default;
        return certs == null || !certs.PathsExist();
    }).ValidateOnStart();*/

builder.AddBlazorComponents();
builder.AddPurchaseRequestWebApp();
var app = await builder.BuildApp();
//Start
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    // 2. CONFIGURATION: HTTP Strict Transport Security (HSTS)
    // Tells browsers to only use HTTPS for future requests.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseForwardedHeaders();
//End
app.Run();