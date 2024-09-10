using Ardalis.SmartEnum;

namespace Domain.Authentication;

public class PurchaseRequestRole:SmartEnum<PurchaseRequestRole,string> {
    public static readonly PurchaseRequestRole Admin = new PurchaseRequestRole(nameof(Admin), "SETI-A-PurchaseRequest-Admin",new PurchaseRequestPermissions() {
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
    public static readonly PurchaseRequestRole Requester = new PurchaseRequestRole(nameof(Requester), "SETI-A-PurchaseRequest-Requester",new PurchaseRequestPermissions() {
        CanCreate = true,
        CanDelete = true,
        CanEdit = true,
        CanView = true,
    });
    public static readonly PurchaseRequestRole Purchaser = new PurchaseRequestRole(nameof(Purchaser), "SETI-A-PurchaseRequest-Purchaser",new PurchaseRequestPermissions() {
        CanCreate = true,
        CanEdit = true,
        CanDelete = true,
        CanView = true,
        CanReceive = true,
        CanOrder = true,
    });
    public static readonly PurchaseRequestRole Approver=new PurchaseRequestRole(nameof(Approver),"SETI-A-PurchaseRequest-Approver",new PurchaseRequestPermissions() {
        CanApprove = true,
        CanReject = true,
        CanView = true,
        CanCreate = true,
        CanDelete = true,
        CanEdit = true,
    });
    public PurchaseRequestPermissions Permissions { get; private set; }
    public static string AuthDomain { get; } = "SETI-A-PurchaseRequest";
    private PurchaseRequestRole(string name, string value,PurchaseRequestPermissions permissions) : base(name, value) {
        this.Permissions = permissions;
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