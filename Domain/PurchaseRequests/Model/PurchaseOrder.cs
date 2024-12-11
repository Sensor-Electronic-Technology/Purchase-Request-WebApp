using MongoDB.Bson;

namespace Domain.PurchaseRequests.Model;

public class PurchaseOrder {
    public string? PoNumber { get; set; }
    public string? ShipTo { get; set; }
    public string? PaymentTerms { get; set; }
    public string? PurchaseType { get; set; }
    public string? ItemType { get; set; }
    public string? PoComments { get; set; }
    public string? EmailComments { get; set; }
}