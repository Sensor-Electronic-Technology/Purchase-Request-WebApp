using Domain.PurchaseRequests.Model;
using Domain.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infrastructure.Services;

public class RemindDataService {
    private readonly IMongoCollection<RemindConfig> _remindCollection;
    private readonly AppTimeProvider _timeProvider;

    public RemindDataService(IMongoClient client,
        IOptions<DatabaseSettings> options, AppTimeProvider timeProvider) {
        var database = client.GetDatabase(options.Value.PurchaseRequestDatabase ?? "purchase_req_db");
        /*this._purchaseRequestCollection = database.GetCollection<PurchaseRequest>(options.Value.PurchaseRequestCollection ?? "purchase_requests");*/
        this._remindCollection = database.GetCollection<RemindConfig>("remind_settings");
        this._timeProvider = timeProvider;
    }

    public async Task<RemindConfig> GetRemindConfigAsync() {
        return await this._remindCollection.Find(_ => true).FirstOrDefaultAsync() ?? new RemindConfig();
    }

    public async Task<bool> UpdateRemindConfig(RemindConfig remindConfig, CancellationToken ct = default) {
        var filter = Builders<RemindConfig>.Filter.Eq(e => e._id, remindConfig._id);
        var update = Builders<RemindConfig>.Update
            .Set(e => e.ApprovalTimeHrs, remindConfig.ApprovalTimeHrs)
            .Set(e => e.ApprovalTimeRepeatHrs, remindConfig.ApprovalTimeRepeatHrs)
            .Set(e => e.OrderTimeHrs, remindConfig.OrderTimeHrs)
            .Set(e => e.OrderTimeRepeatHrs, remindConfig.OrderTimeRepeatHrs)
            .Set(e => e.CheckInTimeDays, remindConfig.CheckInTimeDays)
            .Set(e => e.CheckInTimeRepeatDays, remindConfig.CheckInTimeRepeatDays);
        var result = await this._remindCollection.UpdateOneAsync(filter, update, cancellationToken: ct);
        return result is { IsAcknowledged: true, ModifiedCount: > 0 };
    }
    
   
}