using Ardalis.SmartEnum;
using MongoDB.Bson;

namespace Domain.Authentication;


public class Role:SmartEnum<Role,string> {
    public static readonly Role Admin = new Role(nameof(Admin), "Admin", "Admin role",new PurchaseRequestPermissions() {
        CanApprove = true,
        CanCreate = true,
        CanDelete = true,
        CanEdit = true,
        CanReject = true,
        CanView = true,
        CanReceive = true,
        CanOrder = true,
        CanAdminister = true
    });
    public static readonly Role User = new Role(nameof(User), "User", "User role",new PurchaseRequestPermissions() {
        CanCreate = true,
        CanDelete = true,
        CanEdit = true,
        CanView = true,
    });
    public static readonly Role Purchaser = new Role(nameof(Purchaser), "Purchaser", "Purchaser role",new PurchaseRequestPermissions() {
        CanCreate = true,
        CanEdit = true,
        CanDelete = true,
        CanView = true,
        CanReceive = true,
        CanOrder = true,
    });
    public static readonly Role Approver=new Role(nameof(Approver),"Approver","Approver role",new PurchaseRequestPermissions() {
        CanApprove = true,
        CanReject = true,
        CanView = true,
        CanCreate = true,
        CanDelete = true,
        CanEdit = true,
    });
    public static readonly Role Guest = new Role(nameof(Guest), "Guest", "Guest role",new PurchaseRequestPermissions() {
        CanView = true,
    });
    public string Description { get; private set; }
    public PurchaseRequestPermissions Permissions { get; private set; }
    private Role(string key, string name, string description,PurchaseRequestPermissions permissions) : base(key, name) {
        Description = description;
        Permissions = permissions;
    }
}

public class PurchaseRequestPermissions  {
    public bool CanApprove { get; set; } = false;
    public bool CanReject { get; set; }= false;
    public bool CanCreate { get; set; }= false;
    public bool CanDelete { get; set; }= false;
    public bool CanEdit { get; set; }= false;
    public bool CanView { get; set; }= false;
    public bool CanOrder { get; set; }= false;
    public bool CanReceive { get; set; }= false;
    public bool CanAdminister { get; set; }= false;
}