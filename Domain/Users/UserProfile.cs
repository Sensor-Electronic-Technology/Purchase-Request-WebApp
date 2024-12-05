using MongoDB.Bson;

namespace Domain.Users;

public class UserProfile {
    public string _id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? AvatarPath { get; set; }
    public string? Role { get; set; }
    public UserProfileDefaults? Defaults { get; set; }
}


public class UserProfileDefaults {
    public string? ApproverUsername { get; set; }
    public string? Department { get; set; }
}