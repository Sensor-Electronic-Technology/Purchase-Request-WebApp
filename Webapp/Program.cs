using Webapp;
using QuestPDF.Infrastructure;
using Webapp.Data;

QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);
builder.ConfigurePurchaseRequest();
builder.AddBlazorComponents();
builder.AddPurchaseRequestWebApp();
var app = await builder.BuildApp();
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseForwardedHeaders();
app.Run();



/*builder.Services.Configure<KestrelCustomSettings>(
    builder.Configuration.GetSection(KestrelCustomSettings.SectionName)
);

builder.Services.Configure<KestrelCustomSettings>(
    builder.Configuration.GetSection(KestrelCustomSettings.SectionName)
);*/
/*builder.Services.AddOptions<KestrelCustomSettings>()
    .Bind(builder.Configuration.GetSection(KestrelCustomSettings.SectionName))
    .ValidateDataAnnotations()
    .Validate(settings => {
        var certs = settings.Certificates?.Default;
        return certs == null || !certs.PathsExist();
    }).ValidateOnStart();*/