using MongoDB.Bson;

namespace Domain.Settings;

public class LoginServerSettings {
    public ObjectId _id { get; set; }
    public string HostIp { get; set; } = null!;
    public string UserName { get; set; }= null!;
    public string Password { get; set; }= null!;
    public bool IsLatest { get; set; } = false;
}