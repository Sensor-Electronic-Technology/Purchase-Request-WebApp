using AuthApi.Services;
using Domain.Contracts;
using FastEndpoints;

namespace AuthApi.Endpoints;

public class LoginEndpoint:Endpoint<LoginRequest,LoginResponse> {
    private readonly LoginService _loginService;
    private readonly ILogger<LoginEndpoint> _logger;
    
    public LoginEndpoint(LoginService loginService,ILogger<LoginEndpoint> logger) {
        _loginService = loginService;
        this._logger = logger;
    }
    
    public override void Configure() {
        Post("/api/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct) {
        var userSession = await _loginService.Login(req.Username,req.Password);
        if(userSession != null) {
            await SendAsync(new LoginResponse() { UserSession = userSession, Success = true }, cancellation: ct);
        } else {
            await SendAsync(new LoginResponse() {  Success = false, Message = "Invalid username or password" }, cancellation: ct);
        }
    }
}
