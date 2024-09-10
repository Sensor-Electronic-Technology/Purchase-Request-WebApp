using Infrastructure.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Webapp.Services.Authentication;

namespace Webapp.Services;

internal sealed class UserCircuitHandler:CircuitHandler,IDisposable {
    
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly UserService _userService;
    private readonly AuthApiService _authApiService;
    private readonly ILogger<UserCircuitHandler> _logger;
    
    public UserCircuitHandler(AuthenticationStateProvider authenticationStateProvider, UserService userService, 
        AuthApiService authApiService,ILogger<UserCircuitHandler> logger) {
        this._authenticationStateProvider = authenticationStateProvider;
        this._userService = userService;
        this._authApiService = authApiService;
        this._logger = logger;
    }
    
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken) {
        _authenticationStateProvider.AuthenticationStateChanged += AuthenticationChanged;
        return base.OnCircuitOpenedAsync(circuit, cancellationToken);
    }
    
    private void AuthenticationChanged(Task<AuthenticationState> task) {
        _ = UpdateAuthentication(task);
        async Task UpdateAuthentication(Task<AuthenticationState> task) {
            try {
                var state = await task;
                await this._userService.SetUser(state.User);
            }catch{}
        }
    } 
    
    public override async Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken) {
        var state = await this._authenticationStateProvider.GetAuthenticationStateAsync();
        await this._userService.SetUser(state.User);
    }
    
    public void Dispose() {
        this._authenticationStateProvider.AuthenticationStateChanged -= AuthenticationChanged;
    }
}