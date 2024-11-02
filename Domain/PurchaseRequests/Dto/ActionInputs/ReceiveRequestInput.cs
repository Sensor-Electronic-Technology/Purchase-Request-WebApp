using Domain.PurchaseRequests.Model;
using MongoDB.Bson;

namespace Domain.PurchaseRequests.Dto.ActionInputs;

public class ReceiveRequestInput {
    public ObjectId RequestId { get; set; }
    public bool IsPartial { get; set; }
    public byte[]? EmailDocument { get; set; }
    public List<ItemDeliveryStatus> ItemDelivery { get; set; } = [];
}