using BlazorDownloadFile;
using Domain;
using Infrastructure;
using Infrastructure.Hubs;
using Infrastructure.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.FileProviders;
using MongoDB.Driver;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Radzen;
using SetiFileStore.FileClient;
using Webapp.Components;
using Webapp.Services;
using Webapp.Services.Authentication;

namespace Webapp;

public static class DependencyInjection {
    public static IServiceCollection AddMetrics(this IServiceCollection services,IHostApplicationBuilder builder) {
        var otel =services.AddOpenTelemetry();
        otel.WithMetrics(metrics => {
            metrics.AddAspNetCoreInstrumentation();
            metrics.AddRuntimeInstrumentation();
            metrics.AddMeter("Microsoft.AspNetCore.Hosting");
            metrics.AddMeter("Microsoft.AspNetCore.Server.Kestrel");
        });
        otel.WithTracing(tracing => {
            tracing.AddAspNetCoreInstrumentation();
        });
        
        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (otlpEndpoint != null) {
            otel.UseOtlpExporter();
        }
        return services;
    }

    public static WebApplication AddAssets(this WebApplication app,WebApplicationBuilder builder) {
        List<string> paths = ["images/avatars","images","MailTemplateFiles"];
        List<IFileProvider> fileProviders = [];
        foreach (var path in paths) {
            var provider = new PhysicalFileProvider(
                Path.Combine(builder.Environment.WebRootPath, path));
            fileProviders.Add(provider);
        }
        fileProviders.Add(app.Environment.WebRootFileProvider);
        app.Environment.WebRootFileProvider = new CompositeFileProvider(fileProviders);
        /*//app.Environment.WebRootFileProvider=new CompositeFileProvider()
        var secondaryProvider = new PhysicalFileProvider(
            Path.Combine(builder.Environment.WebRootPath, "images/avatars"));
        
        app.Environment.WebRootFileProvider = new CompositeFileProvider(
            app.Environment.WebRootFileProvider, secondaryProvider);*/
        return app;
    }

    public static async Task<WebApplication> BuildApp(this WebApplicationBuilder builder) {

        var app=builder.Build();
        /*app.AddAssets(builder);*/
        app.UseRequestLocalization(new RequestLocalizationOptions()
            .AddSupportedCultures("en-us")
            .AddSupportedUICultures("en-us"));

        /*await app.DownloadAssets();*/
        if (!app.Environment.IsDevelopment()) {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
        }

        app.MapControllers();
        app.MapStaticAssets();
        //app.UseStaticFiles();
        app.UseAntiforgery();
        app.MapHub<MessagingHub>("/messagehub");
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
        return app;
    }

    public static IHostApplicationBuilder AddBlazorComponents(this IHostApplicationBuilder builder) {
        builder.Services.AddMetrics(builder);
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
        builder.Services.AddControllers();
        builder.Services.AddRadzenComponents();
        builder.Services.AddAuthenticationCore();
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddBlazorDownloadFile(ServiceLifetime.Scoped);
        builder.Services.AddHttpClient();
        return builder;
    }
    
    public static IHostApplicationBuilder AddPurchaseRequestWebApp(this IHostApplicationBuilder builder) {
        builder.AddDomain();
        builder.Services.AddInfrastructure();
        builder.Services.AddSetiFileClient();
        builder.Services.AddScoped<SpinnerService>();
        builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration.GetConnectionString("DefaultConnection") 
                                                                    ?? "mongodb://172.20.3.41:27017"));
        builder.Services.AddSingleton<PrEditingTracker>();
        builder.Services.AddScoped<ProtectedSessionStorage>();
        builder.Services.AddScoped<AuthenticationStateProvider, SetiAuthStateProvider>();
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<MessagingClient>();
        builder.Services.AddHostedService<PrEditTrackerCleanup>();
        return builder;
    }

    private static async Task<IApplicationBuilder> DownloadAssets(this WebApplication app) {
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
        return app;
    }
}