using Domain.PurchaseRequests.TypeConstants;

namespace Domain.PurchaseRequests.Dto.ActionInputs;

public class ApproveRequestInput {
    public byte[]? EmailDocument { get; set; }
    public PurchaseRequestAction? Action { get; set; }
    public string? Comment { get; set; }
}