using System.Security.Claims;
using Domain.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs;
public class MessagingHub:Hub<IMessagingHub> {
    //private static readonly ConnectionMapping<string> Connections=new ConnectionMapping<string>();
    /*public async Task SendMessage(string username, string message) {
        var to = Connections.GetConnections(username);
        var from=Connections.GetKey(Context.ConnectionId);
        foreach (var connectionId in to) {
            await Clients.Client(connectionId).ReceiveMessage(from ?? "Missing Id", message);
        }
    }*/
    
    public async Task SendRefreshAll() { 
        await Clients.Others.ReceiveRefresh();
    }
    
    public override Task OnDisconnectedAsync(Exception? exception) {
        /*var id=Context.ConnectionId;
        Connections.Remove(id);*/
        return base.OnDisconnectedAsync(exception);
    }
}