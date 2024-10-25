using System.Security.Claims;
using Infrastructure.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SETiAuth.Domain.Shared.Authentication;

namespace Webapp.Services.Authentication;

public class SetiAuthStateProvider : AuthenticationStateProvider {
    private readonly ProtectedLocalStorage _localStorage;
    private readonly UserService _userService;
    private readonly AvatarDataService _avatarDataService;

    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
    
    public SetiAuthStateProvider(ProtectedLocalStorage localStorage,UserService userService) {
        this._localStorage = localStorage;
        this._userService=userService;
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync() {
        try {
            var userSessionStorageResult = await _localStorage.GetAsync<UserSessionDto>("UserSession");
            var userSession = userSessionStorageResult.Success ? userSessionStorageResult.Value : null;
            if (userSession == null) {
                await this._userService.SetUser(_anonymous);
                return await Task.FromResult(new AuthenticationState(_anonymous));
            }
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
                new List<Claim> {
                    new Claim(ClaimTypes.Name, userSession.UserAccount.Username), 
                    new Claim(ClaimTypes.Role, userSession.UserAccount.Role),
                    new Claim("Token", userSession.Token)
                },"CustomAuth"));
            await this._userService.SetUser(claimsPrincipal,userSession);
            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        } catch {
            return await Task.FromResult(new AuthenticationState(_anonymous));
        }
    }

    public async Task UpdateAuthenticationState(UserSessionDto? userSession) {
        ClaimsPrincipal claimsPrincipal;
        if (userSession != null) {
            await _localStorage.SetAsync("UserSession", userSession);
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
                new List<Claim> {
                    new Claim(ClaimTypes.Name, userSession.UserAccount.Username), 
                    new Claim(ClaimTypes.Role, userSession.UserAccount.Role),
                    new Claim("Token", userSession.Token)
                },"CustomAuth"));
            await this._userService.SetUser(claimsPrincipal);
        } else {
            await this._userService.SetUser(this._anonymous);
            await this._localStorage.DeleteAsync("UserSession");
            claimsPrincipal = _anonymous;
        }
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }
}