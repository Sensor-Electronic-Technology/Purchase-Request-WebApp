using Ardalis.SmartEnum;

namespace Domain.PurchaseRequests.TypeConstants;
public class PurchaseRequestAction:SmartEnum<PurchaseRequestAction,int> {
    public static readonly PurchaseRequestAction Approve = new(nameof(Approve),1);
    public static readonly PurchaseRequestAction Reject = new(nameof(Reject),2);
    public static readonly PurchaseRequestAction Cancel = new(nameof(Cancel),3);
    public static readonly PurchaseRequestAction Order = new(nameof(Order),4);
    public static readonly PurchaseRequestAction Receive = new(nameof(Receive),5);
    
    public PurchaseRequestAction(string name, int value) : base(name, value) { }
}



