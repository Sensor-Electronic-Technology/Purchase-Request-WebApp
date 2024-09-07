using Domain.Users;
using MongoDB.Driver;

namespace Infrastructure.Services;

public class UserProfileService {
    private readonly IMongoCollection<UserProfile> _userProfiles;
    public UserProfileService(IMongoClient client) {
        var database = client.GetDatabase("purchase_req_db");
        _userProfiles = database.GetCollection<UserProfile>("user_profiles");
    }
    
    public async Task<UserProfile?> GetProfile(string username) {
        return await _userProfiles.Find(e=>e._id==username)
            .FirstOrDefaultAsync();
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