using Domain.PurchaseRequests.Model;
using Domain.PurchaseRequests.TypeConstants;
using MongoDB.Bson;

namespace Domain.PurchaseRequests.Dto;

public class ApproveRequestInput {
    public byte[]? EmailDocument { get; set; }
    public PurchaseRequestAction? Action { get; set; }
    public string? Comment { get; set; }
}