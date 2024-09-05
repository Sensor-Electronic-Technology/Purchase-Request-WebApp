using MongoDB.Bson;

namespace Domain.Authentication;

public class UserAccount {
    public ObjectId _id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public bool IsDomainAccount { get; set; }
}