using System.Linq.Expressions;
using Domain.PurchaseRequests;
using Domain.PurchaseRequests.Dto;
using Domain.PurchaseRequests.Model;
using Domain.PurchaseRequests.TypeConstants;
using Domain.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using QuestPDF;
using SETiAuth.Domain.Shared.Authentication;

namespace Infrastructure.Services;

public class PurchaseRequestService {
    private readonly PurchaseRequestDataService _requestDataService;
    private readonly ContactDataService _contactDataService;
    private readonly DepartmentDataService _departmentDataService;
    private readonly UserProfileService _userProfileService;
    private readonly AuthApiService _authApiService;
    private readonly EmailService _emailService;
    //private readonly 
    
    public PurchaseRequestService(PurchaseRequestDataService requestDataService,UserProfileService userProfileService,
        DepartmentDataService departmentDataService,ContactDataService contactDataService, EmailService emailService,
        AuthApiService authService) {
        this._requestDataService = requestDataService;
        this._contactDataService = contactDataService;
        this._emailService = emailService;
        this._authApiService = authService;
        this._userProfileService=userProfileService;
        this._departmentDataService = departmentDataService;
    }
    

    public async Task<PurchaseRequest> GetPurchaseRequest(ObjectId id) {
        return await this._requestDataService.GetPurchaseRequest(id);
    }

    public PurchaseRequestInput CreatePrInput(string name,string email,string username) {
        var input= new PurchaseRequestInput() {
            RequesterName = name,
            RequesterEmail = email,
            RequesterUsername = username,
            Id=ObjectId.GenerateNewId(),
            PurchaseItems = new List<PurchaseItem>(),
            Quotes = new List<string>(),
        };
        input.PrUrl = $"http://localhost:5015/approve/{input.Id.ToString()}";
        return input;
    }
    
    public async Task<bool> CreatePurchaseRequest(PurchaseRequestInput input) {
        var purchaseRequest=new PurchaseRequest {
            _id = input.Id ?? ObjectId.GenerateNewId(),
            Title=input.Title,
            Description=input.Description,
            Urgent=input.Urgent,
            Approver = new PrApprover {
                Name = input.ApproverName,
                Email = input.ApproverEmail,
                Username = input.ApproverId,
            },
            Requester =new PrRequester() {
                Email = input.RequesterEmail,
                Name = input.RequesterName,
                Username = input.RequesterUsername,
            },
            Department = input.Department,
            Vendor = input.Vendor,
            AdditionalComments = input.AdditionalComments,
            Created = DateTime.Now,
            Quotes = input.Quotes,
            ShippingType = input.ShippingType,
            PurchaseItems = input.PurchaseItems,
        };
        input.PrUrl=$"http://localhost:5015/approve/{purchaseRequest._id.ToString()}";
        purchaseRequest.PrUrl = input.PrUrl;
        await this._requestDataService.InsertOne(purchaseRequest);
        var exists = await this._requestDataService.Exists(purchaseRequest._id);
        if (!exists) return false;
        await this._emailService.SendRequestEmail(input.EmailTemplate ?? [],input, 
            [input.RequesterEmail ?? ""],
            [input.RequesterEmail ?? ""]);
        return true;
    }

    public async Task<bool> UpdatePurchaseRequest(PurchaseRequestInput input) {
        bool success=await this._requestDataService.UpdateOne(new PurchaseRequest().FromInput(input));
        if(success) {
            await this._emailService.SendRequestEmail(input.EmailTemplate ?? [],input, 
                [input.RequesterEmail ?? ""],
                [input.RequesterEmail ?? ""]);
            return true;
        }
        return false;
    }
    
    public async Task<List<Vendor>> GetVendors() {
        return await this._contactDataService.GetVendors();
    }
    
    public async Task<List<Department>> GetDepartments() {
        return await this._departmentDataService.GetDepartments();
    }

    public async Task<List<UserAccountDto>> GetApprovers() {
        return await this._authApiService.GetApprovers();
    }

    public async Task<List<string>> GetUserEmails() {
        return await this._authApiService.GetUserEmails();
    }
    
    public async Task<List<PurchaseRequest>> GetPurchaseRequests() {
        return await this._requestDataService.GetPurchaseRequests();
    }
    
    public async Task<List<PurchaseRequest>> GetApproverRequests(string username) {
        return await this._requestDataService.GetApproverRequests(username);
    }
    
    public async Task<List<PurchaseRequest>> GetUserPurchaseRequests(Expression<Func<PurchaseRequest,bool>> filter) {
        return await this._requestDataService.GetUserPurchaseRequests(filter);
    }
}