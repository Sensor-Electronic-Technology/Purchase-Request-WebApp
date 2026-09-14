using Microsoft.AspNetCore.HttpOverrides;
using Webapp;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);
builder.AddBlazorComponents();
builder.AddPurchaseRequestWebApp();
//Start
builder.Services.Configure<ForwardedHeadersOptions>(options => {
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Clear known networks and proxies so it accepts headers from your Kubernetes cluster
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
//End
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