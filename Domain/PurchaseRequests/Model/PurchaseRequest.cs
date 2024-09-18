using MongoDB.Bson;

namespace Domain.PurchaseRequests.Model;

public class PurchaseRequest {
    public ObjectId _id { get; set; }
    public string? Username { get; set; }
    public string? Approver { get; set; } = null!;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AdditionalComments { get; set; }
    public string? PaymentTerms { get; set; }
    public string? ShippingType { get; set; }
    public Department? Department { get; set; }
    public Vendor? Vendor { get; set; }
    public List<PurchaseItem> Items { get; set; }
    public string? FilePath { get; set; }
    public bool Urgent { get; set; }
    public DateTime Created { get; set; }
    public bool Approved { get; set; }
    public DateTime ApprovedDate { get; set; }
    public bool Rejected { get; set; }
    public DateTime RejectedDate { get; set; }
}
