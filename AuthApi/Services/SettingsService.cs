using Domain.Settings;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AuthApi.Services;

public class SettingsService {
    private readonly IMongoCollection<LoginServerSettings> _settingsCollection;
    public SettingsService(IMongoClient client) {
        var database = client.GetDatabase("settings_db");
        _settingsCollection = database.GetCollection<LoginServerSettings>("login_settings");
    }
    
    public async Task<LoginServerSettings> GetLatestSettings() {
        var settings = await _settingsCollection.Find(s => s.IsLatest).FirstOrDefaultAsync();
        return settings;
    }
    
    public async Task AddLatestSetting(LoginServerSettings settings) {
       await ClearLatestSetting(); 
       settings.IsLatest = true; 
       settings._id = ObjectId.GenerateNewId();
       await _settingsCollection.InsertOneAsync(settings);
    }
    
    private async Task ClearLatestSetting() {
        var filter= Builders<LoginServerSettings>.Filter.Eq(s => s.IsLatest, true);
        var update = Builders<LoginServerSettings>.Update.Set(s => s.IsLatest, false);
        await _settingsCollection.UpdateManyAsync(filter, update);
    }
}