using MongoDB.Bson;

namespace Domain.Assets;

public class WebsiteAsset {
    public ObjectId _id { get; set; }
    public string Name { get; set; }
    public string FileId { get; set; }
}

/*public class Avatar:WebsiteAsset {
}

public class Template:WebsiteAsset {
    public PrUserAction UserActionType { get; set; }
}*/