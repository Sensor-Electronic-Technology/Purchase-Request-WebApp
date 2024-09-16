using Domain.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Domain;

public static class DependencyInjection {
    public static IServiceCollection AddDomain(this IServiceCollection services,IHostApplicationBuilder builder) {
        services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));
        services.Configure<EmailSettings>(builder.Configuration.GetSection(nameof(EmailSettings)));
        return services;
    }
}