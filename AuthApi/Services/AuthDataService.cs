using AuthApi.Settings;
using Domain.Authentication;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AuthApi.Services;

public class AuthDataService {
    private readonly IMongoCollection<UserSession> _userSessionCollection;
    private readonly IMongoCollection<UserAccount> _userAccountCollection;
    
    public AuthDataService(IMongoClient client,IOptions<DatabaseSettings> settings) {
        IMongoDatabase database = client.GetDatabase(settings.Value.AuthenticationDatabase ?? "auth_db");
        this._userSessionCollection = database.GetCollection<UserSession>(settings.Value.SessionCollection ?? "user_sessions");
        this._userAccountCollection = database.GetCollection<UserAccount>(settings.Value.UserCollection ?? "user_accounts");
    }
    
    public async Task<UserSession> LoginUserAccount(string username) {
        var userAccount = await this._userAccountCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
        if (userAccount == null) {
            UserAccount account = new UserAccount();
            account.Username = username;
            account.Role = "guest";
            account._id = ObjectId.GenerateNewId();
            await this._userAccountCollection.InsertOneAsync(account);
            userAccount = account;
        }
        var userSession = new UserSession {
            _id=ObjectId.GenerateNewId(),
            Username = userAccount.Username,
            Role = userAccount.Role,
            LoginTime = DateTime.Now
        };
        await this._userSessionCollection.InsertOneAsync(userSession);
        return userSession;
    }

    public async Task<bool> UpdateUserEmail(string username,string email) {
        var filter=Builders<UserAccount>.Filter.Eq(u => u.Username, username);
        var update=Builders<UserAccount>.Update.Set(u => u.Email, email);
        var result=await this._userAccountCollection.UpdateOneAsync(filter, update);
        return result.IsAcknowledged && result.ModifiedCount>0;
    }
    
    public async Task Logout(ObjectId sessionId) {
        var filter=Builders<UserSession>.Filter.Eq(s => s._id, sessionId);
        var update=Builders<UserSession>.Update.Set(s => s.LogoutTime, DateTime.Now);
        await this._userSessionCollection.UpdateOneAsync(filter, update);
    }
}