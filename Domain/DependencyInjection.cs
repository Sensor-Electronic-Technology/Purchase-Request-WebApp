using Domain.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Domain;

public static class DependencyInjection {
    public static IHostApplicationBuilder AddDomain(this IHostApplicationBuilder builder) {
        builder.Services.AddSingleton<DatabaseSettings>((provider => 
            builder.Configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>()));
        builder.Services.AddSingleton<EmailSettings>((provider => 
            builder.Configuration.GetSection(nameof(EmailSettings)).Get<EmailSettings>()));
        return builder;
    }
}