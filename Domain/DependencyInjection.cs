using Domain.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Domain;

public static class DependencyInjection {
    public static IServiceCollection AddDomain(this IServiceCollection services,IHostApplicationBuilder builder) {
        /*services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));
        services.Configure<EmailSettings>(builder.Configuration.GetSection(nameof(EmailSettings)));*/
        services.AddSingleton<DatabaseSettings>((provider =>
            builder.Configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>()));
        services.AddSingleton<EmailSettings>((provider =>
            builder.Configuration.GetSection(nameof(EmailSettings)).Get<EmailSettings>()));
        return services;
    }
}