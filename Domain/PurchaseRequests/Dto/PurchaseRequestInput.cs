using Domain.PurchaseRequests.Model;
using MongoDB.Bson;

namespace Domain.PurchaseRequests.Dto;

public class PurchaseRequestInput {
    public ObjectId? Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AdditionalComments { get; set; }
    public string? PrUrl { get; set; }
    public string? TempFile { get; set; }
    public string? LinkText { get; set; }
    public List<PurchaseItem> PurchaseItems { get; set; } = [];
    public Vendor? Vendor { get; set; }
    public Department? Department { get; set; }
    public bool Urgent { get; set; }
    public string? ApproverName { get; set; }
    public string? ApproverEmail { get; set; }
    public string? RequesterName { get; set; }
    public string? RequesterUsername { get; set; }
    public string? RequesterEmail { get; set; }
    public List<string> Quotes { get; set; } = [];
    public List<(string name,string filePath)> Attachments { get; set; } = [];
}
