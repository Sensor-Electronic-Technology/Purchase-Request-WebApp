using Microsoft.AspNetCore.HttpOverrides;
using Webapp;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);
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