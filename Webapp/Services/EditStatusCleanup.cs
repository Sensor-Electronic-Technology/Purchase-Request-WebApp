namespace Webapp.Services;

public class EditStatusCleanup:BackgroundService {
    private readonly PrEditStatus _editStatus;
    private Dictionary<string,DateTime> _timers=new();
    private readonly ILogger<EditStatusCleanup> _logger;
    
    public EditStatusCleanup(PrEditStatus editStatus,ILogger<EditStatusCleanup> logger) {
        this._editStatus = editStatus;
        this._editStatus.OnAddToEditingList+=this.AddTimer;
        this._editStatus.OnRemoveFromEditingList+=this.RemoveTimer;
        this._logger=logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));
        try {
            while (await timer.WaitForNextTickAsync(stoppingToken)) {
                foreach(var id in this._timers.Keys) {
                    var seconds=DateTime.Now.Subtract(this._timers[id]).TotalSeconds;
                    if (seconds>10) {
                        Console.WriteLine($"Seconds: {seconds}");
                        this._editStatus.Timeout(id);
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
        this._editStatus.OnAddToEditingList-=this.AddTimer;
        this._editStatus.OnRemoveFromEditingList-=this.RemoveTimer;
        base.Dispose();
    }
}