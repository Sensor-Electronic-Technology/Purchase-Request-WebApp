using Ardalis.SmartEnum;
namespace Domain.PurchaseRequests.TypeConstants;

public class PurchaseType:SmartEnum<PurchaseType,string> {
    public static readonly PurchaseType PurchaseOrderType=new(nameof(PurchaseOrderType),"Purchase Order");
    public static readonly PurchaseType PurchaseCreditType=new(nameof(PurchaseCreditType),"Purchase Card");
    public static readonly PurchaseType OtherType=new(nameof(OtherType),"Other");
    
    public PurchaseType(string name, string value) : base(name, value) { }
}