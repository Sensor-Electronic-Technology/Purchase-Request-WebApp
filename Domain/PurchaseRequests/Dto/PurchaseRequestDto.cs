namespace Domain.PurchaseRequests.Dto;

public class PurchaseRequestDto {
    public string? Requester { get; set; }
    public string? Approver { get; set; } = null!;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AdditionalComments { get; set; }
    public string? ShippingType { get; set; }
    public string? Department { get; set; }
    public string? Vendor { get; set; }
}