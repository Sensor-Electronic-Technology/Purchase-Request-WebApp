using Ardalis.SmartEnum;

namespace Domain.PurchaseRequests.TypeConstants;

public class ItemType:SmartEnum<ItemType,string> {
    public static readonly ItemType Inventory=new(nameof(Inventory),"Inventory");
    public static readonly ItemType Expansion=new(nameof(Expansion),"Expansion");
    public static readonly ItemType RawMaterials=new(nameof(RawMaterials),"Raw materials");
    public static readonly ItemType SparePartsInventory=new(nameof(SparePartsInventory),"Spare Parts Inventory");
    public static readonly ItemType Other=new(nameof(Other),"Other");
    public ItemType(string name, string value) : base(name, value) { }
}