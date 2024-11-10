using MongoDB.Bson;

namespace Domain.ChatModel;

public class ChatRoom {
    public ObjectId _id { get; set; }
    public string OwnerId { get; set; }
    public string RoomName { get; set; }
    public string MemberId { get; set; }
    public Dictionary<string,ChatRoomMessage> Messages { get; set; }
}

public class ChatRoomMessage {
    public DateTime TimeStamp { get; set; }
    public string Message { get; set; }
}