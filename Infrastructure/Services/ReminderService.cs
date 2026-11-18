using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ReminderService(ILogger<ReminderService> logger, IServiceScopeFactory scopeFactory) : BackgroundService {
    private readonly TimeSpan _period = TimeSpan.FromMinutes(30);
    private bool _emailSent = false;
    
    public override async Task StartAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Reminder service started, performing initial check");
        await CheckOpenRequests(cancellationToken);
        await base.StartAsync(cancellationToken);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        var timer = new PeriodicTimer(_period);
        try {
            while (await timer.WaitForNextTickAsync(stoppingToken)) {
                await CheckOpenRequests(stoppingToken);
            }
            
        } catch (OperationCanceledException) {
            logger.LogInformation("Reminder service timer cancelled");
        }
    }

    private async Task CheckOpenRequests(CancellationToken stoppingToken) {
        try {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            var prService = scope.ServiceProvider.GetRequiredService<PrReminderService>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
            /*if (!this._emailSent) {
                this._emailSent = true;
                await emailService.SendReminderEmail("Test Item for email format", "https://purchasing.seti.com", "Approval",
                    "aelmendorf@s-et.com");
            }*/

            logger.LogInformation("Checking one purchase requests {Time}", DateTimeOffset.UtcNow);
            
            foreach (var pr in await prService.GetRequestWaitingApproval().ToListAsync(stoppingToken)) {
                if (string.IsNullOrEmpty(pr.Title)) {
                    continue;
                }
                logger.LogInformation("Sending Approval Reminder: PrTitle: {PrTitle}, Recipient: {PrRecipient}", pr.Title,pr.Approver.Email);
                await emailService.SendReminderEmail(pr.Title,pr.PrUrl ?? "https://purchasing.seti.com","Approval","aelmendorf@s-et.com");
                /*logger.LogInformation("Pr: {PrName} PrTime: {PrTime} Now: {Now} Delta(Mins): {Delta}",pr.Title ,pr.Created, now,timeProvider.MinutesSince(pr.Created));*/
            }
            
            foreach (var pr in await prService.GetRequestWaitingOrder().ToListAsync(stoppingToken)) {
                if (string.IsNullOrEmpty(pr.Title)) {
                    continue;
                }
                logger.LogInformation("Sending Order Reminder: PrTitle: {PrTitle}, Recipient: {PrRecipient}", pr.Title,pr.Purchaser?.Email ?? "dglover@s-et.com");
                await emailService.SendReminderEmail(pr.Title,pr.PrUrl ?? "https://purchasing.seti.com","Order","aelmendorf@s-et.com");
            }
            
            foreach (var pr in await prService.GetRequestWaitingDelivery().ToListAsync(stoppingToken)) {
                if (string.IsNullOrEmpty(pr.Title)) {
                    continue;
                }
                logger.LogInformation("Sending Check In Reminder: PrTitle: {PrTitle}, Recipient: {PrRecipient}", pr.Title,pr.Requester.Email);
                await emailService.SendReminderEmail(pr.Title,pr.PrUrl ?? "https://purchasing.seti.com","Check In","aelmendorf@s-et.com");
            }
        } catch (Exception ex) when (ex is not OperationCanceledException) {
            logger.LogError(ex, "Error occurred executing timed background work.");
        }
    }
    
    public override Task StopAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Reminder service stopped");
        return base.StopAsync(cancellationToken);
    }
}