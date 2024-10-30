using Domain.PurchaseRequests.Model;
using Domain.PurchaseRequests.TypeConstants;

namespace Domain.PurchaseRequests.Dto.ActionInputs;

public class OrderRequestInput {
    public byte[]? EmailDocument { get; set; }
    public PurchaseRequestAction? Action { get; set; }
    public List<string> ExternalEmails { get; set; } = new();
    public List<string> ExternalCopyEmails { get; set; } = new();
    public bool IncludeInternalEmails { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = new();
    public bool SendExternalEmail { get; set; }
    public string? Comment { get; set; }
}