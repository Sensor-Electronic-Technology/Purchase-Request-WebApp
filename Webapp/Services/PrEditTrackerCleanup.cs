using System.Collections.Concurrent;
using Infrastructure.Services;
using TimeProvider = System.TimeProvider;

namespace Webapp.Services;

public class PrEditTrackerCleanup:IHostedService, IDisposable {
    private readonly PrEditingTracker _editingTracker;
    private Dictionary<string,ITimer> _timers=new();
    //private ConcurrentDictionary<string, ITimer> _timers = new();
    private readonly ILogger<PrEditTrackerCleanup> _logger;
    private readonly AppTimeProvider _appTimeProvider;
    private readonly int TimeoutSeconds=10*60;
    private Lock _lock = new Lock();
    
    public PrEditTrackerCleanup(PrEditingTracker editingTracker,ILogger<PrEditTrackerCleanup> logger,AppTimeProvider timeProvider) {
        this._editingTracker = editingTracker;
        this._editingTracker.OnAddToEditingList+=this.AddTimer;
        this._editingTracker.OnRemoveFromEditingList+=this.RemoveTimer;
        this._logger=logger;
        this._appTimeProvider=timeProvider;
    }
    public Task StartAsync(CancellationToken cancellationToken) {
        this._logger.LogInformation("Starting PrEditTrackerCleanup");
        return Task.CompletedTask;
    }
    private void AddTimer(string id) {
        try {
            using (this._lock.EnterScope()) {
                if (this._timers.ContainsKey(id)) {
                    this._timers[id].Dispose();
                    this._editingTracker.Timeout(id);
                    if (this._timers.Remove(id)) {
                        this._logger.LogInformation("Removed timer for {Id}, Tracking: {TCount}", id,this._timers.Count);
                    } else {
                        this._logger.LogError("Failed to remove timer for {Id}, Tracking: {TCount}", id,this._timers.Count);
                    }
                }
                this._timers[id]=this._appTimeProvider.CreateTimer(state => {
                    this._editingTracker.Timeout(id);
                    this.RemoveTimer(id);
                },id,TimeSpan.FromSeconds(this.TimeoutSeconds),TimeSpan.FromSeconds(0));
                this._logger.LogInformation("Added timer for {Id}, Tracking: {TCount}", id,this._timers.Count);
            }
        }catch(Exception e) {
            this._logger.LogError(e,"Error adding timer for {Id}",id);
        }
    }
    
    private void RemoveTimer(string id) {
        try {
            using (this._lock.EnterScope()) {
                if(this._timers.TryGetValue(id,out var timer)) {
                    timer.Dispose();
                    if (!this._timers.Remove(id)) {
                        this._logger.LogError("Failed to removed timer for {Id}, Tracking: {TCount}", id,this._timers.Count); 
                    } else {
                        this._logger.LogInformation("Removed timer for {Id}, Tracking: {TCount}", id,this._timers.Count);
                    }
                } else {
                    this._logger.LogError("Failed to removed timer for {Id}, Tracking: {TCount}", id,this._timers.Count);            
                }
            }
        }catch(Exception e) {
            this._logger.LogError(e,"Error removing timer for {Id}",id);
        }
    }
    
    public Task StopAsync(CancellationToken cancellationToken) {
        this._timers.Clear();
        this._logger.LogInformation("Shutting down PrEditTrackerCleanup");
        return Task.CompletedTask;
    }

    public void Dispose() {
        this._editingTracker.OnAddToEditingList-=this.AddTimer;
        this._editingTracker.OnRemoveFromEditingList-=this.RemoveTimer;
    }
}