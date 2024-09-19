using MongoDB.Bson;

namespace Domain.PurchaseRequests.Model;

public class PurchaseOrder {
    public string? PoNumber { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? ShipTo { get; set; }
    public string? PaymentTerms { get; set; }
    public PoTracking Tracking { get; set; } = new();
}

public class PoTracking {
    public DateTime ToFinance { get; set; }
    public DateTime Ordered { get; set; }
    public DateTime Received { get; set; }
}