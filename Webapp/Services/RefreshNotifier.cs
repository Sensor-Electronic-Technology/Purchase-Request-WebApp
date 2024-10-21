namespace Webapp.Services;

public class RefreshNotifier {
    public event Action? OnNotifyRefresh;
    
    public void NotifyRefresh() {
        this.OnNotifyRefresh?.Invoke();
    }
    
    public void Subscribe(Action action) {
        this.OnNotifyRefresh+=action;
    }
}