using Domain.PurchaseRequests.Dto;
using Domain.PurchaseRequests.TypeConstants;
using MongoDB.Bson;

namespace Domain.PurchaseRequests.Model;

public class PurchaseRequest {
    public ObjectId _id { get; set; }
    public PrRequester Requester { get; set; } = null!;
    public PrApprover Approver { get; set; } = null!;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AdditionalComments { get; set; }
    public string? ShippingType { get; set; }
    public string? PrUrl { get; set; }
    public Department? Department { get; set; }
    public Vendor? Vendor { get; set; }
    public List<PurchaseItem> PurchaseItems { get; set; } = [];
    public decimal TotalCost=>PurchaseItems.Sum(x=>x.TotalCost);
    public List<string> EmailCopyList { get; set; } = [];
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
            Id = _id,
            RequesterUsername = Requester?.Username,
            RequesterName = Requester?.Name,
            RequesterEmail = Requester?.Email,
            ApproverName = Approver?.Name,
            ApproverEmail = Approver?.Email,
            ApproverId = Approver?.Username,
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

    public PurchaseRequest FromInput(PurchaseRequestInput input) {
        this._id = input.Id ?? ObjectId.GenerateNewId();
        this.Requester = new PrRequester {
            Email = input.RequesterEmail,
            Name = input.RequesterName,
            Username = input.RequesterUsername,
        };
        this.Approver = new PrApprover {
            Email = input.ApproverEmail,
            Name = input.ApproverName,
            Username = input.ApproverId,
        };
        this.Title = input.Title;
        this.Description = input.Description;
        this.AdditionalComments = input.AdditionalComments;
        this.ShippingType = input.ShippingType;
        this.PrUrl = input.PrUrl;
        this.Department = input.Department;
        this.Vendor = input.Vendor;
        this.PurchaseItems = input.PurchaseItems;
        this.Urgent = input.Urgent;
        this.Quotes = input.Quotes;
        return this;
    }
}
