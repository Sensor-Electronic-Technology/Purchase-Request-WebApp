using MongoDB.Bson;

namespace Domain.PurchaseRequests.Dto;

public class CancelPurchaseRequestInput {
    public ObjectId Id { get; set; }
    public string? Title { get; set; }
    public string? Reason { get; set; }
    public List<string> FileIds { get; set; } = [];
    public byte[]? EmailTemplate { get; set; }
    public List<string> EmailCopyList { get; set; } = [];
}