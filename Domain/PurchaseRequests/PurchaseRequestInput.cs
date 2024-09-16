using MongoDB.Bson;

namespace Domain.PurchaseRequests;

public class PurchaseRequestInput {
    public ObjectId? Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AdditionalComments { get; set; }
    public string? FilePath { get; set; }
    public string? PrUrl { get; set; }
    public string? LinkText { get; set; }
    public bool Urgent { get; set; }
    public string? ApproverName { get; set; }
    public string? ApproverEmail { get; set; }
    public string? RequesterName { get; set; }
    public string? RequesterUsername { get; set; }
    public string? RequesterEmail { get; set; }
}
