namespace Webapp.Services;

public class SessionStorageService {
    private IDictionary<string,string> _sessionStorage = new Dictionary<string, string>(); 
    private Lock _lock = new Lock();
    
    public string? GetItem(string username) {
        using (this._lock.EnterScope()) {
            if(this._sessionStorage.ContainsKey(username)) {
                return this._sessionStorage[username];
            }
            return default;
        }
    }
    
    public void SetItem(string username, string token) {
        using (this._lock.EnterScope()) {
            this._sessionStorage[username] = token;
        }
    }
}