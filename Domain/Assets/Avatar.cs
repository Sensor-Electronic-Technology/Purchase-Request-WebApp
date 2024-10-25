using MongoDB.Bson;

namespace Domain.Assets;

public class Avatar {
    public ObjectId _id { get; set; }
    public string Name { get; set; }
    public string FileId { get; set; }
}