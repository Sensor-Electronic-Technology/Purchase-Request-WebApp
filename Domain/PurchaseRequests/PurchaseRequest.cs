using MongoDB.Bson;

namespace Domain.PurchaseRequests;

public class PurchaseRequest {
    public ObjectId _id { get; set; }
    public bool Approved { get; set; }
    public bool Ordered { get; set; }
    public bool Received { get; set; }
    public string FilePath { get; set; }
    
}