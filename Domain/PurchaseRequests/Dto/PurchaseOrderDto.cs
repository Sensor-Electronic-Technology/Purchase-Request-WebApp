using Domain.PurchaseRequests.Model;
using Domain.PurchaseRequests.Pdf;

namespace Domain.PurchaseRequests.Dto;

public class PurchaseOrderDto {
    public string PoNumber { get; set; }
    public DateTime Date { get; set; }
    public string Department { get; set; }
    public string Description { get; set; }
    public Vendor? Vendor { get; set; }
    public Contact? ToAddress { get; set; }
    public string ShipTo { get; set; }
    public string Requester { get; set; }
    public string ShippingMethod { get; set; }
    public List<PurchaseItem> Items { get; set; }
    public string FOB { get; set; }
    public string PaymentTerms { get; set; }
    public decimal TotalCost { get; set; }
    public string Comments { get; set; }
}