using System.Linq.Expressions;
using Domain.PurchaseRequests.Dto;
using Domain.PurchaseRequests.Dto.ActionInputs;
using Domain.PurchaseRequests.Model;
using Domain.PurchaseRequests.Pdf;
using Domain.PurchaseRequests.TypeConstants;
using Domain.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using QuestPDF.Fluent;
using SETiAuth.Domain.Shared.Authentication;
using SetiFileStore.Domain.Contracts;
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
    private readonly IConfiguration _configuration;
    private readonly AppTimeProvider _timeProvider;
    
    public PurchaseRequestService(PurchaseRequestDataService requestDataService,UserProfileService userProfileService,
        DepartmentDataService departmentDataService,ContactDataService contactDataService, EmailService emailService,
        AuthApiService authService,FileService fileService,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<PurchaseRequestService> logger,AppTimeProvider timeProvider) {
        this._requestDataService = requestDataService;
        this._contactDataService = contactDataService;
        this._emailService = emailService;
        this._authApiService = authService;
        this._userProfileService=userProfileService;
        this._departmentDataService = departmentDataService;
        this._fileService = fileService;
        this._environment = environment;
        this._configuration=configuration;  
        this._logger = logger;
        this._timeProvider = timeProvider;
    }
    public async Task<PurchaseRequest> GetPurchaseRequest(ObjectId id) {
        return await this._requestDataService.GetPurchaseRequest(id);
    }
    public async Task<PurchaseRequestInput> CreatePrInput(UserProfile requester) {
        var input= new PurchaseRequestInput() {
            Id=ObjectId.GenerateNewId(),
            RequesterName = requester.FirstName + " " + requester.LastName,
            RequesterInitials = $"{requester.FirstName?.Substring(0, 1) ?? ""}{requester.LastName?.Substring(0, 1) ?? ""}",
            RequesterEmail = requester.Email,
            RequesterUsername = requester._id,
            PurchaseItems = new List<PurchaseItem>(),
            Quotes = new List<string>(),
            Attachments = new List<FileInput>(),
            EmailCcList = new List<string>(),
            Department =await this._departmentDataService.FindDepartmentById(requester.Defaults?.Department ?? ""),
            ShippingType = ShippingTypes.Ground.Name,
            Created = this._timeProvider.Now()
        };
        if(!string.IsNullOrEmpty(requester.Defaults?.ApproverUsername)) {
            var approver=await this._authApiService.GetApprover(requester.Defaults.ApproverUsername);
            if (approver != null) {
                input.ApproverName = approver.FirstName + " " + approver.LastName;
                input.ApproverEmail = approver.Email;
                input.ApproverId = approver.Username;
            }
        }

        input.PrUrl=$"http://purchasing.seti.com/action/{input.Id.ToString()}/{(int)PrUserAction.APPROVE}";
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
                Initials = input.RequesterInitials
            },
            Department = input.Department,
            Vendor = input.Vendor,
            AdditionalComments = input.AdditionalComments,
            Created = this._timeProvider.Now(),
            Quotes = input.Quotes,
            ShippingType = input.ShippingType,
            PurchaseItems = input.PurchaseItems,
            EmailCopyList = input.EmailCcList
        };
        input.PrUrl=$"http://172.20.4.207/action/{purchaseRequest._id.ToString()}/{(int)PrUserAction.APPROVE}";
        purchaseRequest.PrUrl = input.PrUrl;
        if (string.IsNullOrEmpty(input.RequesterEmail) || string.IsNullOrEmpty(input.ApproverEmail)) {
            this._logger.LogError("Error creating purchase request, Approver or Requester were missing emailsr");
            return false;
        }
        List<string> to = [input.ApproverEmail];
        List<string> cc = [input.RequesterEmail];
        if (input.EmailCcList?.Any() == true) {
            cc.AddRange(input.EmailCcList);
        }
        await this._requestDataService.InsertOne(purchaseRequest);
        var exists = await this._requestDataService.Exists(purchaseRequest._id);
        if (!exists) {
            this._logger.LogError("Error creating purchase request, failed to insert into database");
            return false;
        }
        await this._emailService.SendRequestEmail(input.EmailTemplate ?? [], input,
            to,
            cc);
        this._logger.LogInformation("Purchase request created successfully {RequestId}",input.Title);
        return true;
    }
    public async Task<bool> UpdatePurchaseRequest(PurchaseRequestInput input) {
        Console.WriteLine($"Updating Purchase Request {input.Id}");
        if(!input.Id.HasValue) {
            this._logger.LogError("Error updating purchase request, Id was missing");
            return false;
        }
        if (input.Status == PrStatus.NeedsApproval) {
            input.PrUrl=$"http://purchasing.seti.com/action/{input.Id.ToString()}/{(int)PrUserAction.APPROVE}";
        } else {
            input.PrUrl=$"http://purchasing.seti.com/action/{input.Id.ToString()}/{(int)PrUserAction.ORDER}";
        }
        if (string.IsNullOrEmpty(input.RequesterEmail) || string.IsNullOrEmpty(input.ApproverEmail)) {
            this._logger.LogError("Error updating purchase request, Approver or Requester were missing emails");
            return false;
        }
        
        List<string> to = [input.ApproverEmail];
        List<string> cc = [input.RequesterEmail];
        
        if(input.Status == PrStatus.Approved) {
            var purchaserList =await this._authApiService.GetPurchasers();
            var purchasers= purchaserList?.Where(e=>e.Email!="itsupport@s-et.com").Select(e => e.Email).ToList();
            if(purchasers!=null || purchasers?.Count>0) {
                to.AddRange(purchasers);
            }
        }
        
        if (input.EmailCcList?.Any() == true) {
            cc.AddRange(input.EmailCcList);
        }
        var pr = new PurchaseRequest().FromInput(input);
        bool success=await this._requestDataService.UpdateOne(pr);
        Console.WriteLine("Updated Purchase Request: "+success);
        if (!success) {
            this._logger.LogError("Error updating purchase request, failed to update in database");
            return false;
        }
        
        foreach(var quote in input.Quotes) {
            var fileData=await this._fileService.DownloadFile(quote,this._configuration["AppDomain"] ?? "purchase_request");
            if (fileData != null) {
                input.Attachments.Add(new FileInput(fileData.Name,fileData.Data));
            }
        }
        await this._emailService.SendRequestEmail(input.EmailTemplate ?? [],input, 
            to,
            cc);
        this._logger.LogInformation("Purchase request updated successfully {RequestId}",input.Title);
        return true;
    }
    public async Task<bool> CancelPurchaseRequest(CancelRequestInput input) {
        bool includePurchaser = false;
        var request = await this._requestDataService.GetPurchaseRequest(input.Id);
        if (request == null) {
            this._logger.LogError("Error cancelling purchase request, request not found");
            return false;
        }
        
        if (request.Purchaser != null) {
            if (string.IsNullOrEmpty(request.Requester.Email)){
                this._logger.LogError("Error updating purchase request, Requester was missing email");
                return false;
            }
            includePurchaser = true;
        }
        if (string.IsNullOrEmpty(request.Requester.Email) || string.IsNullOrEmpty(request.Approver.Email)) {
            this._logger.LogError("Error updating purchase request, Approver or Requester were missing emails");
            return false;
        }

        List<string> to;
        to = [request.Requester.Email];
        List<string> cc=[request.Approver.Email];
        if (includePurchaser) {
            var purchaserList =await this._authApiService.GetPurchasers();
            var purchasers= purchaserList?.Where(e=>e.Email!="itsupport@s-et.com").Select(e => e.Email).ToList();
            if(purchasers!=null || purchasers?.Count>0) {
                cc.AddRange(purchasers);
            }
        }
        if (request.EmailCopyList?.Any() == true) {
            cc.AddRange(request.EmailCopyList);
        }
        if(request.Status == PrStatus.Ordered) {
            request.Status = PrStatus.Rejected;
            request.RejectedDate = this._timeProvider.Now();
            await this._requestDataService.UpdateOne(request);
            await this._emailService.SendCancellationEmail(input.EmailTemplate ?? [], input.Title ?? "Unknown",
                to,
                cc);
            return true;
        } else {
            var deleted = await this._requestDataService.DeletePurchaseRequest(input.Id);
            if (!deleted) return false;
            foreach (var quote in input.FileIds) {
                await this._fileService.DeleteFile(quote,this._configuration["AppDomain"] ?? "purchase_request");
            }
            await this._emailService.SendCancellationEmail(input.EmailTemplate ?? [], input.Title ?? "Unknown",
                to,
                cc);
            return true;
        }
    }
    public async Task<bool> ApproveRejectPurchaseRequest(ApproveRequestInput input,PurchaseRequest request) {
        if (string.IsNullOrEmpty(request.Requester.Email) || string.IsNullOrEmpty(request.Approver.Email)) {
            this._logger.LogError("Error approving/rejecting purchase request, Approver or Requester were missing emails");
            return false;
        }
        
        bool approved = input.Action==PurchaseRequestAction.Approve ? true : false;
        request.ApprovalResult = new ApprovalResult() {
            Approved = approved, 
            Comments = input.Comment ?? ""
        };
        List<string> to;
        List<string> cc;
        if(input.Action == PurchaseRequestAction.Approve) {
            request.ApprovedDate = this._timeProvider.Now();
            var purchaserList =await this._authApiService.GetPurchasers();
            var purchasers= purchaserList.Where(e=>e.Email!="itsupport@s-et.com").Select(e => e.Email).ToList();
            if(purchasers==null || purchasers.Count==0) {
                to = [request.Requester.Email];
            } else {
                to = purchasers;
            }
            cc = [request.Requester.Email,request.Approver.Email];
        } else {
            request.RejectedDate = this._timeProvider.Now();
            to = [request.Requester.Email];
            cc = [request.Approver.Email];
        }
        
        if (request.EmailCopyList?.Any() == true) {
            cc.AddRange(request.EmailCopyList);
        }
        
        request.Status = approved ? PrStatus.Approved : PrStatus.Rejected;
        if (approved) {
            request.PrUrl = $"http://purchasing.seti.com/action/{request._id.ToString()}/{(int)PrUserAction.ORDER}";
        }
        
        var success=await this._requestDataService.UpdateOne(request);
        List<FileData> files = [];
        foreach (var quote in request.Quotes) {
            var fileData=await this._fileService.DownloadFile(quote,this._configuration["AppDomain"] ?? "purchase_request");
            if (fileData != null) {
                files.Add(fileData);
            }
        }
        if (!success) {
            this._logger.LogError("Error approving/rejecting purchase request, failed to update in database");
            return false;
        }
        var document = new PurchaseRequestDocument(request.ToInput(),Path.Combine($"{this._environment.WebRootPath}","images/seti_logo.png"));
        await this._emailService.SendApprovalEmail(input.EmailDocument ?? [],request.Title ?? "Not Titled",approved,
            request.PrUrl ?? "",
            document.GeneratePdf(),
            files,
            to,
            cc);
        return true;
    }
    public async Task<bool> OrderPurchaseRequest(PurchaseOrderDto order,byte[] emailDocument) {
        var request = await this._requestDataService.UpdateFromOrder(order);
        if (request == null) return false;
        
        if (string.IsNullOrEmpty(request.Requester.Email) || string.IsNullOrEmpty(request.Approver.Email)) {
            this._logger.LogError("Error ordering purchase request, Approver or Requester were missing emails");
            return false;
        }
        List<string> to = [request.Requester.Email];
        List<string> cc = [request.Purchaser?.Email ?? "space@s-et.com",request.Approver.Email];
        if (request.EmailCopyList?.Any() == true) {
            cc.AddRange(request.EmailCopyList);
        }
        var document = new PurchaseOrderDocument(request.ToPurchaseOrderDto(),Path.Combine($"{this._environment.WebRootPath}","images/seti_logo.png"));
        await this._emailService.SendOrderEmail(emailDocument,request.Title ?? "Not Titled",
            document.GeneratePdf(),
            [],
            to,
            cc);
        return true;
    }
    public async Task<bool> ReceivePurchaseOrder(ReceiveRequestInput input) {
        var request = await this._requestDataService.UpdateFromReceive(input);
        if (request == null) {
            this._logger.LogError("Error receiving purchase request, request not found");
            return false;
        }
        if (string.IsNullOrEmpty(request.Requester.Email) || string.IsNullOrEmpty(request.Approver.Email)) {
            this._logger.LogError("Error receiving purchase request, Approver or Requester are missing emails");
            return false;
        }
        List<string> to = [request.Requester.Email];
        List<string> cc = [request.Purchaser?.Email ?? "space@s-et.com",request.Approver.Email];
        //List<string> cc = [request.Approver.Email];
        if (request.EmailCopyList?.Any() == true) {
            cc.AddRange(request.EmailCopyList);
        }
        var document = new PurchaseOrderDocument(request.ToPurchaseOrderDto(),Path.Combine($"{this._environment.WebRootPath}","images/seti_logo.png"));
        await this._emailService.SendReceivedEmail(input.EmailDocument ?? [],request.Title ?? "Not Titled",
            document.GeneratePdf(),
            request.Status==PrStatus.Delivered,
            [],
            to,
            cc);
        return true;
    }
    public async Task<PurchaseOrderDto> GetPurchaseOrderDto(ObjectId requestId) {
        var request = await this._requestDataService.GetPurchaseRequest(requestId);
        var po=request.ToPurchaseOrderDto();
        return po;   
    }
    public async Task<List<QuotesDto>> GetQuotes() {
        var quotes=await this._requestDataService.GetQuotes();
        foreach(var quote in quotes) {
            var fileData=await this._fileService.GetFileInfo(quote.FileId,this._configuration["AppDomain"] ?? "purchase_request");
            if (fileData != null) {
                quote.Filename = fileData.Filename;
            }
            quote.Url = this._configuration["FileServiceUrl"];
            if (!string.IsNullOrWhiteSpace(quote.Url)) {
                quote.Url=quote.Url.Remove(quote.Url.Length);
                quote.Url+=HttpConstants.FileDownloadInlinePath
                    .Replace("{appDomain}","purchase_request")
                    .Replace("{fileId}",quote.FileId);   
            }
        }
        return quotes;
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
    
    public async Task<List<PurchaseRequest>> GetPurchaseRequests(string username, string role,string email) {
        return await this._requestDataService.GetPurchaseRequests(username,role,email);
    }
}