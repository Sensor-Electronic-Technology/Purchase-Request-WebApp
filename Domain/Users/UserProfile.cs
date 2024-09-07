using MongoDB.Bson;

namespace Domain.Users;

public class UserProfile {
    public string _id { get; set; }
    public string? FirstName { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public List<ObjectId> PurchaseRequests { get; set; } = [];
}