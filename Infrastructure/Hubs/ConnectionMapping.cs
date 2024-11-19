namespace Infrastructure.Hubs;

public class ConnectionMapping<T> where T : class{
    private readonly Dictionary<T,HashSet<string>> _connections = new Dictionary<T, HashSet<string>>();
    private Lock _connectionsLock = new Lock();
    
    public int Count => _connections.Count;

    public void Add(T key, string connectionId) {
        using (this._connectionsLock.EnterScope()) {
            if (!this._connections.TryGetValue(key, out HashSet<string> connections)) {
                connections=new HashSet<string>();
                this._connections.Add(key, connections);
            }
            
            lock (connections) {
                connections.Add(connectionId);
            }
        }
    }

    public IEnumerable<string> GetConnections(T key) {
        if(this._connections.TryGetValue(key, out HashSet<string>? connections)) {
            return connections;
        }
        return [];
    }
    
    public void PrintConnections() {
        foreach(var connection in this._connections) {
            foreach(var conn in connection.Value) {
                Console.WriteLine($"Key:{connection.Key} Connection: {conn}");
            }
        }
    }
    
    public T? GetKey(string connectionId) {
        using (this._connectionsLock.EnterScope()) {
            foreach (var connection in this._connections) {
                if (connection.Value.Contains(connectionId)) {
                    return connection.Key;
                }
            }
        }
        return null;
    }
    
    public void Remove(string connectionId) {
        using (this._connectionsLock.EnterScope()) {
            foreach (var connection in this._connections) {
                lock (connection.Value) {
                    connection.Value.Remove(connectionId);
                    if (connection.Value.Count == 0) {
                        this._connections.Remove(connection.Key);
                    }
                }
            }
        }
    }

    public void Remove(T key, string connectionId) {
        using (this._connectionsLock.EnterScope()) {
            HashSet<string> connections;
            if(!this._connections.TryGetValue(key, out connections)) {
                return;
            }

            lock (connections) {
                connections.Remove(connectionId);
                if (connections.Count == 0) {
                    this._connections.Remove(key);
                }
            }
        }
    }
}