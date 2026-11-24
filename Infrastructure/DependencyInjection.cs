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
        services.AddScoped<PoNumberService>();
        services.AddScoped<ChatRoomDataService>();
        services.AddScoped<RemindDataService>();
        services.AddSingleton<AppTimeProvider>();
        services.AddTransient<PrReminderService>();
        services.AddHostedService<ReminderHostedService>();
        
        
        /*services.AddScoped<PrReminderService>();
        services.AddHostedService<ReminderService>();*/
        return services;
    }
}