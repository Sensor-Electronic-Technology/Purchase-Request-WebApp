using Domain.PurchaseRequests.Model;
using Domain.PurchaseRequests.TypeConstants;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Domain.PurchaseRequests.Dto;

public class PrReminder {
    public ObjectId Id { get; set; }
    public string? Title { get; set; }
    public string? PrLink { get; set; }
    public PrStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public double DeltaMins { get; set; }
    public DateTime Now { get; set; }
    public DateTime Previous { get; set; }
    public List<string> Recipients { get; set; } = [];
    public WriteModel<PurchaseRequest>? Update { get; set; }

    public static PrReminder FromPurchaseRequest(PurchaseRequest pr, DateTime now, double deltaMins,
        List<string>? purchaserEmails = null) {
        var update = pr.Status switch {
            PrStatus.NeedsApproval => Builders<PurchaseRequest>.Update.Set(e => e.ApprovalRemindDate, now),
            PrStatus.Approved => Builders<PurchaseRequest>.Update.Set(e => e.OrderRemindDate, now),
            PrStatus.Ordered => Builders<PurchaseRequest>.Update.Set(e => e.CheckInRemindDate, now),
            _ => throw new InvalidOperationException(
                message: $"PrStatus was not one of acceptied values. PrStatus {pr.Status:G}")
        };
        var recipients = pr.Status switch {
            PrStatus.NeedsApproval => [pr.Approver.Email ?? "itsupport@s-et.com"],
            PrStatus.Approved => purchaserEmails ?? ["itsupport@s-et.com"],
            PrStatus.Ordered => [pr.Requester.Email ?? "itsupport@s-et.com"],
            _ => []
        };
        var previousTime = pr.Status switch {
            PrStatus.NeedsApproval => pr.ApprovalRemindDate > DateTime.MinValue ? pr.ApprovalRemindDate : pr.Created,
            PrStatus.Approved => pr.OrderRemindDate > DateTime.MinValue ? pr.OrderRemindDate : pr.ApprovedDate,
            PrStatus.Ordered => pr.CheckInRemindDate > DateTime.MinValue ? pr.CheckInRemindDate : pr.OrderedDate,
            _ => DateTime.MinValue
        };
        return new PrReminder() {
            Id = pr._id,
            Title = pr.Title,
            PrLink = pr.PrUrl,
            Status = pr.Status,
            Now = now,
            Previous = previousTime,
            StatusText = pr.Status switch {
                PrStatus.NeedsApproval => "Approval",
                PrStatus.Approved => "Order",
                PrStatus.Ordered => "Check In",
                _ => "Status Error"
            },
            DeltaMins = deltaMins,
            Recipients = recipients,
            Update = new UpdateOneModel<PurchaseRequest>(Builders<PurchaseRequest>.Filter.Eq(e => e._id, pr._id),
                update),
        };
    }
}