using MongoDB.Bson;

namespace Domain.PurchaseRequests.Model;

public class PoNumber {
    public string _id { get; set; }
    public int Seq { get; set; }
    public int Year { get; set; }
    public string? Initials { get; set; }
    public ObjectId RequestId { get; set;}
}