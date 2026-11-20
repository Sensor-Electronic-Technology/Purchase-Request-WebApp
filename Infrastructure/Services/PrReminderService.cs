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
    private static readonly string FromAddress = "purchase.request@s-et.com";
    private readonly IMongoCollection<PurchaseRequest> _prCollection;
    private readonly EmailService _emailService;
    private readonly AppTimeProvider _timeProvider;
    private readonly ReminderSettings _reminderSettings;
    private readonly IWebHostEnvironment _environment;
    private readonly AuthApiService _authApiService;
    private readonly ILogger<PrReminderService> _logger;

    public PrReminderService(IMongoClient client,
        AuthApiService authApiService,
        AppTimeProvider timeProvider,
        IOptions<DatabaseSettings> dbOptions,
        IOptions<ReminderSettings> remindOptions, 
        IWebHostEnvironment environment, 
        ILogger<PrReminderService> logger, 
        EmailService emailService) {
        var database = client.GetDatabase(dbOptions.Value.PurchaseRequestDatabase ?? "purchase_req_db");
        this._prCollection = database.GetCollection<PurchaseRequest>(dbOptions.Value.PurchaseRequestCollection ?? "purchase_requests");
        this._timeProvider = timeProvider;
        this._environment = environment;
        this._logger = logger;
        _emailService = emailService;
        this._reminderSettings = remindOptions.Value;
        this._environment = environment;
        this._authApiService = authApiService;
        this._logger=logger;
        this._emailService=emailService;
    }

    public async Task Check(CancellationToken ct=default) {
        var now=_timeProvider.Now();
        if (!InTimeRange(now)) {
            return;
        }
        var filePath=$"{this._environment.WebRootPath}/MailTemplateFiles/ReminderEmailTemplate.html";
        var messageTemplate=await File.ReadAllTextAsync(filePath, ct);
        
        var purchaserList =await this._authApiService.GetPurchasers();
        var purchasers= purchaserList.Where(e=>e.Email!="itsupport@s-et.com").Select(e => e.Email).ToList();
        
        List<WriteModel<PurchaseRequest>> updates = [];
        List<MimeMessage> reminderMessages = [];
        await foreach (var prRemind in GetRequestWaitingApproval().WithCancellation(ct)) {
            if (prRemind.Update == null) continue;
            this._logger.LogInformation("Pr: {PrName} PrTime: {PrTime} Now: {Now} Delta(Mins): {Delta}",
                prRemind.Title,prRemind.Previous,prRemind.Now,prRemind.DeltaMins);
            updates.Add(prRemind.Update);
            reminderMessages.Add(CreateRemindEmail(prRemind,messageTemplate));
        }

        if (updates.Count > 0) {
            var updateResult=await _prCollection.BulkWriteAsync(updates, cancellationToken: ct);
            if (updateResult is { IsAcknowledged: true, ModifiedCount: > 0 }) {
                this._logger.LogInformation("PurchaseRequest Approval reminders updated");
                await this._emailService.SendReminderEmails(reminderMessages);
            } else {
                this._logger.LogError("PurchaseRequest reminders not updated and emails not sent");
            }
        } else {
            this._logger.LogInformation("No reminders found");
        }
        
        updates.Clear();
        reminderMessages.Clear();
        await foreach (var prRemind in GetRequestWaitingOrder(purchasers).WithCancellation(ct)) {
            if (prRemind.Update == null) continue;
            this._logger.LogInformation("Pr: {PrName} PrTime: {PrTime} Now: {Now} Delta(Mins): {Delta}",
                prRemind.Title,prRemind.Previous,prRemind.Now,prRemind.DeltaMins);
            updates.Add(prRemind.Update);
            reminderMessages.Add(CreateRemindEmail(prRemind,messageTemplate));
        }

        if (updates.Count > 0) {
            var updateResult=await _prCollection.BulkWriteAsync(updates, cancellationToken: ct);
            if (updateResult is { IsAcknowledged: true, ModifiedCount: > 0 }) {
                this._logger.LogInformation("PurchaseRequest Approval reminders updated");
                await this._emailService.SendReminderEmails(reminderMessages);
            } else {
                this._logger.LogError("PurchaseRequest reminders not updated and emails not sent");
            }
        } else {
            this._logger.LogInformation("No reminders found");
        }

    }

    private bool InTimeRange(DateTime now) {
        /*if (now.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) {
            this._logger.LogInformation("Skipping check, outside of allowed days");
            return false;
        }*/

        TimeOnly minTime = new TimeOnly(6, 0, 0);
        TimeOnly maxTime = new TimeOnly(18, 0, 0);
        if (!(now.TimeOfDay >= minTime.ToTimeSpan() && now.TimeOfDay < maxTime.ToTimeSpan())) {
            this._logger.LogInformation("Skipping check, outside of allowed time");
            return false;
        }

        return true;
    }
    
    public async IAsyncEnumerable<PrReminder> GetRequestWaitingApproval([EnumeratorCancellation] CancellationToken ct=default) {
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

    public async IAsyncEnumerable<PrReminder> GetRequestWaitingOrder(List<string> purchaserEmails) {
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
                    yield return PrReminder.FromPurchaseRequest(prItem, now, deltaMin,purchaserEmails);
                }
            }
        }
    }

    public async IAsyncEnumerable<PrReminder> GetRequestWaitingDelivery() {
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
    }
    
    public MimeMessage CreateRemindEmail(PrReminder reminder,string htmlMessage) {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress($"Purchase Request", FromAddress));
        
        this._logger.LogInformation("Sending Reminder: Recipient:{Recipient} Pr:{Title} DeltaMin:{Delta}",
            $"[{string.Join(',',reminder.Recipients)}]",reminder.Title,reminder.DeltaMins);
        message.To.AddRange(reminder.Recipients.Select(recipient => new MailboxAddress("aelmendorf@s-et.com", "aelmendorf@s-et.com")));
        /*foreach (var recipient in reminder.Recipients) {
            message.To.Add(new MailboxAddress(recipient, recipient));
        }*/
        /*message.To.AddRange(reminder.Recipients.Select(recipient => new MailboxAddress("aelmendorf@s-et.com", "aelmendorf@s-et.com")));*/

        message.Subject = $"Purchase Request Reminder-{reminder.StatusText} Needed";

        var body = htmlMessage.Replace("{{ pr_title }}", reminder.Title)
            .Replace("{{ reminder_type }}", reminder.StatusText.ToLower())
            .Replace("{{ pr_link }}", $"  <a style=\"color: #0c63e4\" href=\"{reminder.PrLink}\">Purchase Request Link</a>");
            /*.Replace("{{ pr_link }}", reminder.PrLink);*/
            
        var builder = new BodyBuilder { 
            HtmlBody = body
        };
        message.Body = builder.ToMessageBody();
        return message;
    }
}