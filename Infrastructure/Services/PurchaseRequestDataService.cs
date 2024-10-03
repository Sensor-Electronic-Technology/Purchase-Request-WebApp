using System.Linq.Expressions;
using Domain.PurchaseRequests.Dto;
using Domain.PurchaseRequests.Model;
using Domain.PurchaseRequests.TypeConstants;
using Domain.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Services;

public class PurchaseRequestDataService {
    private readonly IMongoCollection<PurchaseRequest> _purchaseRequestCollection;
    
    public PurchaseRequestDataService(IMongoClient client, IOptions<DatabaseSettings> options) {
        var database = client.GetDatabase(options.Value.PurchaseRequestDatabase ?? "purchase_req_db");
        this._purchaseRequestCollection = database.GetCollection<PurchaseRequest>(options.Value.PurchaseRequestCollection ?? "purchase_requests");
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
        return await this._purchaseRequestCollection.Find(e=>e.Approver==username).ToListAsync();
    }
    
    public async Task InsertOne(PurchaseRequest purchaseRequest) {
        await this._purchaseRequestCollection.InsertOneAsync(purchaseRequest);
    }
    
    public async Task<bool> Exists(ObjectId id) {
        return await this._purchaseRequestCollection.Find(pr => pr._id == id).AnyAsync();
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