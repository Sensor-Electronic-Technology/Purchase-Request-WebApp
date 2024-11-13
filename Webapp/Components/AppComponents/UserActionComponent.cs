using Domain.PurchaseRequests.Model;
using Domain.PurchaseRequests.TypeConstants;
using Domain.Users;
using Infrastructure.Hubs;
using Infrastructure.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Radzen;
using Webapp.Services;

namespace Webapp.Components.AppComponents;

public abstract class UserActionComponent:ComponentBase {
    [Inject] protected MessagingClient _messagingClient { get; set; }
    [Inject] protected NotificationService _notificationService { get; set; }
    [Inject] protected PurchaseRequestService _purchaseRequestService { get; set; }
    [Inject] protected IWebHostEnvironment _environment { get; set; }
    [Inject] protected SpinnerService _spinnerService { get; set; }
    [Parameter] public UserProfile UserProfile { get; set; }
    [Parameter] public EventCallback<string> ActionCompleted { get; set; }
    [Parameter] public PurchaseRequest? PurchaseRequest { get; set; } 
    
    protected abstract Task SubmitHandler();
    protected abstract Task CancelHandler();
    
    protected override Task OnInitializedAsync() {
        this._messagingClient.HubConnection.On(HubConstants.Events.ReceiveRefresh,this.RefreshHandler);
        return base.OnInitializedAsync();
    }

    protected async Task SendRefresh() {
        await this._messagingClient.SendRefreshAll();
    }
    
    protected virtual Task RefreshHandler() {
        this.ShowNotification(NotificationSeverity.Warning,"Refresh Request","Refresh request received, please refresh page one you are done editing.");
        return Task.CompletedTask;
    }

    protected void ShowNotification(NotificationSeverity severity,string title,string message,int duration=5000) {
        this._notificationService.Notify(new NotificationMessage {
            Severity=severity,
            Detail=message,
            Summary = title,
            Duration = duration
        });
    }
}