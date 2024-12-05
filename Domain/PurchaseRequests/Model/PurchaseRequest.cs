using Domain.PurchaseRequests.Dto;
using Domain.PurchaseRequests.Dto.ActionInputs;
using Domain.PurchaseRequests.TypeConstants;
using Domain.Users;
using MongoDB.Bson;

namespace Domain.PurchaseRequests.Model;

public class PurchaseRequest {
    public ObjectId _id { get; set; }
    public PrRequester Requester { get; set; } = null!;
    public PrApprover Approver { get; set; } = null!;
    public PrPurchaser? Purchaser { get; set; }
    public PrReceiver? Receiver { get; set; }
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
    public ApprovalResult? ApprovalResult { get; set; }
    public CheckInResult? CheckInResult { get; set; }
    public PrStatus Status { get; set; }
    public DateTime Created { get; set; }
    public DateTime ApprovedDate { get; set; }
    public DateTime RejectedDate { get; set; }
    public DateTime OrderedDate { get; set; }
    public DateTime ReceivedDate { get; set; }

    public PurchaseRequestInput ToInput() {
        return new PurchaseRequestInput {
            Id = _id,
            RequesterUsername = this.Requester?.Username,
            RequesterName = this.Requester?.Name,
            RequesterEmail = this.Requester?.Email,
            RequesterInitials = this.Requester?.Initials,
            ApproverName = this.Approver?.Name,
            ApproverEmail = this.Approver?.Email,
            ApproverId = this.Approver?.Username,
            Title = this.Title,
            Description = this.Description,
            AdditionalComments = this.AdditionalComments,
            ShippingType = this.ShippingType,
            PrUrl = this.PrUrl,
            Department = this.Department,
            Vendor = this.Vendor,
            PurchaseItems = this.PurchaseItems,
            Urgent = this.Urgent,
            Quotes = this.Quotes,
            EmailCcList = this.EmailCopyList,
            Created = this.Created,
            ApprovedDate = this.ApprovedDate,
            RejectedDate = this.RejectedDate,
            OrderedDate = this.OrderedDate,
            ReceivedDate = this.ReceivedDate,
            Status = this.Status
        };
    }
    
    public PurchaseRequestInput Repeat(UserProfile user) {
        return new PurchaseRequestInput {
            Id = _id,
            RequesterUsername = user._id,
            RequesterName = $"{user.FirstName} {user.LastName}",
            RequesterEmail = user.Email,
            RequesterInitials = $"{user.FirstName?[0]}{user.LastName?[0]}",
            ApproverName = string.Empty,
            ApproverEmail = string.Empty,
            ApproverId = string.Empty,
            ShippingType = this.ShippingType,
            Department = this.Department,
            Vendor = this.Vendor,
            PurchaseItems = this.PurchaseItems,
            Urgent = this.Urgent,
            Quotes = this.Quotes,
            EmailCcList = this.EmailCopyList,
            Status = PrStatus.NeedsApproval
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
        this.Created = input.Created;
        this.EmailCopyList=input.EmailCcList;
        this.ApprovedDate = input.ApprovedDate;
        this.RejectedDate = input.RejectedDate;
        this.OrderedDate = input.OrderedDate;
        this.ReceivedDate = input.ReceivedDate;
        this.Status = input.Status;
        return this;
    }
    
    public PurchaseOrderDto ToPurchaseOrderDto() {
        return new PurchaseOrderDto {
            RequestId = this._id,
            PoNumber = this.PurchaseOrder?.PoNumber ?? "",
            ShipTo = this.PurchaseOrder?.ShipTo ?? "",
            PaymentTerms = this.PurchaseOrder?.PaymentTerms ?? "",
            Department = this.Department,
            Description = this.Description,
            Vendor = this.Vendor,
            Requester = this.Requester,
            ShippingMethod = this.ShippingType,
            Items = this.PurchaseItems,
            Comments = this.PurchaseOrder?.PoComments ?? "",
            EmailCopyList = this.EmailCopyList,
            Date = this.OrderedDate,
        };
    }

    public PurchaseRequest? Clone() {
        return this.MemberwiseClone() as PurchaseRequest;
    }
}
