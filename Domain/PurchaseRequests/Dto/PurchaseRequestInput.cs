using Domain.PurchaseRequests.Model;
using MongoDB.Bson;

namespace Domain.PurchaseRequests.Dto;



public class PurchaseRequestInput {
    public ObjectId? Id { get; set; }
    public string? ApproverName { get; set; }
    public string? ApproverEmail { get; set; }
    public string? RequesterName { get; set; }
    public string? RequesterUsername { get; set; }
    public string? RequesterEmail { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AdditionalComments { get; set; }
    public string? ShippingType { get; set; }
    public string? PrUrl { get; set; }
    public byte[]? TempFile { get; set; }=Array.Empty<byte>();
    public Vendor? Vendor { get; set; }
    public Department? Department { get; set; }
    public List<PurchaseItem> PurchaseItems { get; set; } = [];
    public decimal TotalCost => PurchaseItems.Sum(x => x.TotalCost);
    public bool Urgent { get; set; }
    public List<string> Quotes { get; set; } = [];
    public List<FileInput> Attachments { get; set; } = [];
}
