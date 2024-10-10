using Domain;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MongoDB.Driver;
using Radzen;
using Webapp.Components;
using Webapp.Services.Authentication;
using Infrastructure;
using QuestPDF.Infrastructure;
using SetiFileStore.FileClient;
using Webapp.Services;

QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options => options.DetailedErrors = true)
    .AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);
builder.Services.AddDevExpressBlazor(options => {
    options.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5;
    options.SizeMode = DevExpress.Blazor.SizeMode.Small;
});
builder.Services.AddMvc();
builder.Services.AddScoped<SpinnerService>();
builder.Services.AddDomain(builder);
builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
builder.Services.AddAuthenticationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration.GetConnectionString("DefaultConnection") 
                                                            ?? "mongodb://172.20.3.41:27017"));
builder.Services.AddInfrastructure();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<PrEditStatus>();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<AuthenticationStateProvider, SetiAuthStateProvider>();
builder.Services.AddScoped<UserService>();
builder.Services.AddHostedService<EditStatusCleanup>();
//builder.Services.AddHttpClient<FileService>(x=>x.BaseAddress = new Uri(builder.Configuration["FileServiceUrl"] ?? "http://localhost:8080/FileStorage/"));
builder.Services.AddSetiFileClient();
builder.Services.AddInfrastructure();
/*builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<CircuitHandler, UserCircuitHandler>());*/
var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.MapControllers();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
//app.Urls.Add("http://0.0.0.0:8080");
app.Run();
