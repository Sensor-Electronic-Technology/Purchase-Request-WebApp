using AuthApi.Services;
using Domain.Contracts;
using FastEndpoints;

namespace AuthApi.Endpoints;

public class UpdateEmailEndpoint:Endpoint<UpdateEmailRequest,UpdateEmailResponse> {
    private readonly AuthDataService _authDataService;
    private readonly ILogger<UpdateEmailEndpoint> _logger;
    
    public UpdateEmailEndpoint(AuthDataService authDataService,ILogger<UpdateEmailEndpoint> logger) {
        _authDataService = authDataService;
        this._logger = logger;
    }
    
    public override void Configure() {
        Put("/api/email/update");
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(UpdateEmailRequest req, CancellationToken ct) {
        var result= await _authDataService.UpdateUserEmail(req.Username,req.Email);
        await SendAsync(new UpdateEmailResponse(){Success = result},cancellation:ct);
    }
}