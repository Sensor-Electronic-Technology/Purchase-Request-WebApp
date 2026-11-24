using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

//IServiceScopeFactory scopeFactory
public class ReminderHostedService(
    ILogger<ReminderHostedService> logger, 
    IServiceScopeFactory scopeFactory) : BackgroundService {
    private readonly TimeSpan _period = TimeSpan.FromMinutes(30);

    public override async Task StartAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Reminder service started, performing initial check");
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        logger.LogInformation("Starting initial reminder check");
        await ExecuteStart(stoppingToken);
        var timer = new PeriodicTimer(_period);
        logger.LogInformation("Initial check complete.  Starting main loop");
        try {
            while (await timer.WaitForNextTickAsync(stoppingToken)) {
                await using var scope = scopeFactory.CreateAsyncScope();
                var remindService = scope.ServiceProvider.GetRequiredService<PrReminderService>();
                await remindService.Check(stoppingToken);
            }
        } catch (OperationCanceledException) {
            logger.LogInformation("Reminder service timer cancelled");
        }
    }

    private async Task ExecuteStart(CancellationToken ct = default) {
        try {
            await using var scope = scopeFactory.CreateAsyncScope();
            var remindService=scope.ServiceProvider.GetRequiredService<PrReminderService>();
            await remindService.Check(ct);
        } catch (Exception ex) when (ex is not OperationCanceledException) {
            logger.LogError(ex, "Initial check failed in ExecuteStart");
        } catch (OperationCanceledException) {
            logger.LogInformation("Initial check canceled in ExecuteStart");
        }
    }
    
    public override Task StopAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Reminder service stopped");
        return base.StopAsync(cancellationToken);
    }
}