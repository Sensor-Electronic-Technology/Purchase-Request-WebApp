using Domain.PurchaseRequests.Model;
using MongoDB.Driver;
using System;
using Domain.PurchaseRequests.TypeConstants;
using Domain.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class PrReminderService {
    private readonly IMongoCollection<PurchaseRequest> _prCollection;
    private readonly AppTimeProvider _timeProvider;

    public PrReminderService(IMongoClient client,
        AppTimeProvider timeProvider,
        IOptions<DatabaseSettings> options) {
        var database = client.GetDatabase(options.Value.PurchaseRequestDatabase ?? "purchase_req_db");
        this._prCollection = database.GetCollection<PurchaseRequest>(options.Value.PurchaseRequestCollection ?? "purchase_requests");
        this._timeProvider=timeProvider;
    }

    public async IAsyncEnumerable<PurchaseRequest> GetRequestWaitingApproval() {
        var filter=Builders<PurchaseRequest>.Filter.Eq(e => e.Status,PrStatus.NeedsApproval);
        var findOptions = new FindOptions<PurchaseRequest>() { BatchSize = 10 };
        using IAsyncCursor<PurchaseRequest>? cursor=await this._prCollection.FindAsync(filter,findOptions);
        while (await cursor.MoveNextAsync()) {
            foreach (var prItem in cursor.Current) {
                if (_timeProvider.HoursSince(prItem.Created) >= 24) {
                    yield return prItem;
                }
            }
        }
    }
    
    public async IAsyncEnumerable<PurchaseRequest> GetRequestWaitingOrder() {
        var filter=Builders<PurchaseRequest>.Filter.Eq(e => e.Status,PrStatus.Approved);
        var findOptions = new FindOptions<PurchaseRequest>() { BatchSize = 10 };
        using IAsyncCursor<PurchaseRequest>? cursor=await this._prCollection.FindAsync(filter,findOptions);
        while (await cursor.MoveNextAsync()) {
            foreach (var prItem in cursor.Current) {
                if (_timeProvider.HoursSince(prItem.ApprovedDate) >= 24) {
                    yield return prItem;
                }
            }
        }
    }
    
    public async IAsyncEnumerable<PurchaseRequest> GetRequestWaitingDelivery() {
        var filter=Builders<PurchaseRequest>.Filter.Eq(e => e.Status,PrStatus.Ordered);
        var findOptions = new FindOptions<PurchaseRequest>() { BatchSize = 10 };
        using IAsyncCursor<PurchaseRequest>? cursor=await this._prCollection.FindAsync(filter,findOptions);
        while (await cursor.MoveNextAsync()) {
            foreach (var prItem in cursor.Current) {
                if (_timeProvider.DaysSince(prItem.OrderedDate) >=14) {
                    yield return prItem;
                }
            }
        }
    }

    /*private bool CheckHours(PurchaseRequest prItem,int limit) {
        if (prItem.ReminderDate > DateTime.MinValue) {
            if()   
        }
    }*/
}