namespace Webapp.Services;

public class SpinnerService
{
    public event Action<string> OnShow;
    public event Action<string> OnUpdateMessage;
    public event Action OnHide;
    
    public bool IsVisible { get; private set; }

    public void Show(string? loadingMessage = default) {
        this.IsVisible = true;
        this.OnShow?.Invoke(loadingMessage ?? "Loading...");
    }
    
    public void UpdateMessage(string message) {
        this.OnUpdateMessage?.Invoke(message);
    }
    
    public void Hide() {
        this.IsVisible = false;
        this.OnHide?.Invoke();
    }
}