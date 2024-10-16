namespace Domain.PurchaseRequests.Model;

public class PurchaseItem {
    public int Quantity { get; set; }
    public string ProductName { get; set; }
    public string? Hyperlink { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
}