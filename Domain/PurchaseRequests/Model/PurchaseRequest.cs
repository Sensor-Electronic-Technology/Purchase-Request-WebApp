using Domain.PurchaseRequests.Dto;
using Domain.PurchaseRequests.TypeConstants;
using MongoDB.Bson;

namespace Domain.PurchaseRequests.Model;

public class PurchaseRequest {
    public ObjectId _id { get; set; }
    public string? Requester { get; set; }
    public string? Approver { get; set; } = null!;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AdditionalComments { get; set; }
    public string? ShippingType { get; set; }
    public string? PrUrl { get; set; }
    public Department? Department { get; set; }
    public Vendor? Vendor { get; set; }
    public List<PurchaseItem> PurchaseItems { get; set; } = [];
    public decimal TotalCost=>PurchaseItems.Sum(x=>x.TotalCost);
    public bool Urgent { get; set; }
    public List<string> Quotes { get; set; } = [];
    public PurchaseOrder? PurchaseOrder { get; set; }
    public PrStatus Status { get; set; }
    public DateTime Created { get; set; }
    public DateTime ApprovedDate { get; set; }
    public DateTime RejectedDate { get; set; }
    public DateTime OrderedDate { get; set; }
    public DateTime ReceivedDate { get; set; }

    public PurchaseRequestInput ToInput() {
        return new PurchaseRequestInput {
            RequesterUsername = Requester,
            ApproverName = Approver,
            Title = Title,
            Description = Description,
            AdditionalComments = AdditionalComments,
            ShippingType = ShippingType,
            PrUrl = PrUrl,
            Department = Department,
            Vendor = Vendor,
            PurchaseItems = PurchaseItems,
            Urgent = Urgent,
            Quotes = Quotes,
        };
    }
}
