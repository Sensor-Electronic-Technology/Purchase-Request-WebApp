
using Domain.Shared;
using Domain.Shared.Authentication;
using Domain.Shared.Constants;
using Domain.Shared.Contracts.Requests;
using Domain.Shared.Contracts.Responses;

namespace Webapp.Services;


public class AuthService {
    private HttpClient _client;

    public AuthService() {
        this._client = new HttpClient();
        this._client.BaseAddress = new Uri("http://172.20.4.20:5000/api/");
        
    }

    public async Task<UserSessionDto?> Login(string username,string password,bool isLocal) {
        var response=await this._client.PostAsJsonAsync(HttpClientConstants.LoginEndpoint,new LoginRequest() {
            Username = username,
            Password = password,
            AuthDomain = "PurchaseRequestSystem",
            IsDomainUser = true
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