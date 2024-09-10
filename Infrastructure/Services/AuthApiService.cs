using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using SETiAuth.Domain.Shared.Authentication;
using SETiAuth.Domain.Shared.Constants;
using SETiAuth.Domain.Shared.Contracts.Requests;
using SETiAuth.Domain.Shared.Contracts.Responses;

namespace Infrastructure.Services;
public class AuthApiService {
    private readonly HttpClient _client;
    private readonly IConfiguration _configuration;
    public AuthApiService(IConfiguration configuration) {
        this._client = new HttpClient();
        this._client.BaseAddress = new Uri(HttpClientConstants.LoginApiUrl);
        this._configuration = configuration;
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

    public async Task<List<UserAccountDto>> GetApprovers() {
        Console.WriteLine($"AuthDomain: {this._configuration["AuthDomain"]}");
        var response=this._client.PostAsJsonAsync(HttpClientConstants.GetUsersEndpoint,
            new GetUsersRequest() { AuthDomain = this._configuration["AuthDomain"], Role = "Approver" });
        /*if (!response.IsCompletedSuccessfully) {
            Console.WriteLine("Failed to fetch approvers");
            return [];
        }*/
        var usersResponse=await response.Result.Content.ReadFromJsonAsync<GetUsersResponse>();
        
        return usersResponse?.Users ?? [];
    }
    
    
    public async Task Logout(string? token) {
        await this._client.PostAsJsonAsync(HttpClientConstants.LogoutEndpoint,new LogoutRequest() {
            Token = token
        });
    }
}