namespace Domain.PurchaseRequests.Dto;

public class RequestMailMerge {
    public string PrAction { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Requester { get; set; }
    public string Approver { get; set; }
    public string PrLink { get; set; }
    public string AdditionalComments { get; set; }
}

public class ApproveMailMerge {
    public string PrAction { get; set; }
    public string Title { get; set; }
    public string Requester { get; set; }
    public string Approver { get; set; }
    public string PrLink { get; set; }
    public string CommentsTitle { get; set; }
    public string ApprovalComments { get; set; }
}

