using Ardalis.SmartEnum;

namespace Domain.PurchaseRequests.TypeConstants;

public class ShippingType:SmartEnum<ShippingType, string> {
    public static readonly ShippingType Ground = new ShippingType(nameof(Ground), "Ground");
    public static readonly ShippingType TwoDay = new ShippingType(nameof(TwoDay), "2nd Day");
    public static readonly ShippingType Delivered = new ShippingType(nameof(Delivered), "Delivered");
    public static readonly ShippingType PickUp = new ShippingType(nameof(PickUp), "Pick-up");
    public static readonly ShippingType Service = new ShippingType(nameof(Service), "Service");

    private ShippingType(string name, string value) : base(name, value) { }
    
}