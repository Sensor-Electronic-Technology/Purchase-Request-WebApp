using Domain.PurchaseRequests.Model;
using MongoDB.Driver;
using System.Runtime.CompilerServices;
using Domain.PurchaseRequests.Dto;
using Domain.PurchaseRequests.TypeConstants;
using Domain.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services;

public class PrReminderService {
    private static readonly TimeOnly MinTime = new TimeOnly(6, 0, 0);
    private static readonly TimeOnly MaxTime = new TimeOnly(18, 0, 0);
    private static readonly string FromAddress = "purchase.request@s-et.com";
    private readonly IMongoCollection<PurchaseRequest> _prCollection;
    private readonly EmailService _emailService;
    private readonly AppTimeProvider _timeProvider;
    //private readonly ReminderSettings _reminderSettings;
    private readonly IWebHostEnvironment _environment;
    private readonly AuthApiService _authApiService;
    private readonly ILogger<PrReminderService> _logger;
    private readonly RemindDataService _remindDataService;

    public PrReminderService(IMongoClient client,
        AuthApiService authApiService,
        AppTimeProvider timeProvider,
        IOptions<DatabaseSettings> dbOptions,
        IOptions<ReminderSettings> remindOptions,
        IWebHostEnvironment environment,
        ILogger<PrReminderService> logger,
        EmailService emailService, 
        RemindDataService remindDataService) {
        var database = client.GetDatabase(dbOptions.Value.PurchaseRequestDatabase ?? "purchase_req_db");
        this._prCollection =
            database.GetCollection<PurchaseRequest>(dbOptions.Value.PurchaseRequestCollection ?? "purchase_requests");
        this._timeProvider = timeProvider;
        this._environment = environment;
        this._logger = logger;
        this._emailService = emailService;
        _remindDataService = remindDataService;
        //this._reminderSettings = remindOptions.Value;
        this._environment = environment;
        this._authApiService = authApiService;
        this._logger = logger;
        this._emailService = emailService;
        this._remindDataService = remindDataService;
    }

    public async Task Check(CancellationToken ct = default) {
        var now = _timeProvider.Now();
        if (!InTimeRange(now)) {
            return;
        }
        var remindConfig=await this._remindDataService.GetRemindConfigAsync();
        var filePath = $"{this._environment.WebRootPath}/MailTemplateFiles/ReminderEmailTemplate.html";
        var messageTemplate = await File.ReadAllTextAsync(filePath, ct);
        var purchaserList = await this._authApiService.GetPurchasers();
        var purchasers = purchaserList.Where(e => e.Email != "itsupport@s-et.com").Select(e => e.Email).ToList();
        await ApprovalReminderCheck(messageTemplate,remindConfig, ct);
        await OrderReminderCheck(messageTemplate,remindConfig, purchasers, ct);
        //await CheckInReminderCheck(messageTemplate,remindConfig, ct);
    }

    private async Task ApprovalReminderCheck(string messageTemplate,RemindConfig config, CancellationToken ct) {
        List<WriteModel<PurchaseRequest>> updates = [];
        List<MimeMessage> reminderMessages = [];
        this._logger.LogInformation("Starting 'Waiting For Approval' check");
        await foreach (var prRemind in GetRequestWaitingApproval(config).WithCancellation(ct)) {
            if (prRemind.Update == null) continue;
            this._logger.LogInformation("Pr: {PrName} PrTime: {PrTime} Now: {Now} Delta(Hrs): {Delta}",
                prRemind.Title, prRemind.Previous, prRemind.Now, prRemind.DeltaTime);
            updates.Add(prRemind.Update);
            reminderMessages.Add(CreateRemindEmail(prRemind, messageTemplate));
        }

        if (updates.Count > 0) {
            var updateResult = await _prCollection.BulkWriteAsync(updates, cancellationToken: ct);
            if (updateResult is { IsAcknowledged: true, ModifiedCount: > 0 }) {
                this._logger.LogInformation("PurchaseRequest 'Approval Reminders' updated");
                await this._emailService.SendReminderEmails(reminderMessages);
            } else {
                this._logger.LogError("PurchaseRequest reminders not updated and emails not sent");
            }
        }
        this._logger.LogInformation("'Waiting For Approval' check completed");
    }
    
    private async Task OrderReminderCheck(string messageTemplate,RemindConfig config,List<string> purchasers, CancellationToken ct) {
        List<WriteModel<PurchaseRequest>> updates = [];
        List<MimeMessage> reminderMessages = [];
        this._logger.LogInformation("Starting 'Waiting For Order' check");
        await foreach (var prRemind in GetRequestWaitingOrder(config,purchasers).WithCancellation(ct)) {
            if (prRemind.Update == null) continue;
            this._logger.LogInformation("Pr: {PrName} PrTime: {PrTime} Now: {Now} Delta(Hours): {Delta}",
                prRemind.Title, prRemind.Previous, prRemind.Now, prRemind.DeltaTime);
            updates.Add(prRemind.Update);
            reminderMessages.Add(CreateRemindEmail(prRemind, messageTemplate));
        }

        if (updates.Count > 0) {
            var updateResult = await _prCollection.BulkWriteAsync(updates, cancellationToken: ct);
            if (updateResult is { IsAcknowledged: true, ModifiedCount: > 0 }) {
                this._logger.LogInformation("PurchaseRequest 'Order Reminders' Updated");
                await this._emailService.SendReminderEmails(reminderMessages);
            } else {
                this._logger.LogError("PurchaseRequest reminders not updated and emails not sent");
            }
        }
        this._logger.LogInformation("'Waiting For Order' check completed");
    }
    
    private async Task CheckInReminderCheck(string messageTemplate,RemindConfig config, CancellationToken ct) {
        List<WriteModel<PurchaseRequest>> updates = [];
        List<MimeMessage> reminderMessages = [];
        this._logger.LogInformation("Starting 'Waiting For Check-In' check");
        await foreach (var prRemind in GetCheckInReminders(config).WithCancellation(ct)) {
            if (prRemind.Update == null) continue;
            this._logger.LogInformation("Pr: {PrName} PrTime: {PrTime} Now: {Now} Delta(Days): {Delta}",
                prRemind.Title, prRemind.Previous, prRemind.Now, prRemind.DeltaTime);
            updates.Add(prRemind.Update);
            reminderMessages.Add(CreateRemindEmail(prRemind, messageTemplate));
        }

        if (updates.Count > 0) {
            var updateResult = await _prCollection.BulkWriteAsync(updates, cancellationToken: ct);
            if (updateResult is { IsAcknowledged: true, ModifiedCount: > 0 }) {
                this._logger.LogInformation("PurchaseRequest 'Check-In Reminders' Updated");
                await this._emailService.SendReminderEmails(reminderMessages);
            } else {
                this._logger.LogError("PurchaseRequest reminders not updated and emails not sent");
            }
        }
        this._logger.LogInformation("'Waiting For Check-In' check completed");
    }

    private static bool InTimeRange(DateTime now) =>
        now.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday) &&
        now.TimeOfDay >= MinTime.ToTimeSpan() && now.TimeOfDay <= MaxTime.ToTimeSpan();


    private async IAsyncEnumerable<PrReminder> GetRequestWaitingApproval(RemindConfig config,
        [EnumeratorCancellation] CancellationToken ct = default) {
        var filter = Builders<PurchaseRequest>.Filter.Eq(e => e.Status, PrStatus.NeedsApproval);
        var findOptions = new FindOptions<PurchaseRequest>() { BatchSize = 10 };
        using IAsyncCursor<PurchaseRequest>? cursor = await this._prCollection.FindAsync(filter, findOptions, ct);
        while (await cursor.MoveNextAsync(ct)) {
            foreach (var prItem in cursor.Current) {
                var now = _timeProvider.Now();
                var repeat = prItem.ApprovalRemindDate > DateTime.MinValue;
                var deltaMin = repeat
                    ? _timeProvider.HoursSince(now, prItem.ApprovalRemindDate)
                    : _timeProvider.HoursSince(now, prItem.Created);
                var needReminder = repeat
                    ? deltaMin >= config.ApprovalTimeRepeatHrs
                    : deltaMin >= config.ApprovalTimeHrs;
                if (needReminder) {
                    yield return PrReminder.FromPurchaseRequest(prItem, now, deltaMin);
                }
            }
        }
    }

    private async IAsyncEnumerable<PrReminder> GetRequestWaitingOrder(RemindConfig config,List<string> purchaserEmails) {
        var filter = Builders<PurchaseRequest>.Filter.Eq(e => e.Status, PrStatus.Approved);
        var findOptions = new FindOptions<PurchaseRequest>() { BatchSize = 10 };
        using IAsyncCursor<PurchaseRequest>? cursor = await this._prCollection.FindAsync(filter, findOptions);
        while (await cursor.MoveNextAsync()) {
            foreach (var prItem in cursor.Current) {
                var now = _timeProvider.Now();
                var repeat = prItem.OrderRemindDate > DateTime.MinValue;
                var deltaMin = repeat
                    ? _timeProvider.MinutesSince(now, prItem.OrderRemindDate)
                    : _timeProvider.MinutesSince(now, prItem.ApprovedDate);
                var needReminder = repeat
                    ? deltaMin >= config.OrderTimeRepeatHrs
                    : deltaMin >= config.OrderTimeHrs;
                if (needReminder) {
                    yield return PrReminder.FromPurchaseRequest(prItem, now, deltaMin, purchaserEmails);
                }
            }
        }
    }

    private async IAsyncEnumerable<PrReminder> GetCheckInReminders(RemindConfig config) {
        var filter = Builders<PurchaseRequest>.Filter.Eq(e => e.Status, PrStatus.Ordered);
        var findOptions = new FindOptions<PurchaseRequest>() { BatchSize = 10 };
        using IAsyncCursor<PurchaseRequest>? cursor = await this._prCollection.FindAsync(filter, findOptions);
        while (await cursor.MoveNextAsync()) {
            foreach (var prItem in cursor.Current) {
                var now = _timeProvider.Now();
                var repeat = prItem.CheckInRemindDate > DateTime.MinValue;
                var deltaMin = repeat
                    ? _timeProvider.DaysSince(now, prItem.CheckInRemindDate)
                    : _timeProvider.DaysSince(now, prItem.OrderedDate);
                var needReminder = repeat
                    ? deltaMin >= config.CheckInTimeRepeatDays
                    : deltaMin >= config.CheckInTimeDays;
                if (needReminder) {
                    yield return PrReminder.FromPurchaseRequest(prItem, now, deltaMin);
                }
            }
        }
    }

    public MimeMessage CreateRemindEmail(PrReminder reminder, string htmlMessage) {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress($"Purchase Request", FromAddress));
        this._logger.LogInformation("Sending Reminder: Recipient:{Recipient} Pr:{Title} DeltaMin:{Delta}",
            $"[{string.Join(',', reminder.Recipients)}]", reminder.Title, reminder.DeltaTime);
        message.To.Add(new MailboxAddress($"Purchase Request", "itsupport@s-et.com"));
        message.To.AddRange(reminder.Recipients.Select(recipient => new MailboxAddress(recipient,recipient)));
        message.Subject = $"Purchase Request Reminder-{reminder.StatusText} Needed";
        var body = htmlMessage.Replace("{{ pr_title }}", reminder.Title)
            .Replace("{{ reminder_type }}", reminder.StatusText.ToLower())
            .Replace("{{ pr_link }}",
                $"  <a style=\"color: #0c63e4\" href=\"{reminder.PrLink}\">Purchase Request Link</a>");
        var builder = new BodyBuilder {
            HtmlBody = body
        };
        message.Body = builder.ToMessageBody();
        return message;
    }

    /*private async IAsyncEnumerable<PrReminder> GetRequestWaitingApproval(
        [EnumeratorCancellation] CancellationToken ct = default) {
        var filter = Builders<PurchaseRequest>.Filter.Eq(e => e.Status, PrStatus.NeedsApproval);
        var findOptions = new FindOptions<PurchaseRequest>() { BatchSize = 10 };
        using IAsyncCursor<PurchaseRequest>? cursor = await this._prCollection.FindAsync(filter, findOptions, ct);
        while (await cursor.MoveNextAsync(ct)) {
            foreach (var prItem in cursor.Current) {
                var now = _timeProvider.Now();
                var repeat = prItem.ApprovalRemindDate > DateTime.MinValue;
                var deltaMin = repeat
                    ? _timeProvider.MinutesSince(now, prItem.ApprovalRemindDate)
                    : _timeProvider.MinutesSince(now, prItem.Created);
                var needReminder = repeat
                    ? deltaMin >= _reminderSettings.ApprovalTimeRepeatMins
                    : deltaMin >= _reminderSettings.ApprovalTimeMins;
                if (needReminder) {
                    yield return PrReminder.FromPurchaseRequest(prItem, now, deltaMin);
                }
            }
        }
    }

    private async IAsyncEnumerable<PrReminder> GetRequestWaitingOrder(List<string> purchaserEmails) {
        var filter = Builders<PurchaseRequest>.Filter.Eq(e => e.Status, PrStatus.Approved);
        var findOptions = new FindOptions<PurchaseRequest>() { BatchSize = 10 };
        using IAsyncCursor<PurchaseRequest>? cursor = await this._prCollection.FindAsync(filter, findOptions);
        while (await cursor.MoveNextAsync()) {
            foreach (var prItem in cursor.Current) {
                var now = _timeProvider.Now();
                var repeat = prItem.OrderRemindDate > DateTime.MinValue;
                var deltaMin = repeat
                    ? _timeProvider.MinutesSince(now, prItem.OrderRemindDate)
                    : _timeProvider.MinutesSince(now, prItem.ApprovedDate);
                var needReminder = repeat
                    ? deltaMin >= _reminderSettings.OrderTimeRepeatMins
                    : deltaMin >= _reminderSettings.OrderTimeMins;
                if (needReminder) {
                    yield return PrReminder.FromPurchaseRequest(prItem, now, deltaMin, purchaserEmails);
                }
            }
        }
    }

    private async IAsyncEnumerable<PrReminder> GetRequestWaitingDelivery() {
        var filter = Builders<PurchaseRequest>.Filter.Eq(e => e.Status, PrStatus.Ordered);
        var findOptions = new FindOptions<PurchaseRequest>() { BatchSize = 10 };
        using IAsyncCursor<PurchaseRequest>? cursor = await this._prCollection.FindAsync(filter, findOptions);
        while (await cursor.MoveNextAsync()) {
            foreach (var prItem in cursor.Current) {
                var now = _timeProvider.Now();
                var repeat = prItem.CheckInRemindDate > DateTime.MinValue;
                var deltaMin = repeat
                    ? _timeProvider.MinutesSince(now, prItem.CheckInRemindDate)
                    : _timeProvider.MinutesSince(now, prItem.OrderedDate);
                var needReminder = repeat
                    ? deltaMin >= _reminderSettings.CheckInTimeRepeatMins
                    : deltaMin >= _reminderSettings.CheckInTimeMins;
                if (needReminder) {
                    yield return PrReminder.FromPurchaseRequest(prItem, now, deltaMin);
                }
            }
        }
    }*/
}