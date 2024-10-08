using Ardalis.SmartEnum;

namespace Domain.PurchaseRequests.TypeConstants;

public class ShippingTypes:SmartEnum<ShippingTypes, string> {
    public static readonly ShippingTypes Ground = new ShippingTypes(nameof(Ground), "Ground");
    public static readonly ShippingTypes TwoDay = new ShippingTypes(nameof(TwoDay), "2nd Day");
    public static readonly ShippingTypes Delivered = new ShippingTypes(nameof(Delivered), "Delivered");
    public static readonly ShippingTypes PickUp = new ShippingTypes(nameof(PickUp), "Pick-up");
    public static readonly ShippingTypes Service = new ShippingTypes(nameof(Service), "Service");

    private ShippingTypes(string name, string value) : base(name, value) { }
    
}