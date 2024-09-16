using Domain;
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
builder.Services.AddDomain(builder);
builder.Services.AddControllers();
builder.Services.AddRadzenComponents();
builder.Services.AddAuthenticationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration.GetConnectionString("DefaultConnection") 
                                                            ?? "mongodb://172.20.3.41:27017"));
builder.Services.AddInfrastructure();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<AuthenticationStateProvider, SetiAuthStateProvider>();
builder.Services.AddScoped<UserService>();
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


app.Run();


/*builder.Services.AddAuthentication().AddCookie("SetiScheme", options => {
    options.Cookie.Name = "SetiAuth";
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    options.Cookie.IsEssential = true;
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/accessdenied";
    options.ReturnUrlParameter = "returnUrl";
});*/
/*builder.Services.AddAuthenticationCore().AddCookiePolicy(options => {
    options.ConsentCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    options.ConsentCookie.Name = "SetiAuth";
    options.ConsentCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    options.ConsentCookie.HttpOnly = true;
    options.ConsentCookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
    options.ConsentCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    options.ConsentCookie.IsEssential = true;
});*/
/*builder.Services.AddAuthenticationCore(options => {
    options.AddScheme("")
});
options.AddPolicy("EditUser", policy =>
    policy.RequireAssertion(context =>
    {
        if (context.Resource is RouteData rd)
        {
            var routeValue = rd.RouteValues.TryGetValue("id", out var value);
            var id = Convert.ToString(value,
                System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;

            if (!string.IsNullOrEmpty(id))
            {
                return id.StartsWith("EMP", StringComparison.InvariantCulture);
            }
        }

        return false;
    })
);*/