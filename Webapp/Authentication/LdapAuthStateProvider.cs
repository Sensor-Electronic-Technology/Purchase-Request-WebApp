using System.Security.Claims;
using Domain.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Visus.Ldap;

namespace Webapp.Authentication;

public class LdapAuthStateProvider : AuthenticationStateProvider {
    private readonly ProtectedSessionStorage _sessionStorage;
    // _anonymous for unautheticated user. a "claim" is a piece of information about a user or system entity.
    // ClaimsPrincipal is used to represent an anonymous (unauthenticated) user, and designed to work with claims-based identity systems
    // A ClaimsPrincipal can be composed of multiple ClaimsIdentity instances. 
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    public LdapAuthStateProvider(ProtectedSessionStorage sessionStorage) {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync() {
        try {
            var userSessionStorageResult = await _sessionStorage.GetAsync<UserSession>("UserSession");
            var userSession = userSessionStorageResult.Success ? userSessionStorageResult.Value : null;
            Claim claim= new Claim(ClaimTypes.Name, userSession?.Username ?? "Anonymous");
            if (userSession == null)
                return await Task.FromResult(new AuthenticationState(_anonymous));
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
                new List<Claim> {
                    new Claim(ClaimTypes.Name, userSession.Username), new Claim(ClaimTypes.Role, userSession.Role)
                },"CustomAuth"));
            return await Task.FromResult(new AuthenticationState(claimsPrincipal));
        } catch {
            return await Task.FromResult(new AuthenticationState(_anonymous));
        }
    }

    public async Task UpdateAuthenticationState(UserSession? userSession) {
        ClaimsPrincipal claimsPrincipal;
        if (userSession != null) {
            await _sessionStorage.SetAsync("UserSession", userSession);
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> {
                new Claim(ClaimTypes.Name, userSession.Username), new Claim(ClaimTypes.Role, userSession.Role)
            }));
        } else {
            await _sessionStorage.DeleteAsync("UserSession");
            claimsPrincipal = _anonymous;
        }
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }
}