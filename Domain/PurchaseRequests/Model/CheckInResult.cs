namespace Domain.PurchaseRequests.Model;

public class CheckInResult {
    public bool Complete { get; set; }
    public List<ItemDeliveryStatus> ItemDelivery { get; set; } = [];
}