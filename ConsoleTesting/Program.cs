﻿// See https://aka.ms/new-console-template for more information

using Domain.PurchaseRequests.Model;
using Domain.Users;
using MongoDB.Driver;
using SETiAuth.Domain.Shared.Constants;
using Infrastructure.Services;
using ClosedXML.Excel;
using Domain.PurchaseRequests.Pdf;
using Domain.PurchaseRequests.TypeConstants;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Companion;

//Console.WriteLine($"Address: {HttpClientConstants.LoginApiUrl}");
//await TestMongoIdString();
//await TestMongoQueryIdString();
//await TestSendEmail();
//await CreateVendors();
//await TestExcel();

QuestPDF.Settings.License = LicenseType.Community;

/*var model = await GetPurchaseRequest();
var document = new PurchaseRequestDocument(model);
document.GeneratePdf(@"C:\Users\aelme\Documents\PurchaseRequestData\PurchaseRequest.pdf");*/

var model = await GetPurchaseOrderDto();
var document = new PurchaseOrderDocument(model);
document.GeneratePdf(@"C:\Users\aelme\Documents\PurchaseRequestData\PurchaseOrder.pdf");
//await document.ShowInCompanionAsync();

async Task<PurchaseRequest> GetPurchaseRequest() {
    string path = @"C:\Users\aelmendo\Documents\PurchaseRequestData\PurchaseRequestForm.xlsm";
    var client=new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("purchase_req_db");
    var collection = database.GetCollection<Vendor>("vendors");
    var purchaseRequest=new PurchaseRequest();
    List<PurchaseItem> items = new();
    items.Add(new PurchaseItem() {
        ProductName = "Star tech 4 port video splitter/546287str", Quantity = 1, UnitCost = 45.69m
    });
    items.Add(new PurchaseItem() {
        ProductName = "Logitech Mouse/Keyboard Combo/mk432", Quantity = 1, UnitCost = 24.65m
    });
    items.Add(new PurchaseItem() {
        ProductName = "Intel I7 546214 mini pc", Quantity = 3, UnitCost = 459.45m
    });
    purchaseRequest.Items = items;
    var vendor = await collection.Find(e => e.Name == "Amazon.com").FirstOrDefaultAsync();
    if (vendor != null) {
        purchaseRequest.Department = new Department(){Name = "Epi"};
        purchaseRequest.Description = "Consultant Computers";
        purchaseRequest.Vendor = vendor;
        purchaseRequest.ShippingType = ShippingType.Ground.Value;
        purchaseRequest.PaymentTerms = PaymentTerm.NetTerms.Value;
        purchaseRequest.Username = "Amanda Elmore";
        purchaseRequest.Urgent = true;
        purchaseRequest.Approver = "Rakesh Jain";
        purchaseRequest.Title = "Consultant Computers";
        purchaseRequest.Created = DateTime.Now;
        purchaseRequest.AdditionalComments = "The consultant computers are for the HQ consultants. " +
                                            "One computer with windows 11 and a display. Another computer with windows 11 and a display. " +
                                            " Also two keyboards and a mouse for each computers.  " +
                                            "Another filler line to increase the Reason For Purchase. " +
                                            " I want to make sure the wrapping works when writing to a single cell";
        purchaseRequest.Approved = false;
        purchaseRequest.Rejected = false;
    }

    return purchaseRequest;
}

async Task<PurchaseOrderDto> GetPurchaseOrderDto() {
    string path = @"C:\Users\aelmendo\Documents\PurchaseRequestData\PurchaseRequestForm.xlsm";
    var client=new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("purchase_req_db");
    var collection = database.GetCollection<Vendor>("vendors");
    var purchaseOrder=new PurchaseOrderDto();
    List<PurchaseItem> items = new();
    items.Add(new PurchaseItem() {
        ProductName = "Star tech 4 port video splitter/546287str", Quantity = 1, UnitCost = 45.69m
    });
    items.Add(new PurchaseItem() {
        ProductName = "Logitech Mouse/Keyboard Combo/mk432", Quantity = 1, UnitCost = 24.65m
    });
    items.Add(new PurchaseItem() {
        ProductName = "Intel I7 546214 mini pc", Quantity = 3, UnitCost = 459.45m
    });
    purchaseOrder.Items = items;
    var vendor = await collection.Find(e => e.Name == "Amazon.com").FirstOrDefaultAsync();
    if (vendor != null) {
        purchaseOrder.Department = "Support";
        purchaseOrder.Description = "Consultant Computers";
        purchaseOrder.Vendor = vendor;
        purchaseOrder.ShippingMethod = ShippingType.Ground.Value;
        purchaseOrder.PaymentTerms = PaymentTerm.NetTerms.Value;
        purchaseOrder.Requester = "Amanda Elmore";
        purchaseOrder.ShipTo = "SETi";
        purchaseOrder.FOB = "Destination";
        purchaseOrder.TotalCost = items.Sum(e => e.Quantity * e.UnitCost);
    }
    return purchaseOrder;
}

async Task TestExcel() {
    string path = @"C:\Users\aelmendo\Documents\PurchaseRequestData\PurchaseRequestForm.xlsm";
    var client=new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("purchase_req_db");
    var collection = database.GetCollection<Vendor>("vendors");
    var wb=new XLWorkbook(path);
    var prSheet= wb.Worksheet("Po Req.");

    var vendor = await collection.Find(e => e.Name == "Amazon.com").FirstOrDefaultAsync();
    if (vendor != null) {
        prSheet.Cell(ExcelMap.RequestDate.Value).Value = DateTime.Now.ToString("MM/dd/yyyy");
        prSheet.Cell(ExcelMap.Requester.Value).Value = "Amanda Elmore";
        prSheet.Cell(ExcelMap.Department.Value).Value = "Support";
        prSheet.Cell(ExcelMap.VendorName.Value).Value = vendor.Name;
        prSheet.Cell(ExcelMap.Contact.Value).Value = vendor.Contact;
        prSheet.Cell(ExcelMap.StreetAddress.Value).Value = vendor.StreetAddress;
        prSheet.Cell(ExcelMap.CityStateZip.Value).Value = vendor.CityStateZip;
        prSheet.Cell(ExcelMap.Phone.Value).Value = vendor.Phone;
        prSheet.Cell(ExcelMap.Email.Value).Value = vendor.Email;
        prSheet.Cell(ExcelMap.PaymentTerms.Value).Value=PaymentTerm.NetTerms.Value;
        prSheet.Cell(ExcelMap.Description.Value).Value = "Consultant Computers";
        prSheet.Cell(ExcelMap.PurchaseReason.Value).Value = "Computers for HQ Consultants. One computer with windows 11 " +
                                                            "and a display. Another computer with windows 11 and a display. " +
                                                            " Also two keyboards and a mous for each computers.  " +
                                                            "Another filler line to increase the Reason For Purchase. " +
                                                            " I want to make sure the wrapping works when writing to a single sell";
        prSheet.Cell(ExcelMap.DeliveryMethod.Value).Value = ShippingType.Ground.Value;
        
        List<PurchaseItem> items = new();
        items.Add(new PurchaseItem() {
            ProductName = "Star tech 4 port video splitter/546287str", Quantity = 1, UnitCost = 45.69m
        });
        items.Add(new PurchaseItem() {
            ProductName = "Logitech Mouse/Keyboard Combo/mk432", Quantity = 1, UnitCost = 24.65m
        });
        items.Add(new PurchaseItem() {
            ProductName = "Intel I7 546214 mini pc", Quantity = 3, UnitCost = 459.45m
        });
        Console.WriteLine($"EndRow: {ExcelMap.QuantityLines.EndRow}");
        int rowCount=items.Count<ExcelMap.QuantityLines.RowCount ? items.Count:ExcelMap.QuantityLines.RowCount;
        for (int i = 0; i < rowCount; i++) {
            
            prSheet.Cell(ExcelMap.QuantityLines.Row + i, ExcelMap.QuantityLines.Column).Value = items[i].Quantity;
            prSheet.Cell(ExcelMap.ProductDescription.Row + i, ExcelMap.ProductDescription.Column).Value = items[i].ProductName;
            prSheet.Cell(ExcelMap.UnitCost.Row + i, ExcelMap.UnitCost.Column).Value = items[i].UnitCost;
            items[i].TotalCost=items[i].Quantity * items[i].UnitCost;
            prSheet.Cell(ExcelMap.ProductTotal.Row + i, ExcelMap.ProductTotal.Column).Value = items[i].Quantity * items[i].UnitCost;
        }
        wb.SaveAs(@"C:\Users\aelmendo\Documents\PurchaseRequestData\PurchaseRequestFormFilled.xlsm");
    } else {
        Console.WriteLine("Vendor not found");
    }
}

async Task CreateVendors() {
    string path =@"C:\Users\aelmendo\Documents\PurchaseRequestItems\Vendors.txt";
    var client=new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("purchase_req_db");
    var collection = database.GetCollection<Vendor>("vendors");
    await using var stream=File.OpenRead(path);
    using var reader=new StreamReader(stream);
    List<Vendor> vendors = [];
    while (!reader.EndOfStream) {
        //Console.WriteLine(reader.ReadLine());
        var items = reader.ReadLine()?.Split('\t');
        if (items != null && items.Length > 0) {
            Vendor vendor = new() {
                Name = items[0], 
                StreetAddress = items[1], 
                CityStateZip = items[2],
                Contact = items[3],
                Phone = items[4], 
                Fax = items[5],
                Email = items[6]
            };
            vendors.Add(vendor);
        }
    }
    await collection.InsertManyAsync(vendors);
    Console.WriteLine("Check database");
}

async Task TestSendEmail() {
    EmailService emailService = new();
    //await emailService.SendEmail(["rakesh@s-et.com"], ["aelmendorf@s-et.com","space@s-et.com"]);
}

async Task TestMongoIdString() {
    var client=new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("purchase_req_db");
    var collection = database.GetCollection<UserProfile>("user_profiles");
    UserProfile profile = new() { _id = "aelmendo", Email = "" };
    await collection.InsertOneAsync(profile);
    Console.WriteLine("Check database");
}

async Task TestMongoQueryIdString() {
    var client=new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("purchase_req_db");
    var collection = database.GetCollection<UserProfile>("user_profiles");
    collection.Find(e=>e._id=="aelmendo").ToList().ForEach(e=>Console.WriteLine(e._id));
}

