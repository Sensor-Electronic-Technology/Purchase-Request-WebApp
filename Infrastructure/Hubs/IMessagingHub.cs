namespace Infrastructure.Hubs;

public interface IMessagingHub {
    public Task ReceiveMessage(string from, string message);
    public Task ReceiveRefresh();
    public Task Connected();
}