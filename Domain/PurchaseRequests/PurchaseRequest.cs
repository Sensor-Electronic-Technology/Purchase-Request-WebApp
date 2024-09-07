using MongoDB.Bson;

namespace Domain.PurchaseRequests;

public class PurchaseRequest {
    public ObjectId _id { get; set; }
    public ObjectId UserProfileId { get; set; }
    public ObjectId? ApprovedBy { get; set; }
    public string? Title { get; set; }
    public DateTime Created { get; set; }
    public bool? Approved { get; set; }
    public DateTime ApprovedDate { get; set; }
    public bool? Ordered { get; set; }
    public DateTime OrderedDate { get; set; }
    public bool? Received { get; set; }
    public DateTime ReceivedDate { get; set; }
    public string? FilePath { get; set; }
    public bool Urgent { get; set; }
}

public class PurchaseRequestDto {
    public string UserName { get; set; }
    public string? Title { get; set; }
    
    public bool? Approved { get; set; }
    public bool? Ordered { get; set; }
    public bool? Received { get; set; }
    public string? FilePath { get; set; }
}