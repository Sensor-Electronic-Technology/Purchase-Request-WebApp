using Domain.PurchaseRequests.Model;
using Domain.PurchaseRequests.TypeConstants;
using MongoDB.Bson;

namespace Domain.PurchaseRequests.Dto.ActionInputs;



public class PurchaseRequestInput {
    public ObjectId? Id { get; set; }
    public string? ApproverName { get; set; }
    public string? ApproverEmail { get; set; }
    public string? ApproverId { get; set; }
    public string? RequesterName { get; set; }
    public string? RequesterUsername { get; set; }
    public string? RequesterEmail { get; set; }
    public string? RequesterInitials { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AdditionalComments { get; set; }
    public string? ShippingType { get; set; }
    public string? PrUrl { get; set; }
    public PrStatus Status { get; set; }
    public byte[]? TempFile { get; set; }=[];
    public byte[]? EmailTemplate { get; set; }=[];
    public List<string> EmailCcList { get; set; } = [];
    public Vendor? Vendor { get; set; }
    public Department? Department { get; set; }
    public List<PurchaseItem> PurchaseItems { get; set; } = [];
    public decimal TotalCost => PurchaseItems.Sum(x => x.TotalCost);
    public bool Urgent { get; set; }
    public List<string> Quotes { get; set; } = [];
    public List<FileInput> Attachments { get; set; } = [];
    public DateTime Created { get; set; }
    public DateTime ApprovedDate { get; set; }
    public DateTime RejectedDate { get; set; }
    public DateTime OrderedDate { get; set; }
    public DateTime ReceivedDate { get; set; }
}
