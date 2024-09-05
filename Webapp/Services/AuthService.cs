using Domain.Contracts;
namespace Webapp.Services;


public class AuthService {
    private HttpClient _client;

    public AuthService() {
        this._client = new HttpClient();
        this._client.BaseAddress = new Uri("http://172.20.4.50:5000/api");
    }
    
    public async Task<bool> Login(string username, string password) {
        var response = await _client.PostAsJsonAsync("/login",new LoginRequest(){Username = username, Password = password,IsDomainUser = true});
        
        return response.IsSuccessStatusCode;
    }
}