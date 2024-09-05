using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Radzen;
using Visus.Ldap;
using Visus.LdapAuthentication;
using Visus.LdapAuthentication.Configuration;
using Webapp.Authentication;
using Webapp.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRadzenComponents();

builder.Services.AddAuthenticationCore();

builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<AuthenticationStateProvider, LdapAuthStateProvider>();
builder.Services.AddSingleton<UserAccountService>();
builder.Services.AddLdapAuthentication<LdapUser, LdapGroup>(o => {
    builder.Configuration.GetSection(LdapOptions.Section).Bind(o);
});

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