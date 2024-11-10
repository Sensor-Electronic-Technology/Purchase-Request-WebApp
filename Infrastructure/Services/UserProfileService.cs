using Domain.Settings;
using Domain.Users;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infrastructure.Services;

public class UserProfileService {
    private readonly IMongoCollection<UserProfile> _userProfiles;
    public UserProfileService(IMongoClient client,IOptions<DatabaseSettings> options) {
        var database = client.GetDatabase(options.Value.PurchaseRequestDatabase ?? "purchase_req_db");
        this._userProfiles = database.GetCollection<UserProfile>(options.Value.UserProfileCollection ?? "user_profiles");
    }
    
    public async Task<UserProfile?> GetProfile(string username) {
        return await _userProfiles.Find(e=>e._id==username)
            .FirstOrDefaultAsync();
    }
    
    public async Task<List<UserProfile>> GetProfiles() {
        return await _userProfiles.Find(e=>true)
            .ToListAsync();
    }
    
    public async Task<bool> ProfileExists(string username) {
        return await _userProfiles.Find(e=>e._id==username)
            .AnyAsync();
    }
    
    public async Task<UserProfile> CreateProfile(UserProfile profile) {
        await _userProfiles.InsertOneAsync(profile);
        return profile;
    }
    
    public async Task<bool> UpdateProfile(UserProfile profile) {
        var result=await _userProfiles.ReplaceOneAsync(e=>e._id==profile._id, profile);
        return result.IsAcknowledged && result.ModifiedCount>0;
    }
}