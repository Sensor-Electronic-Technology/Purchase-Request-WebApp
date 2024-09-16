using Ardalis.SmartEnum;
namespace Domain.PurchaseRequests;

public class EmailType:SmartEnum<EmailType,string> {
    public static readonly EmailType NeedsApproval=new(nameof(NeedsApproval),"Purchase Request Needs Approval");
    public static readonly EmailType NeedsPurchase=new(nameof(NeedsPurchase),"Purchase Request Approved");
    public static readonly EmailType Rejected=new(nameof(Rejected),"Purchase Request Rejected");
    public static readonly EmailType Ordered=new(nameof(Ordered),"Purchase Request Ordered");
    public static readonly EmailType Received=new(nameof(Received),"Order Received");
    
    public EmailType(string name, string value) : base(name, value) { }
}