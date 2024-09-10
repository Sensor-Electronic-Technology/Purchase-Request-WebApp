namespace Domain.PurchaseRequests;

public class PurchaseRequestInput {
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? EmailMessage { get; set; }
    public string FilePath { get; set; }
    public bool Urgent { get; set; }
    public Approver Approver { get; set; }
}

public class Approver {
    public string Name { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
}