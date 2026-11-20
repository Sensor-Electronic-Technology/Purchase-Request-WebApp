using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Webapp.Data;

namespace Webapp.Services;

public class ReceiveMessageEventArgs : EventArgs {
    public string From { get; set; }
    public string Message { get; set; }
}

public class MessagingClient : IAsyncDisposable {
    private readonly ILogger<MessagingClient> _logger;
    private readonly KestrelCustomSettings _certSettings;
    private readonly IWebHostEnvironment _env;
    public HubConnection HubConnection { get; private set; }
    public bool IsConnected => HubConnection.State == HubConnectionState.Connected;
    private bool _isStarted;

    public MessagingClient(IOptions<KestrelCustomSettings> options, 
        ILogger<MessagingClient> logger,
        NavigationManager navigationManager,
        IWebHostEnvironment env) {
        _env = env;
        this._certSettings = options.Value;
        //not cert path defined in kubernetes deployment
        if (this._env.IsDevelopment()) {
            //development
            HubConnection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri(HubConstants.HubUrl))
                .WithAutomaticReconnect()
                .Build();
        } else {
            //production
            HubConnection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri(HubConstants.HubUrl), SignalRCertVerification)
                .WithAutomaticReconnect()
                .Build();
        }

        this._logger = logger;
        this._isStarted = false;
    }

    private void SignalRCertVerification(HttpConnectionOptions options) {
        var certPath = this._certSettings.Certificates.Default.Path;
        if (certPath != null) {
            Console.WriteLine($"Certificate path: {certPath}");
        }

        X509Certificate2 companyRootCa = X509CertificateLoader.LoadCertificateFromFile(
            certPath ?? "/secrets/certs/tls.crt");

        bool CustomValidator(object sender, X509Certificate? certificate, X509Chain? chain,
            SslPolicyErrors sslPolicyErrors) {
            if (chain == null || certificate == null) return false;
            chain.ChainPolicy.ExtraStore.Add(companyRootCa);
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            var element = new X509Certificate2(certificate);
            bool isValidChain = chain.Build(element);
            bool matchesCompanyRoot = chain.ChainElements[^1].Certificate.Thumbprint == companyRootCa.Thumbprint;
            return isValidChain && matchesCompanyRoot;
        }

        options.HttpMessageHandlerFactory = (innerHandler) => {
            if (innerHandler is HttpClientHandler clientHandler) {
                clientHandler.ServerCertificateCustomValidationCallback = CustomValidator;
            }

            return innerHandler;
        };
        options.WebSocketConfiguration = (webSocketOptions) => {
            webSocketOptions.RemoteCertificateValidationCallback = CustomValidator;
        };
        /*var customValidator =
            (sender, certificate, chain, sslPolicyErrors) => {
                if (chain == null || certificate == null) return false;
                chain.ChainPolicy.ExtraStore.Add(companyRootCA);
                chain.ChainPolicy.VerificationFlags =
                    X509VerificationFlags.AllowUnknownCertificateAuthority;
                var element = new X509Certificate2(certificate);
                bool isValidChain = chain.Build(element);
                bool matchesCompanyRoot =
                    chain.ChainElements[^1].Certificate.Thumbprint == companyRootCA.Thumbprint;
                return isValidChain && matchesCompanyRoot;
            };

        options.HttpMessageHandlerFactory = (innerHandler) => {
            if (innerHandler is HttpClientHandler clientHandler) {
                clientHandler.ServerCertificateCustomValidationCallback =
                    (m, c, ch, e) => CustomValidator(m, c, ch, e);
            }
            return innerHandler;
        };
        options.WebSocketConfiguration = (webSocketOptions) => {
            webSocketOptions.RemoteCertificateValidationCallback =
                (s, c, ch, e) => CustomValidator(s, c, ch, e);
        };*/
    }

    public async Task StartAsync() {
        if (this._isStarted) return;
        try {
            await HubConnection.StartAsync();
            this._isStarted = true;
        } catch (Exception e) {
            this._logger.LogError(e, "Failed to start messaging client");
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
        if (!this._isStarted) return;
        try {
            await HubConnection.StopAsync();
            this._isStarted = false;
        } catch (Exception e) {
            this._logger.LogError(e, "Failed to stop messaging client");
        }
    }

    public ValueTask DisposeAsync() {
        return HubConnection.DisposeAsync();
    }
}


/*HubConnection = new HubConnectionBuilder()
    .WithUrl(this._navigationManager.ToAbsoluteUri(HubConstants.HubUrl), options => {
        options.HttpMessageHandlerFactory = (innerHandler) => {
            if (innerHandler is HttpClientHandler clientHandler) {
                clientHandler.ServerCertificateCustomValidationCallback =
                    (message, cert, chain, sslPolicyErrors) => true; // Bypasses chain & name errors
            }

            return innerHandler;
        };
        options.WebSocketConfiguration = (webSocketOptions) => {
            webSocketOptions.RemoteCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true; // Forces trust on the WebSocket
        };
    })
    .WithAutomaticReconnect()
    .Build();*/