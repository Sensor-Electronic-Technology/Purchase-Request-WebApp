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
    public ObjectId DepartmentId { get; set; }
    public ObjectId VendorId { get; set; }
    public DateTime Created { get; set; }
    public bool Approved { get; set; }
    public DateTime ApprovedDate { get; set; }
    public bool Rejected { get; set; }
    public DateTime RejectedDate { get; set; }
    public bool Ordered { get; set; }
    public DateTime OrderedDate { get; set; }
    public bool Received { get; set; }
    public DateTime ReceivedDate { get; set; }
    public string? FilePath { get; set; }
    public bool Urgent { get; set; }
}