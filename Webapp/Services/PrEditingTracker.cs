using DevExpress.Blazor.RichEdit.SpellCheck;
using MongoDB.Bson;

namespace Webapp.Services;

public class PrEditingTracker {
    public event Action<string> OnAddToEditingList;
    public event Action<string> OnRemoveFromEditingList;
    public event Action<string,string> OnTimeout;
    
    private Dictionary<string,string> _editingList = new Dictionary<string, string>(); 
    
    private Lock _lock = new Lock();

    /*private List<string> _editingList = [];*/

    public Dictionary<string,string> EditingList {
        get=>this._editingList;
        set {
            if(value!=this._editingList) {
                this._editingList = value;
            }
        }
    }

    public string? IsAvailable(string id) {
        using(this._lock.EnterScope()) {
            if (this.EditingList.ContainsKey(id)) {
                return this.EditingList[id];
            } else {
                return default;
            }
        }
    }

    public void UserTryClear(string username) {
        using(this._lock.EnterScope()) {
             var id=this.EditingList.FirstOrDefault(e=>e.Value==username).Key;
             if (!string.IsNullOrEmpty(id)) {
                 this.EditingList.Remove(id);
                 this.OnRemoveFromEditingList?.Invoke(id);
             }
        }
    }
    
    public void StartEditing(string username,string id) {
        using (this._lock.EnterScope()) {
            this.EditingList[id] = username;
            /*this.OnAddToEditingList?.Invoke(new KeyValuePair<string,string>(username,id));*/
            this.OnAddToEditingList?.Invoke(id);
        }
    }
    
    public void FinishEditing(string id) {
        using (this._lock.EnterScope()) {
            if (this.EditingList.ContainsKey(id)) {
                this.EditingList.Remove(id);
            }
            this.OnRemoveFromEditingList?.Invoke(id);
        }
    }
    
    public void Timeout(string id) {
        using (this._lock.EnterScope()) {
            if (this.EditingList.ContainsKey(id)) {
                var username = this.EditingList[id];
                this.EditingList.Remove(id);
                this.OnTimeout?.Invoke(username,id);
            }
        }
    }
}