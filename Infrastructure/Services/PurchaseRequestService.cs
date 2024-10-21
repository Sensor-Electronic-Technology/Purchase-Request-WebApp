using System.Linq.Expressions;
using Domain.PurchaseRequests.Dto;
using Domain.PurchaseRequests.Model;
using Domain.PurchaseRequests.Pdf;
using Domain.PurchaseRequests.TypeConstants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using QuestPDF.Fluent;
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
    private readonly ILogger<PurchaseRequestService> _logger;
    private readonly IWebHostEnvironment _environment;
    
    public PurchaseRequestService(PurchaseRequestDataService requestDataService,UserProfileService userProfileService,
        DepartmentDataService departmentDataService,ContactDataService contactDataService, EmailService emailService,
        AuthApiService authService,FileService fileService,
        IWebHostEnvironment environment,
        ILogger<PurchaseRequestService> logger) {
        this._requestDataService = requestDataService;
        this._contactDataService = contactDataService;
        this._emailService = emailService;
        this._authApiService = authService;
        this._userProfileService=userProfileService;
        this._departmentDataService = departmentDataService;
        this._fileService = fileService;
        this._environment = environment;
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
        input.PrUrl=$"http://172.20.4.207/action/{input.Id.ToString()}/{(int)PrUserAction.APPROVE}";
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
        input.PrUrl=$"http://172.20.4.207/action/{purchaseRequest._id.ToString()}/{(int)PrUserAction.APPROVE}";
        purchaseRequest.PrUrl = input.PrUrl;
        await this._requestDataService.InsertOne(purchaseRequest);
        var exists = await this._requestDataService.Exists(purchaseRequest._id);
        if (!exists) return false;
        await this._emailService.SendRequestEmail(input.EmailTemplate ?? [], input,
            [input.RequesterEmail ?? ""],
            [input.RequesterEmail ?? ""]);
        return true;
    }

    public async Task<bool> UpdatePurchaseRequest(PurchaseRequestInput input) {
        Console.WriteLine($"Updating Purchase Request {input.Id}");
        if(!input.Id.HasValue) return false;
        if (input.Status == PrStatus.NeedsApproval) {
            input.PrUrl=$"http://172.20.4.207/action/{input.Id.ToString()}/{(int)PrUserAction.APPROVE}";
        } else {
            input.PrUrl=$"http://172.20.4.207/action/{input.Id.ToString()}/{(int)PrUserAction.ORDER}";
        }
        
        var pr = new PurchaseRequest().FromInput(input);
        bool success=await this._requestDataService.UpdateOne(pr);
        Console.WriteLine("Updated Purchase Request: "+success);
        if(!success) return false;
        
        foreach(var quote in input.Quotes) {
            var fileData=await this._fileService.DownloadFile(quote);
            if (fileData != null) {
                input.Attachments.Add(new FileInput(fileData.Name,fileData.Data));
            }
        }
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
        var document = new PurchaseRequestDocument(request.ToInput(),Path.Combine($"{this._environment.WebRootPath}","images/seti_logo.png"));
        await this._emailService.SendApprovalEmail(input.EmailDocument ?? [],request.Title ?? "Not Titled",approved,
            request.PrUrl ?? "",
            document.GeneratePdf(),
            files,
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