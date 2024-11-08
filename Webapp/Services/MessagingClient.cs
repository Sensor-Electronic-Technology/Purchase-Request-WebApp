using Infrastructure.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Webapp.Services;

public class ReceiveMessageEventArgs : EventArgs {
    public string From { get; set; }
    public string Message { get; set; }
}

public class MessagingClient:IAsyncDisposable {
    private readonly ILogger<MessagingClient> _logger;
    private readonly NavigationManager _navigationManager;
    public HubConnection HubConnection { get; private set; }
    public bool IsConnected => HubConnection.State == HubConnectionState.Connected;
    private bool _isStarted;
    
    public event EventHandler<ReceiveMessageEventArgs>? OnReceiveMessage;
    public event EventHandler? OnReceiveRefresh;
    
    public MessagingClient(IConfiguration configuration,
        ILogger<MessagingClient> logger,
        NavigationManager navigationManager) {
        this._navigationManager = navigationManager;
        HubConnection = new HubConnectionBuilder()
            .WithUrl(this._navigationManager.ToAbsoluteUri(HubConstants.HubUrl))
            .WithAutomaticReconnect()
            .Build();
        this._logger = logger;
    }
    
    public async Task StartAsync() {
        if(this._isStarted) return;
        try {
            await HubConnection.StartAsync();
        }catch(Exception e) {
            this._logger.LogError(e,"Failed to start messaging client");
        }
    }

    public async Task SendRefresh(string? username) {
        if (this.IsConnected) {
            if (string.IsNullOrEmpty(username)) return;
            await this.HubConnection.SendAsync(HubConstants.Methods.SendRefresh, username);
        }
    }

    public async Task SendRefreshAll() {
        if (this.IsConnected) {
            await this.HubConnection.SendAsync(HubConstants.Methods.SendRefreshAll);
        }
    }

    public async Task Register(string username) {
        if (this.IsConnected) {
            await this.HubConnection.SendAsync(HubConstants.Methods.Register, username);
        }
    }
    
    public async Task StopAsync() {
        if(!this._isStarted) return;
        try {
            await HubConnection.StopAsync();
        }catch(Exception e) {
            this._logger.LogError(e,"Failed to stop messaging client");
        }
    }

    public ValueTask DisposeAsync() {
        return HubConnection.DisposeAsync();
    }
}