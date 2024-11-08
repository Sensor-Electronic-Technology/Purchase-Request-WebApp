using System.Security.Claims;
using Domain.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Hubs;
public class MessagingHub:Hub<IMessagingHub> {
    /*private static readonly Dictionary<string, string> UserLookup = new Dictionary<string, string>();*/
    private static readonly ConnectionMapping<string> Connections=new ConnectionMapping<string>();
    public async Task SendMessage(string username, string message) {
        var to = Connections.GetConnections(username);
        var from=Connections.GetKey(Context.ConnectionId);
        foreach (var connectionId in to) {
            await Clients.Client(connectionId).ReceiveMessage(from ?? "Missing Id", message);
        }
    }
    
    public async Task SendRefresh(string username) {
        var to = Connections.GetConnections(username);
        foreach (var connectionId in to) {
            await Clients.Client(connectionId).ReceiveRefresh();
        }
    }
    
    public async Task SendRefreshAll() { 
        await Clients.All.ReceiveRefresh();
    }

    public async Task Register(string username) {
        var currentId = Context.ConnectionId;
        if (!Connections.GetConnections(username).Any()) {
            Connections.Add(username,currentId);
            await Clients.Client(currentId).Connected();
        }
    }

    public override Task OnDisconnectedAsync(Exception? exception) {
        var id=Context.ConnectionId;
        Connections.Remove(id);
        return base.OnDisconnectedAsync(exception);
    }
}