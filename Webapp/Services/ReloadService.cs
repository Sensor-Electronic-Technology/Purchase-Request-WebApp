namespace Webapp.Services;

public class ReloadService {
    public event Action OnReloadRequested;
    
    public void Reload() {
        OnReloadRequested?.Invoke();
    }
}