using SETiAuth.Domain.Shared.Authentication;
using SETiAuth.Domain.Shared.Constants;
using SETiAuth.Domain.Shared.Contracts.Requests;
using SETiAuth.Domain.Shared.Contracts.Responses;

namespace Webapp.Services;
public class AuthService {
    private readonly HttpClient _client;

    public AuthService() {
        this._client = new HttpClient();
        this._client.BaseAddress = new Uri(HttpClientConstants.LoginApiUrl);
        
    }

    public async Task<UserSessionDto?> Login(string username,string password,bool isDomainUser) {
        var response=await this._client.PostAsJsonAsync(HttpClientConstants.LoginEndpoint,new LoginRequest() {
            Username = username,
            Password = password,
            AuthDomain = "PurchaseRequestSystem",
            IsDomainUser = isDomainUser
        });
        if (response.IsSuccessStatusCode) {
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return loginResponse?.UserSession;
        } else {
            return null;
        }
    }
    
    public async Task Logout(string? token) {
        await this._client.PostAsJsonAsync(HttpClientConstants.LogoutEndpoint,new LogoutRequest() {
            Token = token
        });
    }
}