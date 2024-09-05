using System.DirectoryServices.AccountManagement;
using Domain.Authentication;
using Domain.Settings;
using Microsoft.AspNetCore.Authentication;
using MongoDB.Driver;

namespace AuthApi.Services;

public class LoginService {
    private readonly SettingsService _settingsService;
    private readonly AuthDataService _authDataService;
    private readonly ILogger<LoginService> _logger;
    
    private LoginServerSettings _loginServerSettings;
    
    public LoginService(ILogger<LoginService> logger,AuthDataService authDataService,SettingsService settingsService) {
        this._settingsService = settingsService;
        this._authDataService = authDataService;
        this._logger = logger;
    }
    
    public async Task<UserSession?> Login(string username, string password) {
        if(await this.Auth(username,password)) {
            return await this._authDataService.LoginUserAccount(username);
        } else {
            return null;
        }
    }
    
    /*public async Task Logout(string username) {
        await this._authDataService.Logout();
    }*/

    private Task<bool> Auth(string username, string password) {
        try {
            using PrincipalContext context = new PrincipalContext(ContextType.Domain,
                this._loginServerSettings.HostIp,
                this._loginServerSettings.UserName, 
                this._loginServerSettings.Password);
            UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
            if (user != null) {
                if (context.ValidateCredentials(username,password)) {
                    this._logger.LogInformation("User authenticated. User: {User}",user.SamAccountName);
                    return Task.FromResult(true);
                } else {
                    this._logger.LogWarning("Login Failed. User: {User}",user.SamAccountName);
                    return Task.FromResult(true);
                }
            } else {
                this._logger.LogWarning("Username not found: {Username}",username);
                return Task.FromResult(true);
            }
        }catch(Exception e) {
            this._logger.LogError(e,"Error authenticating user");
            return Task.FromResult(false);
        }
    }

    public async Task LoadSettings() {
        this._loginServerSettings= await this._settingsService.GetLatestSettings();
    }
}