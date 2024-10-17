﻿using System.Linq.Expressions;
using Domain.PurchaseRequests.Dto;
using Domain.PurchaseRequests.Model;
using Domain.PurchaseRequests.TypeConstants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using SETiAuth.Domain.Shared.Authentication;
using SetiFileStore.FileClient;

namespace Infrastructure.Services;

public class PurchaseRequestService {
    private readonly PurchaseRequestDataService _requestDataService;
    private readonly ContactDataService _contactDataService;
    private readonly DepartmentDataService _departmentDataService;
    private readonly UserProfileService _userProfileService;
    private readonly AuthApiService _authApiService;
    private readonly EmailService _emailService;
    private readonly FileService _fileService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<PurchaseRequestService> _logger;
    
    public PurchaseRequestService(PurchaseRequestDataService requestDataService,UserProfileService userProfileService,
        DepartmentDataService departmentDataService,ContactDataService contactDataService, EmailService emailService,
        AuthApiService authService,FileService fileService,IServiceScopeFactory serviceScopeFactory,ILogger<PurchaseRequestService> logger) {
        this._requestDataService = requestDataService;
        this._contactDataService = contactDataService;
        this._emailService = emailService;
        this._authApiService = authService;
        this._userProfileService=userProfileService;
        this._departmentDataService = departmentDataService;
        this._fileService = fileService;
        this._serviceScopeFactory = serviceScopeFactory;
        this._logger = logger;
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
            Created = TimeProvider.Now(),
            Quotes = input.Quotes,
            ShippingType = input.ShippingType,
            PurchaseItems = input.PurchaseItems,
            EmailCopyList = input.EmailCcList
        };
        input.PrUrl=$"http://localhost:5015/action/{purchaseRequest._id.ToString()}/{PrUserAction.APPROVE}";
        purchaseRequest.PrUrl = input.PrUrl;
        await this._requestDataService.InsertOne(purchaseRequest);
        var exists = await this._requestDataService.Exists(purchaseRequest._id);
        await Task.Delay(1000);
        if (!exists) return false;
        /*_ = Task.Run(async () => {
            await using var scope = this._serviceScopeFactory.CreateAsyncScope();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
            await emailService.SendRequestEmail(input.EmailTemplate ?? [], input,
                [input.RequesterEmail ?? ""],
                [input.RequesterEmail ?? ""]);
        });*/
        await this._emailService.SendRequestEmail(input.EmailTemplate ?? [], input,
            [input.RequesterEmail ?? ""],
            [input.RequesterEmail ?? ""]);
        return true;
    }

    public async Task<bool> UpdatePurchaseRequest(PurchaseRequestInput input) {
        if(!input.Id.HasValue) return false;
        var pr = new PurchaseRequest().FromInput(input);
        bool success=await this._requestDataService.UpdateOne(pr);
        if(!success) return false;
        await this._emailService.SendRequestEmail(input.EmailTemplate ?? [],input, 
            [input.RequesterEmail ?? ""],
            [input.RequesterEmail ?? ""]);
        return true;
    }

    public async Task<bool> CancelPurchaseRequest(CancelPurchaseRequestInput input) {
        var deleted = await this._requestDataService.DeletePurchaseRequest(input.Id);
        if (!deleted) return false;
        foreach (var quote in input.FileIds) {
            await this._fileService.DeleteFile(quote);
        }
        await this._emailService.SendCancellationEmail(input.EmailTemplate ?? [], input.Title ?? "Unknown",
            ["aelmendorf@s-et.com" ?? ""],
            ["aelmendorf@s-et.com" ?? ""]);
        return true;
    }
    
    public async Task<bool> ApproveRejectPurchaseRequest(ApproveRequestInput input,PurchaseRequest request) {
        bool approved = input.Action==PurchaseRequestAction.Approve ? true : false;
        request.ApprovalResult = new ApprovalResult() {
            Approved = approved, 
            Comments = input.Comment ?? "",
        };
        if(input.Action == PurchaseRequestAction.Approve) {
            request.ApprovedDate = TimeProvider.Now();
        } else {
            request.RejectedDate = TimeProvider.Now();
        }
        request.Status = approved ? PrStatus.Approved : PrStatus.Rejected;
        if (approved) {
            request.PrUrl = $"http://localhost:5015/action/{request._id.ToString()}/{PrUserAction.ORDER}";
        }
        

        var success=await this._requestDataService.UpdateOne(request);
        List<FileData> files = [];
        foreach (var quote in request.Quotes) {
            var fileData=await this._fileService.DownloadFile(quote);
            if (fileData != null) {
                files.Add(fileData);
            }
        }
        if(!success) return false;
        await this._emailService.SendApprovalEmail(input.EmailDocument ?? [],request.Title ?? "Not Titled",approved,
            request.PrUrl ?? "",files,
            [request.Requester.Email ?? ""],
            [request.Requester.Email ?? ""]);
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
    
    public async Task<List<PurchaseRequest>> GetPurchaseRequests(string username, string role) {
        return await this._requestDataService.GetPurchaseRequests(username,role);
    }
    
    public async Task<List<PurchaseRequest>> GetApproverRequests(string username) {
        return await this._requestDataService.GetApproverRequests(username);
    }
    
    public async Task<List<PurchaseRequest>> GetUserPurchaseRequests(Expression<Func<PurchaseRequest,bool>> filter) {
        return await this._requestDataService.GetUserPurchaseRequests(filter);
    }
}