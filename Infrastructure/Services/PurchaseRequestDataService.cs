﻿using System.Linq.Expressions;
using Domain.Authentication;
using Domain.PurchaseRequests.Dto;
using Domain.PurchaseRequests.Dto.ActionInputs;
using Domain.PurchaseRequests.Model;
using Domain.PurchaseRequests.TypeConstants;
using Domain.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Radzen;

namespace Infrastructure.Services;

public class PurchaseRequestDataService {
    private readonly IMongoCollection<PurchaseRequest> _purchaseRequestCollection;
    private readonly AppTimeProvider _timeProvider;
    
    public PurchaseRequestDataService(IMongoClient client, 
        IOptions<DatabaseSettings> options,AppTimeProvider timeProvider) {
        var database = client.GetDatabase(options.Value.PurchaseRequestDatabase ?? "purchase_req_db");
        this._purchaseRequestCollection = database.GetCollection<PurchaseRequest>(options.Value.PurchaseRequestCollection ?? "purchase_requests");
        this._timeProvider = timeProvider;
    }
    public async Task<PurchaseRequest> GetPurchaseRequest(ObjectId id) {
        return await this._purchaseRequestCollection.Find(pr => pr._id == id).FirstOrDefaultAsync();
    }
    public async Task<List<PurchaseRequest>> GetPurchaseRequests(string username, string role,string? email) {
        if(PurchaseRequestRole.TryFromName(role, out var prRole)) {
            switch (prRole.Name) {
                case nameof(PurchaseRequestRole.Requester): {
                    return await this.GetRequesterRequests(username,email).ToListAsync();
                }
                case nameof(PurchaseRequestRole.Approver): {
                    return await this.GetApproverRequests(username,email).ToListAsync();
                }
                case nameof(PurchaseRequestRole.Purchaser): {
                    return await this.GetPurchaserRequests(username,email).ToListAsync();
                }
                default:
                    return [];
            }
        }
        return [];
    }

    private async IAsyncEnumerable<PurchaseRequest> GetRequesterRequests(string username,string? email) {
        var findOptions = new FindOptions<PurchaseRequest>() { BatchSize = 10 };
        
        var filter=Builders<PurchaseRequest>.Filter.Or(Builders<PurchaseRequest>.Filter.Eq(e=>e.Requester.Username,username),Builders<PurchaseRequest>.Filter.AnyEq(e=>e.EmailCopyList,email));
        using IAsyncCursor<PurchaseRequest>? cursor=await this._purchaseRequestCollection.FindAsync(filter,findOptions);
        /*if (string.IsNullOrEmpty(email)) {
            cursor = await this._purchaseRequestCollection.FindAsync(e => e.Requester.Username==username,findOptions);
        } else {
            cursor = await this._purchaseRequestCollection.FindAsync(e => e.Requester.Username==username || e.EmailCopyList.Contains(email),findOptions);
        }*/
        
        while (await cursor.MoveNextAsync()) {
            var batch = cursor.Current;
            foreach (var item in batch) {
                if (item.Status == PrStatus.Delivered) {
                    if (this._timeProvider.DaysSince(item.ReceivedDate) <= 7) {
                        yield return item;
                    }
                }else if(item.Status==PrStatus.Rejected) {
                    if(this._timeProvider.DaysSince(item.RejectedDate) <= 7) {
                        yield return item;
                    }
                } else {
                    yield return item;
                }
            }
        }
    }
    
    private async IAsyncEnumerable<PurchaseRequest> GetPurchaserRequests(string username,string? email) {
        var findOptions = new FindOptions<PurchaseRequest>() { BatchSize = 10 };
        /*using var cursor = await this._purchaseRequestCollection.FindAsync(pr => pr.Status>=PrStatus.Approved && pr.Status!=PrStatus.Rejected,findOptions);*/
        var byPurchaser=Builders<PurchaseRequest>.Filter.And(Builders<PurchaseRequest>.Filter.Gte(e=>e.Status,PrStatus.Approved),Builders<PurchaseRequest>.Filter.Ne(e=>e.Status,PrStatus.Rejected));
        var byRequester=Builders<PurchaseRequest>.Filter.Or(Builders<PurchaseRequest>.Filter.Eq(e=>e.Requester.Username,username),Builders<PurchaseRequest>.Filter.AnyEq(e=>e.EmailCopyList,email));
        var filter=Builders<PurchaseRequest>.Filter.Or(byPurchaser,byRequester);
        using IAsyncCursor<PurchaseRequest>? cursor=await this._purchaseRequestCollection.FindAsync(filter,findOptions);
        while (await cursor.MoveNextAsync()) {
            var batch = cursor.Current;
            foreach (var item in batch) {
                if (item.Status == PrStatus.Approved) {
                    yield return item;
                }else if (item.Status>=PrStatus.Ordered) {
                    //if (item.Purchaser?.Username != username) continue;
                    if (item.Status == PrStatus.Delivered) {
                        if(this._timeProvider.DaysSince(item.ReceivedDate) <= 7) {
                            yield return item;
                        }
                    } else {
                        yield return item;
                    }
                }
            }
        }
    }
    
    private async IAsyncEnumerable<PurchaseRequest> GetApproverRequests(string username,string email) {
        var findOptions = new FindOptions<PurchaseRequest>() { BatchSize = 10 };
       // using var cursor = await this._purchaseRequestCollection.FindAsync(pr =>pr.Approver.Username == username,findOptions);
        var filter=Builders<PurchaseRequest>.Filter.Or(
            Builders<PurchaseRequest>.Filter.Eq(e=>e.Approver.Username,username),
            Builders<PurchaseRequest>.Filter.Eq(e=>e.Requester.Username,username),
            Builders<PurchaseRequest>.Filter.AnyEq(e=>e.EmailCopyList,email));
        using IAsyncCursor<PurchaseRequest>? cursor=await this._purchaseRequestCollection.FindAsync(filter,findOptions);
        while (await cursor.MoveNextAsync()) {
            var batch = cursor.Current;
            foreach (var item in batch) {
                if (item.Status == PrStatus.Delivered) {
                    if (this._timeProvider.DaysSince(item.ReceivedDate) <= 7) {
                        yield return item;
                    }
                }else  {
                    yield return item;
                }
            }
        }
    }
    
    public async Task<List<PurchaseRequest>> GetRequesterRequestStd(string username) {
        var findOptions = new FindOptions<PurchaseRequest>() { BatchSize = 10 };
        using var cursor = await this._purchaseRequestCollection.FindAsync(e => e.Requester.Username==username,findOptions);
        List<PurchaseRequest> requests=new();
        while (await cursor.MoveNextAsync()) {
            var batch = cursor.Current;
            foreach (var item in batch) {
                if (item.Status == PrStatus.Delivered) {
                    if (this._timeProvider.DaysSince(item.ReceivedDate) <= 7) {
                        requests.Add(item);
                    }
                }else if(item.Status==PrStatus.Rejected) {
                    if(this._timeProvider.DaysSince(item.RejectedDate) <= 7) {
                        requests.Add(item);
                    }
                } else {
                    requests.Add(item);
                }
            }
        }
        return requests;
    }

    public async Task<PurchaseRequest?> UpdateFromOrder(PurchaseOrderDto order) {
        var filter=Builders<PurchaseRequest>.Filter.Eq(e => e._id, order.RequestId);
        var update = Builders<PurchaseRequest>.Update
            .Set(e => e.Status, PrStatus.Ordered)
            .Set(e => e.PurchaseOrder,
                new PurchaseOrder() {
                    PoNumber = order.PoNumber,
                    PoComments = order.PoComments,
                    EmailComments = order.EmailComments,
                    PaymentTerms = order.PaymentTerms,
                    ShipTo = order.ShipTo,
                    PurchaseType=order.PurchaseType,
                    ItemType=order.ItemType,
                })
            .Set(e=>e.Purchaser,order.Purchaser)
            .Set(e => e.OrderedDate, this._timeProvider.Now())
            .Set(e => e.PurchaseItems, order.Items)
            .Set(e => e.Vendor, order.Vendor)
            .Set(e=>e.EmailCopyList,order.EmailCopyList)
            .Set(e => e.ShippingType, order.ShippingMethod);
        var result=await this._purchaseRequestCollection.UpdateOneAsync(filter,update);
        if (!result.IsAcknowledged) {
            return null;
        }
        return await this.GetPurchaseRequest(order.RequestId);
    }
    
    public async Task<PurchaseRequest?> UpdateFromReceive(ReceiveRequestInput receivedInput) {
        var filter=Builders<PurchaseRequest>.Filter.Eq(e => e._id, receivedInput.RequestId);
        var update = Builders<PurchaseRequest>.Update
            .Set(e => e.Status, PrStatus.Delivered)
            .Set(e => e.Receiver, receivedInput.Receiver)
            .Set(e => e.ReceivedDate, this._timeProvider.Now())
            .Set(e => e.CheckInResult, new CheckInResult() {
                Complete = receivedInput.Complete,
                ItemDelivery = receivedInput.ItemDelivery
            });
        var result=await this._purchaseRequestCollection.UpdateOneAsync(filter,update);
        if (!result.IsAcknowledged) {
            return null;
        }
        var request=await this._purchaseRequestCollection
            .Find(e => e._id == receivedInput.RequestId)
            .FirstOrDefaultAsync();
        if(request is { CheckInResult: not null }) {
            return request;
        }
        return null;
    }

    public async Task<bool> DeletePurchaseRequest(ObjectId id) {
        var result=await this._purchaseRequestCollection.DeleteOneAsync(e => e._id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
    
    public async Task<IQueryable<PurchaseRequest>?> GetPurchaseRequestsQuery() {
        return this._purchaseRequestCollection.AsQueryable();
    }
    
    public async Task InsertOne(PurchaseRequest purchaseRequest) {
        await this._purchaseRequestCollection.InsertOneAsync(purchaseRequest);
    }
    
    public async Task<bool> UpdateOne(PurchaseRequest purchaseRequest) {
        var result = await this._purchaseRequestCollection.ReplaceOneAsync(pr => pr._id == purchaseRequest._id, purchaseRequest);
        return result.IsAcknowledged;
    }
    
    public async Task<bool> Exists(ObjectId id) {
        return await this._purchaseRequestCollection.Find(pr => pr._id == id).AnyAsync();
    }
    
    public async Task<List<QuotesDto>> GetQuotes() {
        return await this._purchaseRequestCollection.AsQueryable()
            .Where(e=>e.Quotes.Any())
            .SelectMany(pr => pr.Quotes, (pr, q) => new QuotesDto() {
                PrTitle  = pr.Title,
                PrDescription = pr.Description,
                FileId = q,
                Username = pr.Requester.Username,
            }).ToListAsync();
    }
}