using System.Linq.Expressions;
using Domain.Authentication;
using Domain.PurchaseRequests.Dto;
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
    
    public PurchaseRequestDataService(IMongoClient client, IOptions<DatabaseSettings> options) {
        var database = client.GetDatabase(options.Value.PurchaseRequestDatabase ?? "purchase_req_db");
        this._purchaseRequestCollection = database.GetCollection<PurchaseRequest>(options.Value.PurchaseRequestCollection ?? "purchase_requests");
    }

    public async Task<List<PurchaseRequest>> GetPurchaseRequests(string username, string role) {
        if(PurchaseRequestRole.TryFromName(role, out var prRole)) {
            switch (prRole.Name) {
                case nameof(PurchaseRequestRole.Requester): {
                    return await this._purchaseRequestCollection.Find(pr => pr.Requester.Username == username)
                        .ToListAsync();
                }
                case nameof(PurchaseRequestRole.Approver): {
                    return await this._purchaseRequestCollection.Find(pr =>pr.Approver.Username == username)
                        .ToListAsync();
                }
                case nameof(PurchaseRequestRole.Purchaser): {
                    return await this._purchaseRequestCollection.Find(pr => pr.Status == PrStatus.Approved)
                        .ToListAsync();
                }
                default:
                    return [];
            }
        }
        return [];
    }

    public async Task<bool> DeletePurchaseRequest(ObjectId id) {
        var result=await this._purchaseRequestCollection.DeleteOneAsync(e => e._id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
    
    public async Task<List<PurchaseRequest>> GetPurchaseRequests() {
        return await this._purchaseRequestCollection.Find(pr => true).ToListAsync();
    }
    
    public async Task<PurchaseRequest> GetPurchaseRequest(ObjectId id) {
        return await this._purchaseRequestCollection.Find(pr => pr._id == id).FirstOrDefaultAsync();
    }
    
    public async Task<List<PurchaseRequest>> GetUserPurchaseRequests(Expression<Func<PurchaseRequest,bool>> filter) {
        return await this._purchaseRequestCollection.Find(filter).ToListAsync();
    }

    public async Task<List<PurchaseRequest>> GetApproverRequests(string username) {
        return await this._purchaseRequestCollection.Find(e=>e.Approver.Username==username).ToListAsync();
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

    public IMongoQueryable<PurchaseRequest>? GetQueryObject() {
        return this._purchaseRequestCollection.AsQueryable();
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
    
    /*public async Task<bool> CreatePurchaseRequest(PurchaseRequestInput input) {
        var purchaseRequest=new PurchaseRequest {
            Title=input.Title,
            Description=input.Description,
            FilePath=input.FilePath,
            Urgent=input.Urgent,
            Approver= input.ApproverName,
            Username = input.RequesterUsername,
        };
        await this._purchaseRequestCollection.InsertOneAsync(purchaseRequest);
        var exists = await this._purchaseRequestCollection.Find(e => e._id == purchaseRequest._id).AnyAsync();
        if (exists) {
            
            input.PrUrl=$"http://localhost:5015/approve/{purchaseRequest._id.ToString()}";
            await this._emailService.SendEmail(EmailType.NeedsApproval, input, 
                [input.RequesterEmail ?? ""],
                [input.RequesterEmail ?? ""]);
            return true;
        }
        return false;
    }*/
}