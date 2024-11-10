using Domain.ChatModel;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Services;

public record ChatRoomDto(ObjectId Id, string Name);

public class ChatRoomDataService {
    private readonly IMongoCollection<ChatRoom> _chatRoomCollection;
    
    public ChatRoomDataService(IMongoClient client) {
        var database = client.GetDatabase("chat_db");
        this._chatRoomCollection = database.GetCollection<ChatRoom>("ChatRooms");
    }
    
    public async Task<ChatRoom?> GetChatRoom(ObjectId id) {
        return await this._chatRoomCollection.Find(c => c._id == id).FirstOrDefaultAsync();
    }
    
    public async Task<ChatRoomDto?> GetChatRoomDto(ObjectId id) {
        return await this._chatRoomCollection.Find(c => c._id == id)
            .Project(e=>new ChatRoomDto(e._id,e.RoomName))
            .FirstOrDefaultAsync();
    }
    
    public async Task<List<ChatRoom>> GetChatRooms() {
        return await this._chatRoomCollection.Find(e=>true).ToListAsync();
    }
    
    public async Task<List<ChatRoomDto>?> GetUserChatRooms(string userId) {
        var filter=Builders<ChatRoom>.Filter.Or(
            Builders<ChatRoom>.Filter.Eq(e=>e.OwnerId, userId), 
            Builders<ChatRoom>.Filter.And(
                Builders<ChatRoom>.Filter.Ne(e=>e.OwnerId, userId),
                Builders<ChatRoom>.Filter.Eq(e=>e.MemberId, userId)));
        return await this._chatRoomCollection.Find(filter)
            .Project(e=>new ChatRoomDto(e._id,e.RoomName))
            .ToListAsync();
    }
    
    public async Task<ChatRoomDto?> CreateChatRoom(string owner,string memberId) {
        var chatRoom = new ChatRoom {
            _id = ObjectId.GenerateNewId(),
            OwnerId = owner,
            MemberId = memberId,
            RoomName = $"{owner}->{memberId}",
            Messages = new Dictionary<string, ChatRoomMessage>()
        };
        await this._chatRoomCollection.InsertOneAsync(chatRoom);
        return await this._chatRoomCollection.Find(e=>e._id==chatRoom._id)
            .Project(e=>new ChatRoomDto(e._id,e.RoomName))
            .FirstOrDefaultAsync();
    }
    
    public async Task AddMessage(ObjectId roomId,string username, ChatRoomMessage message) {
        var update = Builders<ChatRoom>.Update.Push(e=>e.Messages, 
            new KeyValuePair<string, ChatRoomMessage>(username, message));
        await this._chatRoomCollection.UpdateOneAsync(e=>e._id==roomId, update);
    }
    
    public async Task AddMessage(string ownerId,string username, ChatRoomMessage message) {
        var update = Builders<ChatRoom>.Update.Push(e=>e.Messages, 
            new KeyValuePair<string, ChatRoomMessage>(username, message));
        await this._chatRoomCollection.UpdateOneAsync(e=>e.OwnerId==ownerId, update);
    }
    
}