using MongoDB.Bson;

namespace Webapp.Services;

public class PrEditStatus {
    public event Action<string> OnAddToEditingList;
    public event Action<string> OnRemoveFromEditingList;
    public event Action<string> OnTimeout;
    
    private object _lock = new object();

    private List<string> _editingList = [];

    public List<string> EditingList {
        get=>this._editingList;
        set {
            if(value!=this._editingList) {
                this._editingList = value;
            }
        }
    }

    public bool IsAvailable(string id) {
        lock (this._lock) {
            return !this.EditingList.Contains(id);
        }
    }
    
    public void StartEditing(string id) {
        lock (this._lock) {
            if (!this.EditingList.Contains(id)) {
                this.EditingList.Add(id);
            }
            this.OnAddToEditingList?.Invoke(id);
        }
    }
    
    public void FinishEditing(string id) {
        lock (this._lock) {
            if (this.EditingList.Contains(id)) {
                this.EditingList.Remove(id);
            }
            this.OnRemoveFromEditingList?.Invoke(id);
        }
    }
    
    public void Timeout(string id) {
        lock (this._lock) {
            if (this.EditingList.Contains(id)) {
                this.EditingList.Remove(id);
            }
            this.OnTimeout?.Invoke(id);
        }
    }
}