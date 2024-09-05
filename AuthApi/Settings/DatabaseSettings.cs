namespace AuthApi.Settings;

public class DatabaseSettings {
    public string SettingsDatabase { get; set; }
    public string LoginSettingsCollection { get; set; }
    public string AuthenticationDatabase { get; set; }
    public string UserCollection { get; set; }
    public string SessionCollection { get; set; }
}