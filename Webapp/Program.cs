using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Radzen;
using Webapp.Components;
using Webapp.Services;
using Webapp.Services.Authentication;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRadzenComponents();
builder.Services.AddAuthenticationCore();
builder.Services.AddSingleton<IMongoClient>(new MongoClient("mongodb://172.20.3.41:27017"));
builder.Services.AddInfrastructure();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<AuthenticationStateProvider, SetiAuthStateProvider>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
/*builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<CircuitHandler, UserCircuitHandler>());*/
var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();