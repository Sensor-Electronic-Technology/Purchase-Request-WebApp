using MongoDB.Bson;

namespace Domain.PurchaseRequests.Model;

public class Department {
    public ObjectId _id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}