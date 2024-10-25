using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services) {
        services.AddScoped<UserProfileService>();
        services.AddScoped<PurchaseRequestService>();
        services.AddScoped<EmailService>();
        services.AddScoped<AuthApiService>();
        services.AddScoped<ContactDataService>();
        services.AddScoped<DepartmentDataService>();
        services.AddScoped<PurchaseRequestDataService>();
        services.AddScoped<AvatarDataService>();
        return services;
    }
}