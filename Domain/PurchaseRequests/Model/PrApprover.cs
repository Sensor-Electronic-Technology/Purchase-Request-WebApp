namespace Domain.PurchaseRequests.Model;

public abstract class PrUser {
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
}

public class PrApprover:PrUser {
}

public class PrPurchaser:PrUser { }

public class PrRequester:PrUser {
    public string? Initials { get; set; }
}

public class PrReceiver:PrUser { }