namespace Domain.PurchaseRequests.Dto;

public class RequestMailMerge {
    public string Title { get; set; }
    public string Description { get; set; }
    public string Requester { get; set; }
    public string Approver { get; set; }
    public string PrLink { get; set; }
    public string AdditionalComments { get; set; }
}