namespace Webapp.Services;

public class PrEditTrackerCleanup:BackgroundService {
    private readonly PrEditingTracker _editingTracker;
    private Dictionary<string,DateTime> _timers=new();
    private readonly ILogger<PrEditTrackerCleanup> _logger;
    
    public PrEditTrackerCleanup(PrEditingTracker editingTracker,ILogger<PrEditTrackerCleanup> logger) {
        this._editingTracker = editingTracker;
        this._editingTracker.OnAddToEditingList+=this.AddTimer;
        this._editingTracker.OnRemoveFromEditingList+=this.RemoveTimer;
        this._logger=logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(10));
        try {
            while (await timer.WaitForNextTickAsync(stoppingToken)) {
                foreach(var id in this._timers.Keys) {
                    var seconds=DateTime.Now.Subtract(this._timers[id]).TotalSeconds;
                    if (seconds>10) {
                        Console.WriteLine($"Seconds: {seconds}");
                        this._editingTracker.Timeout(id);
                        this.RemoveTimer(id);
                    }
                }
            }
        } catch (OperationCanceledException) {
            this._logger.LogInformation("Timed Hosted Service is stopping");
        }
    }
    
    private void AddTimer(string id) {
        this._timers[id]=DateTime.Now;
        this._logger.LogInformation("Added timer for {Id}", id);
    }
    
    private void RemoveTimer(string id) {
        this._timers.Remove(id);
        this._logger.LogInformation("Removed timer for {Id}", id);
    }
    
    public override void Dispose() {
        this._editingTracker.OnAddToEditingList-=this.AddTimer;
        this._editingTracker.OnRemoveFromEditingList-=this.RemoveTimer;
        base.Dispose();
    }
}