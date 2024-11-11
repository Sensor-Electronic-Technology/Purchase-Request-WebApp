using BlazorDownloadFile;
using Blazored.LocalStorage;
using Domain;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MongoDB.Driver;
using Radzen;
using Webapp.Components;
using Webapp.Services.Authentication;
using Infrastructure;
using Infrastructure.Hubs;
using Infrastructure.Services;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using QuestPDF.Infrastructure;
using SetiFileStore.FileClient;
using Webapp.Services;

QuestPDF.Settings.License = LicenseType.Community;
/*DevExpress.XtraPrinting.PrintingOptions.Pdf.RenderingEngine = DevExpress.XtraPrinting.XRPdfRenderingEngine.Skia;*/
var builder = WebApplication.CreateBuilder(args);
// Setup logging to be exported via OpenTelemetry
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

var otel = builder.Services.AddOpenTelemetry();
otel.WithMetrics(metrics =>
{
    // Metrics provider from OpenTelemetry
    metrics.AddAspNetCoreInstrumentation();
    metrics.AddRuntimeInstrumentation();
    metrics.AddConsoleExporter();
    // Metrics provides by ASP.NET Core in .NET 8
    metrics.AddMeter("Microsoft.AspNetCore.Hosting");
    metrics.AddMeter("Microsoft.AspNetCore.Server.Kestrel");
});

// Add Tracing for ASP.NET Core and our custom ActivitySource and export via OTLP
otel.WithTracing(tracing => {
    tracing.AddAspNetCoreInstrumentation();
    tracing.AddConsoleExporter();
});

// Export OpenTelemetry data via OTLP, using env vars for the configuration
var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
if (otlpEndpoint != null) {
    otel.UseOtlpExporter();
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options => options.DetailedErrors = true)
    .AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);

builder.Services.AddRadzenCookieThemeService(options => {
    options.Name = "app-theme"; // The name of the cookie
    options.Duration = TimeSpan.FromDays(365); // The duration of the cookie
});

builder.Services.AddDevExpressBlazor(options => {
    options.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5;
    options.SizeMode = DevExpress.Blazor.SizeMode.Small;
});
builder.Services.AddLocalization();
builder.Services.AddMvc();
builder.Services.AddBlazorBootstrap();
builder.Services.AddScoped<SpinnerService>();
builder.Services.AddDomain(builder);
builder.Services.AddSingleton<RefreshNotifier>();
builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
builder.Services.AddAuthenticationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration.GetConnectionString("DefaultConnection") 
                                                            ?? "mongodb://172.20.3.41:27017"));
builder.Services.AddBlazorDownloadFile(ServiceLifetime.Scoped);
builder.Services.AddInfrastructure();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<PrEditingTracker>();
builder.Services.AddSingleton<SessionStorageService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<AuthenticationStateProvider, SetiAuthStateProvider>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MessagingClient>();
builder.Services.AddHostedService<PrEditTrackerCleanup>();
builder.Services.AddSetiFileClient();
builder.Services.AddInfrastructure();
var app = builder.Build();
app.UseRequestLocalization(new RequestLocalizationOptions()
    .AddSupportedCultures("en-us")
    .AddSupportedUICultures("en-us"));
var logger = app.Services.GetService<ILogger<Program>>();
var scopeFactory = app.Services.GetService<IServiceScopeFactory>();
if(scopeFactory==null) {
    throw new Exception("Error: could not resolve IServiceScopeFactory");
}
using (IServiceScope scope = scopeFactory.CreateScope()) {
    var avatarDataService=scope.ServiceProvider.GetRequiredService<AvatarDataService>();
    if (avatarDataService!=null && logger!=null) {
        logger.LogInformation("Loading avatars");
        await avatarDataService.LoadAvatars();
        logger.LogInformation("Avatars loaded");
    } else {
        throw new Exception("Error: could not resolve AvatarDataService");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.MapControllers();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapHub<MessagingHub>("/messagehub");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
