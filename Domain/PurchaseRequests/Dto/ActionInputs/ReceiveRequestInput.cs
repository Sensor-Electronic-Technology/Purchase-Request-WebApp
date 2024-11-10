using Domain.PurchaseRequests.Model;
using MongoDB.Bson;

namespace Domain.PurchaseRequests.Dto.ActionInputs;

public class ReceiveRequestInput {
    public ObjectId RequestId { get; set; }
    public PrReceiver? Receiver { get; set; }
    public bool Complete { get; set; }
    public byte[]? EmailDocument { get; set; }
    public List<ItemDeliveryStatus> ItemDelivery { get; set; } = [];
}