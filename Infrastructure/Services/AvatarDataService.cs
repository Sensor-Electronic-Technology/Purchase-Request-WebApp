using Domain.Assets;
using Domain.Settings;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using SetiFileStore.FileClient;

namespace Infrastructure.Services;

public class AvatarDataService {
    
    private readonly IMongoCollection<Avatar> _avatarsCollection;
    private readonly FileService _fileService;
    private readonly DatabaseSettings _settings;
    private IConfiguration _configuration;
    private readonly string _avatarDomain;
    
    public static List<string> Avatars { get; private set; }
    
    public AvatarDataService(IMongoClient mongoClient,
        FileService fileService,
        IConfiguration configuration,
        DatabaseSettings settings) {
        this._settings = settings;
        this._fileService = fileService;
        this._avatarDomain=configuration["AvatarDomain"] ?? "avatar";
        var database = mongoClient.GetDatabase(settings.AvatarDatabase);
        this._avatarsCollection = database.GetCollection<Avatar>(settings.AvatarCollection);
    }
    
    public IEnumerable<string> GetAvatars() {
        var directory = Directory.GetCurrentDirectory() + "/wwwroot/images/avatars/";
        if (!Directory.Exists(directory)) yield break;
        var fileNames=Directory.GetFiles(directory);
        foreach(var fileName in fileNames) {
            yield return $"images/avatars/{Path.GetFileName(fileName)}";
        }
    }

    public string? GetRandomAvatar() {
        var directory = Directory.GetCurrentDirectory() + "/wwwroot/avatars/";
        if (Directory.Exists(directory)) {
            Random random = new Random();
            var fileNames=Directory.GetFiles(directory);
            if (fileNames.Length == 0) return default;
            return $"/wwwroot/avatars/{Path.GetFileName(fileNames[random.Next(fileNames.Length)])}";
            //random.Next()
        }
        return default;
    }
    
    public async Task LoadAvatars() {
        var avatars=await this._avatarsCollection.Find(_=>true).ToListAsync();
        foreach(var avatar in avatars) {
            var directory = Directory.GetCurrentDirectory() + "/wwwroot/images/avatars/";
            var path=Path.Combine(directory,avatar.Name);
            if (File.Exists(path)) {
                continue;
            }
            var fileData=await this._fileService.DownloadFileStream(avatar.FileId,this._avatarDomain);
            if(fileData==null) continue;
            if(Directory.Exists(directory)==false) {
                Directory.CreateDirectory(directory);
            }
            await File.WriteAllBytesAsync(path,fileData.Data);
        }
    }
}