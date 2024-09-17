using System.Linq.Expressions;
using Domain.PurchaseRequests;
using Domain.PurchaseRequests.Dto;
using Domain.PurchaseRequests.Model;
using Domain.PurchaseRequests.TypeConstants;
using Domain.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Services;

public class PurchaseRequestService {
    private readonly IMongoCollection<PurchaseRequest> _purchaseRequestCollection;
    private readonly UserProfileService _userProfileService;
    private readonly AuthApiService _authApiService;
    private readonly EmailService _emailService;
    
    public PurchaseRequestService(IMongoClient client,UserProfileService userProfileService,
        EmailService emailService,AuthApiService authService,IOptions<DatabaseSettings> options) {
        var database = client.GetDatabase(options.Value.PurchaseRequestDatabase ?? "purchase_req_db");
        this._purchaseRequestCollection=database.GetCollection<PurchaseRequest>(options.Value.PurchaseRequestCollection ?? "purchase_requests");
        this._emailService = emailService;
        this._authApiService = authService;
        this._userProfileService=userProfileService;
    }
    
    public async Task<List<PurchaseRequest>> GetPurchaseRequests() {
        return await this._purchaseRequestCollection.Find(pr => true).ToListAsync();
    }
    
    public async Task<PurchaseRequest> GetPurchaseRequest(ObjectId id) {
        return await this._purchaseRequestCollection.Find(pr => pr._id == id).FirstOrDefaultAsync();
    }
    
    
    public async Task<bool> CreatePurchaseRequest(PurchaseRequestInput input) {
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
    }

    public async Task PerformAction(PurchaseRequestAction action, ObjectId id) {
        var pr = await this._purchaseRequestCollection.Find(e => e._id == id).FirstOrDefaultAsync();
        if (pr != null) {
            switch (action.Name) {
                case nameof(PurchaseRequestAction.Approve):
                    this.HandleApprove(pr);
                    break;
                case nameof(PurchaseRequestAction.Reject):
                    this.HandleReject(pr);
                    break;
                case nameof(PurchaseRequestAction.Cancel):
                    this.HandleCancel(pr);
                    break;
                case nameof(PurchaseRequestAction.Order):
                    this.HandleOrder(pr);
                    break;
                case nameof(PurchaseRequestAction.Receive):
                    this.HandleReceive(pr);
                    break;
            }
            await this._purchaseRequestCollection.ReplaceOneAsync(e => e._id == id, pr);
        }
    }
    
    private void HandleApprove(PurchaseRequest purchaseRequest) {
        purchaseRequest.Approved = true;
        purchaseRequest.ApprovedDate = DateTime.Now;
    }
    private void HandleCancel(PurchaseRequest purchaseRequest) {
        //Send Email
    }
    private void HandleReject(PurchaseRequest purchaseRequest) {
        purchaseRequest.Approved = false;
        purchaseRequest.Rejected = true;
        purchaseRequest.RejectedDate = DateTime.Now;
        //Send Rejected Email
    }
    private void HandleOrder(PurchaseRequest purchaseRequest) {
        purchaseRequest.Ordered = true;
        purchaseRequest.OrderedDate = DateTime.Now;
        //Send Ordered Email
    }
    
    private void HandleReceive(PurchaseRequest purchaseRequest) {
        purchaseRequest.Received = true;
        purchaseRequest.ReceivedDate = DateTime.Now;
        //Send Received Email
    }
    
    public async Task<List<PurchaseRequest>> GetUserPurchaseRequests(Expression<Func<PurchaseRequest,bool>> filter) {
        return await this._purchaseRequestCollection.Find(filter).ToListAsync();
    }
}