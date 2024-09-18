using MongoDB.Bson;

namespace Domain.PurchaseRequests.Model;

public class PurchaseOrder {
    public string _id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Department { get; set; }
    public string Description { get; set; }
    public Vendor Vendor { get; set; }
    public string ShipTo { get; set; }
    public string Requester { get; set; }
    public string ShippingMethod { get; set; }
    public List<PurchaseItem> Items { get; set; }
    public string FOB { get; set; }
    public string PaymentTerms { get; set; }
    public decimal TotalCost { get; set; }
    public ObjectId PurchaseRequestId { get; set; }
    public PoTracking Tracking { get; set; }
    public PurchaseOrderDocument PurchaseOrderDocument { get; set; }
    
}

public class PoTracking {
    public DateTime Approved { get; set; }
    public DateTime ToFinance { get; set; }
    public DateTime Ordered { get; set; }
    public DateTime Received { get; set; }
}