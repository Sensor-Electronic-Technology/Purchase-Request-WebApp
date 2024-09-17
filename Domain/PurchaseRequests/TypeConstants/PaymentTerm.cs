using Ardalis.SmartEnum;

namespace Domain.PurchaseRequests.TypeConstants;

public class PaymentTerm:SmartEnum<PaymentTerm, string> {
    public static readonly PaymentTerm FOB = new PaymentTerm(nameof(FOB), "FOB");
    public static readonly PaymentTerm CIF = new PaymentTerm(nameof(CIF), "CIF");
    public static readonly PaymentTerm NetTerms = new PaymentTerm(nameof(NetTerms), "Net Terms 30day");
    public static readonly PaymentTerm CreditCard = new PaymentTerm(nameof(CreditCard), "Credit Card");
    public static readonly PaymentTerm Other = new PaymentTerm(nameof(Other), "Other");

    private PaymentTerm(string name, string value) : base(name, value) { }
    
}