using System.Linq.Expressions;
using Domain.PurchaseRequests;
using Domain.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Services;

public class PurchaseRequestService {
    private readonly IMongoCollection<PurchaseRequest> _purchaseRequestCollection;
    private readonly UserProfileService _userProfileService;
    
    public PurchaseRequestService(IMongoClient client,UserProfileService userProfileService,
        IOptions<DatabaseSettings> options) {
        var database = client.GetDatabase(options.Value.PurchaseRequestDatabase ?? "purchase_req_db");
        this._purchaseRequestCollection=database.GetCollection<PurchaseRequest>(options.Value.PurchaseRequestCollection ?? "purchase_requests");
        this._userProfileService=userProfileService;
    }
    
    public async Task<List<PurchaseRequest>> GetPurchaseRequests() {
        return await this._purchaseRequestCollection.Find(pr => true).ToListAsync();
    }
    
    public async Task<bool> CreatePurchaseRequest(PurchaseRequestInput input) {
        var purchaseRequest=new PurchaseRequest {
            Title=input.Title,
            Description=input.Description,
            FilePath=input.FilePath,
            Urgent=input.Urgent,
            Approver= input.ApproverName,
            Username = input.RequesterName,
            
        };
        //await this._purchaseRequestCollection.InsertOneAsync(purchaseRequest);
        return true;
    }
    
    public async Task<PurchaseRequest> GetUserPurchaseRequest(Expression<Func<PurchaseRequest,bool>> filter) {
        return await this._purchaseRequestCollection.Find(filter).FirstOrDefaultAsync();
    }
    
    /*public async Task<PurchaseRequest> GetApproverNeedsApprovalPurchaseRequest(string approver) {
        return await this._purchaseRequestCollection.Find(pr => !pr.Approved && pr.Approver==approver).FirstOrDefaultAsync();
    }
    
    public async Task<PurchaseRequest> GetRequesterNeedsApprovalPurchaseRequest(string requester) {
        return await this._purchaseRequestCollection.Find(pr => !pr.Approved && pr.Username==requester).FirstOrDefaultAsync();
    }
    
    public async Task<PurchaseRequest> GetRequestersNeedsOrderPurchaseRequest(string username) {
        return await this._purchaseRequestCollection.Find(pr => pr.Username==username && pr.Approved && !pr.Ordered).FirstOrDefaultAsync();
    }
    
    public async Task<PurchaseRequest> GetRequestersNeedsOrderPurchaseRequest(string username) {
        return await this._purchaseRequestCollection.Find(pr => pr.Username==username && pr.Approved && !pr.Ordered).FirstOrDefaultAsync();
    }*/
}