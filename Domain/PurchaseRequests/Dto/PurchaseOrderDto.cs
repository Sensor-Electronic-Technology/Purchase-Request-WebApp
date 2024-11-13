using Domain.PurchaseRequests.Model;
using Domain.PurchaseRequests.Pdf;
using MongoDB.Bson;

namespace Domain.PurchaseRequests.Dto;

public class PurchaseOrderDto {
    public ObjectId RequestId { get; set; }
    public string PoNumber { get; set; }
    public DateTime Date { get; set; }
    public Department? Department { get; set; }
    public string? Description { get; set; }
    public Vendor? Vendor { get; set; }
    public Contact? ToAddress { get; set; }
    public string? ShipTo { get; set; }
    public PrRequester? Requester { get; set; }
    public PrPurchaser? Purchaser { get; set; }
    public string? ShippingMethod { get; set; }
    public List<PurchaseItem> Items { get; set; }
    public List<string> EmailCopyList { get; set; }
    public string? FOB { get; set; }
    public string? PaymentTerms { get; set; }
    public string? PurchaseType { get; set; }
    public string? ItemType { get; set; }
    public decimal TotalCost=>this.Items.Sum(x=>x.TotalCost);
    public string? Comments { get; set; }
}