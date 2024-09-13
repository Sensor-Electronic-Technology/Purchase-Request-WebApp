using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MongoDB.Driver;
using Radzen;
using Webapp.Components;
using Webapp.Services.Authentication;
using Infrastructure;
using Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options => options.DetailedErrors = true)
    .AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);
builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
builder.Services.AddAuthenticationCore();
builder.Services.AddSingleton<IMongoClient>(new MongoClient("mongodb://172.20.3.41:27017"));
builder.Services.AddInfrastructure();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<AuthenticationStateProvider, SetiAuthStateProvider>();
builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<PurchaseRequestService>();
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


app.Run();