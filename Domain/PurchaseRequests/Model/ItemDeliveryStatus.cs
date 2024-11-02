namespace Domain.PurchaseRequests.Model;

public class ItemDeliveryStatus {
    public string? Item { get; set; }
    public bool Received { get; set; }
    public string? Location { get; set; }
    public DateTime ReceivedDate { get; set; }
}