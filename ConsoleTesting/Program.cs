// See https://aka.ms/new-console-template for more information

using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Domain.PurchaseRequests.Model;
using Domain.Users;
using MongoDB.Driver;
using SETiAuth.Domain.Shared.Constants;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Infrastructure.Services;
using ClosedXML.Excel;
using Domain;
using Domain.Assets;
using Domain.PurchaseRequests.Dto;
using Domain.PurchaseRequests.Pdf;
using Domain.PurchaseRequests.TypeConstants;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Linq;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Companion;
using SetiFileStore.Domain.Contracts;
using SetiFileStore.Domain.Contracts.Responses;
using SetiFileStore.FileClient;
using TimeProvider = Infrastructure.Services.TimeProvider;

//Console.WriteLine($"Address: {HttpClientConstants.LoginApiUrl}");
//await TestMongoIdString();
//await TestMongoQueryIdString();
//await TestSendEmail();
//await CreateVendors();
//await CreateDepartments();
//await TestExcel();
//await PdfWork();
//await TestReadContentJson();

/*var arr2 = "\rMSG"u8.ToArray();
var arr = "\rDOC"u8.ToArray();
foreach (var a in arr) {
    /*Console.WriteLine(a);#1#
    Console.WriteLine ("Hex: {0:X}", a);
}*/

//await TestPdfUpload();

//FileService fileService = new FileService();
//await fileService.DownloadFile("66f5a3b92b11ab38ca3296a1");
//await fileService.DownloadFileStream("66f5ae982b11ab38ca3296a5");
//await fileService.UploadMultipleFiles(["C:\\Users\\aelme\\Documents\\PurchaseRequestData\\PurchaseRequest.pdf","C:\\Users\\aelme\\Documents\\PurchaseRequestData\\PurchaseRequest-2.pdf"]);
//await fileService.UploadFile("C:\\Users\\aelme\\Documents\\PurchaseRequestData\\PurchaseRequest.pdf");

/*Console.WriteLine(TimeProvider.Now());*/

//await TestMessageOrderV2();

//await CreateAvatarFiles();

//await TestPoCollection();
//await TestGeneratePoNumber();
//await DeleteAllPoNumbers();

//await ComplexQueryTest();
//await CheckMongoTime();

List<UserActionAlert> alerts = [
    new UserActionAlert() { Okay = false, Item = "Item1" },
    new UserActionAlert() { Okay = false, Item = "Item2" }, 
    new UserActionAlert() { Okay = false, Item = "Item3" },
    new UserActionAlert() { Okay = false, Item = "Item4" },
    new UserActionAlert() { Okay = true, Item = "Item5" }, 
    new UserActionAlert() { Okay = false, Item = "Item6" }
];

Console.WriteLine("Before Sort:");
foreach (var alert in alerts) {
    Console.WriteLine($"{alert.Item}: {alert.Okay}");
}
alerts.Sort();

Console.WriteLine("After Sort:");
foreach (var alert in alerts) {
    Console.WriteLine($"{alert.Item}: {alert.Okay}");
}


async Task CheckMongoTime() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("purchase_req_db");
    var collection = database.GetCollection<PurchaseRequest>("purchase_requests");
    
    using var cursor = await collection.FindAsync(_=>true,new FindOptions<PurchaseRequest>() {
        BatchSize = 10
    });

    while (await cursor.MoveNextAsync())
    {
        var batch = cursor.Current;

        foreach (var item in batch) {
            Console.WriteLine($"MongoTime: {item.Created:h:mm:ss tt zz} " +
                              $" AdjustedTime: {TimeProvider.ToLocal(item.Created):h:mm:ss tt zz}" +
                              $" CurrentTime: {TimeProvider.Now():h:mm:ss tt zz}" +
                              $" DaysSince: {TimeProvider.DaysSince(item.Created)}");
        }
    }
}

async Task ComplexQueryTest() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("purchase_req_db");
    var collection = database.GetCollection<Contact>("contacts").OfType<Vendor>();
    
    using var cursor = await collection.FindAsync(_=>true,new FindOptions<Vendor>() {
        BatchSize = 10
    });
    int batchCount = 0;
    while (await cursor.MoveNextAsync())
    {
        var batch = cursor.Current;
        batchCount++;
        foreach (var item in batch) {
           
            Console.WriteLine($"BatchCount: {batchCount} ItemName: {item.Name}");
            Console.WriteLine();
        }
    }
}

async Task DeleteAllPoNumbers() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("purchase_req_db");
    var collection = database.GetCollection<PoNumber>("po_numbers");
    await collection.DeleteManyAsync(_ => true);
    PoNumber poNumber = new() {Seq = 0, Initials = "NA",Year = 2024,RequestId = ObjectId.GenerateNewId()};
    poNumber._id = "2024-NA-0000";
    await collection.InsertOneAsync(poNumber);
    Console.WriteLine("Check database");
}

async Task TestGeneratePoNumber() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("purchase_req_db");
    var collection = database.GetCollection<PoNumber>("po_numbers");
    var builder = Builders<PoNumber>.Sort;
    var sort = builder.Descending(f => f.Seq);

    var result=await collection.Find(_ => true)
        .Project(e=>e.Seq)
        .Sort(Builders<PoNumber>.Sort.Descending(f=>f.Seq)).Limit(1)
        .FirstOrDefaultAsync();
    
    if (result>0) {
        Console.WriteLine($"PoNumber: {result}");
        PoNumber poNumber = new() { Initials = "AE",Year = 2024 };
        poNumber.Seq = result + 1;
        if(poNumber.Seq/1000>0) {
            poNumber._id=$"{poNumber.Year}-{poNumber.Initials}-{poNumber.Seq}";
        } else {
            if(poNumber.Seq/100>0) {
                poNumber._id=$"{poNumber.Year}-{poNumber.Initials}-0{poNumber.Seq}";

            } else {
                if(poNumber.Seq/10>0) {
                    poNumber._id=$"{poNumber.Year}-{poNumber.Initials}-00{poNumber.Seq}";
                } else {
                    poNumber._id=$"{poNumber.Year}-{poNumber.Initials}-000{poNumber.Seq}";
                }
            }
        }
        Console.WriteLine($"New PoNumber: {poNumber._id}");
        await collection.InsertOneAsync(poNumber);
    } else {
        Console.WriteLine("No PoNumber found");
    }
    
    
}

async Task TestPoCollection() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("purchase_req_db");
    var collection = database.GetCollection<PoNumber>("po_numbers");
    //await collection.Indexes.CreateOneAsync(Builders<PoNumber>.IndexKeys.Ascending(e => e.Seq),new CreateIndexOptions() { Unique = true });
    var indexModel= new CreateIndexModel<PoNumber>(Builders<PoNumber>.IndexKeys.Ascending(e => e.Seq),new CreateIndexOptions() { Unique = true });
    await collection.Indexes.CreateOneAsync(indexModel);
    
    /*var filter = Builders<PoNumber>.Filter.Eq(e => e._id, "2024-AE-1621");
    var update = Builders<PoNumber>.Update.Inc(e => e.Seq, 1);
    var options = new FindOneAndUpdateOptions<PoNumber> {
        ReturnDocument = ReturnDocument.After
    };
    var result = await collection.FindOneAndUpdateAsync(filter, update, options);*/

    for (int i = 0; i < 1000; i++) {
         PoNumber poNumber = new() { Seq = i,Initials = "AE",Year = 2024 };
         if(poNumber.Seq/1000>0) {
              poNumber._id=$"{poNumber.Year}-{poNumber.Initials}-{poNumber.Seq}";
         } else {
             if(poNumber.Seq/100>0) {
                 poNumber._id=$"{poNumber.Year}-{poNumber.Initials}-0{poNumber.Seq}";

             } else {
                 if(poNumber.Seq/10>0) {
                     poNumber._id=$"{poNumber.Year}-{poNumber.Initials}-00{poNumber.Seq}";
                 } else {
                     poNumber._id=$"{poNumber.Year}-{poNumber.Initials}-000{poNumber.Seq}";
                 }
             }
         }
         Console.WriteLine($"Inserting: {poNumber._id}");
         await collection.InsertOneAsync(poNumber);
    }
    Console.WriteLine("Check database");
}

void TestNumberFormat() {
    int value = 1;

    for(int i=0;i<4;i++) {
    
    
    
        if(value/1000>0) {
            Console.WriteLine($"Value: {value} Modified: {value}");
        } else {
            if(value/100>0) {

                Console.WriteLine($"Value: {value} Modified: 0{value}");
            } else {
                if(value/10>0) {

                    Console.WriteLine($"Value: {value} Modified: 00{value}");
                } else {
                    Console.WriteLine($"Value: {value} Modified: 000{value}");
                }
            }
        }
        value*=10;
    }
}

async Task CreateAvatarFiles() {
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("website_shared_db");
    var collection = database.GetCollection<Avatar>("website_avatars");
    string basePath = "C:\\Users\\aelmendo\\RiderProjects\\Purchase-Request-WebApp\\Webapp\\wwwroot\\images\\avatars";
    var avatars = new List<Avatar>();
    var files = Directory.GetFiles(basePath)
        .Select(Path.GetFileName)
        .ToList();
    List<FileData> fileData = [];
    foreach (var file in files) {
        if (!string.IsNullOrEmpty(file)) {
            var tmp = Path.GetFileNameWithoutExtension(file);
            if (!string.IsNullOrEmpty(tmp)) {
                var split = tmp.Split('-');
                Console.WriteLine(file);
                Console.Write($"Count:{split.Length} Array: ");
                for (int i = 0; i < split.Length; i++) {
                    Console.Write($"[{i}]: {split[i]}");
                }

                Console.WriteLine();
                var fileBytes = File.ReadAllBytes(Path.Combine(basePath, file));
                fileData.Add(new FileData(file, fileBytes));
            }
        }
    }

    Console.WriteLine("Upload files, please wait....");
    /*List<string> results = [];*/
    foreach (var data in fileData) {
        var result = await UploadFile(data);
        if (result != null) {
            /*results.Add(result);*/
            await collection.InsertOneAsync(new Avatar() {
                _id = ObjectId.GenerateNewId(),
                Name = data.Name,
                FileId = result,
            });
            Console.WriteLine($"File uploaded. FileId: {result}");
        } else {
            Console.WriteLine("File upload failed");
        }
    }

    /*Console.WriteLine("Files uploaded, creating avatar records....");
    if (results.Count == fileData.Count) {
        for (int i = 0; i < results.Count; i++) {
            avatars.Add(new Avatar() { Name = fileData[i].Name, FileName = results[i] });
        }

        await collection.InsertManyAsync(avatars);
        Console.WriteLine("Avatar database created, check database");
    } else {
        Console.WriteLine("Some files failed to upload, delete the database and try again");
    }*/
}


async Task<string?> UploadFile(FileData data) {
    using var client = new HttpClient();
    client.BaseAddress = new Uri("http://localhost:5065/");
    using var form = new MultipartFormDataContent();
    using var fileContent = new ByteArrayContent(data.Data);
    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
    form.Add(fileContent, "file",data.Name);
    var domain="avatar";
    if (string.IsNullOrEmpty(domain)) {
        throw new Exception(message: "Missing required configuration: AppDomain. " +
                                     "Please check your appsettings.json file.");
    }
    form.Add(new StringContent(domain), "appDomain");
    //form.Add(new StringContent("purchase_request"), "appDomain");
    HttpResponseMessage response = await client.PostAsync(HttpConstants.FileUploadPath, form);
    Console.WriteLine(response.ToString());
    if (response.IsSuccessStatusCode) {
        var content =await response.Content.ReadFromJsonAsync<FileUploadResponse>();
        return content?.FileId;
    } else {
        return default;
    }
}

async Task<List<string>> UploadMultipleFiles(List<FileData> input) {
    using var client = new HttpClient();
    client.BaseAddress = new Uri("http://localhost:5065/");
    using var form = new MultipartFormDataContent();
    foreach (var fileInput in input) {
        var fileContent = new ByteArrayContent(fileInput.Data);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
        form.Add(fileContent, "files", fileInput.Name);
    }
    var domain="avatar";
    if (string.IsNullOrEmpty(domain)) {
        throw new Exception(message: "Missing required configuration: AppDomain. " +
                                     "Please check your appsettings.json file.");
    }
    form.Add(new StringContent(domain), "appDomain");
    HttpResponseMessage response = await client.PostAsync(HttpConstants.MultiFileUploadPath, form);
    if (response.IsSuccessStatusCode) {
        var content =await response.Content.ReadFromJsonAsync<MultipleFileUploadResponse>();
        return content?.FileIds ?? new List<string>();
    } else {
        return [];
    }
}

async Task TestMessageOrderV2() {
    var now = DateTime.Now;
    List<ChatMessage> messages = new() {
        new ChatMessage(now, "aelmendo", "Hello"),
        new ChatMessage(now.AddSeconds(5), "aelmendo", "How are you?"),
        new ChatMessage(now.AddSeconds(10), "aelmendo", "I am fine"),
        new ChatMessage(now.AddSeconds(15), "rjain", "What are you doing?"),
        new ChatMessage(now.AddSeconds(20), "rjain", "I am working on a project"),
        new ChatMessage(now.AddSeconds(25), "aelmendo", "What are you doing?"),
        new ChatMessage(now.AddSeconds(30), "rjain", "I am working on a project"),
        new ChatMessage(now.AddSeconds(35), "jdoe", "Hello everyone"),
        new ChatMessage(now.AddSeconds(40), "jdoe", "How's it going?")
    };

    var sortedMessages = messages.OrderBy(m => m.Timestamp).ToList();
    var groupedMessages = new List<List<ChatMessage>>();
    var currentGroup = new List<ChatMessage>();

    for (int i = 0; i < sortedMessages.Count; i++) {
        if (currentGroup.Count == 0 ||
            (sortedMessages[i].Username == currentGroup.Last().Username &&
             sortedMessages[i].Timestamp - currentGroup.Last().Timestamp <= TimeSpan.FromSeconds(20))) {
            currentGroup.Add(sortedMessages[i]);
        } else {
            groupedMessages.Add(new List<ChatMessage>(currentGroup));
            currentGroup.Clear();
            currentGroup.Add(sortedMessages[i]);
        }
    }

    if (currentGroup.Count > 0) {
        groupedMessages.Add(currentGroup);
    }

    foreach (var group in groupedMessages) {
        Console.WriteLine($"Messages from {group.First().Username}:");
        foreach (var message in group) {
            Console.WriteLine($"{message.Timestamp}: {message.Message}");
        }
        Console.WriteLine();
    }
}

async Task TestMessageOrdering() {
    var now=DateTime.Now;
    List<ChatMessage> messages = [
    new ChatMessage(now,"aelmendo","Hello"),
    new ChatMessage(now.AddSeconds(5),"aelmendo","How are you?"),
    new ChatMessage(now.AddSeconds(10),"aelmendo","I am fine"),
    new ChatMessage(now.AddSeconds(15),"rjain","What are you doing?"),
    new ChatMessage(now.AddSeconds(20),"rjain","I am working on a project"),
    new ChatMessage(now.AddSeconds(25),"aelmendo","What are you doing?"),
    new ChatMessage(now.AddSeconds(30),"rjain","I am working on a project")
    ];
    
    var sortedMessages = messages.OrderBy(m => m.Timestamp).ToList();
    var groupedMessages = new List<List<ChatMessage>>();
    var currentGroup = new List<ChatMessage>();

    for (int i = 0; i < sortedMessages.Count; i++) {
        if (currentGroup.Count == 0 || 
            (sortedMessages[i].Username == currentGroup.Last().Username && 
             sortedMessages[i].Timestamp - currentGroup.Last().Timestamp <= TimeSpan.FromMinutes(1))) {
            currentGroup.Add(sortedMessages[i]);
        } else {
            groupedMessages.Add(new List<ChatMessage>(currentGroup));
            currentGroup.Clear();
            currentGroup.Add(sortedMessages[i]);
        }
    }

    if (currentGroup.Count > 0) {
        groupedMessages.Add(currentGroup);
    }

    foreach (var group in groupedMessages) {
        Console.WriteLine($"Messages from {group.First().Username}:");
        foreach (var message in group) {
            Console.WriteLine($"{message.Timestamp}: {message.Message}");
        }
        Console.WriteLine();
    }
}

async Task TestReadContentJson() {
    var jsonString = 
        "[\n  {\n    \"objectId\": \"66f6f924d35e2cf395eadd6f\",\n    \"fileName\": \"PurchaseRequest-2.pdf\",\n    \"isSuccessful\": true,\n    \"errorMessage\": null\n  },\n  {\n    \"objectId\": \"66f6f925d35e2cf395eadd71\",\n    \"fileName\": \"PurchaseRequest - Copy.pdf\",\n    \"isSuccessful\": true,\n    \"errorMessage\": null\n  }\n]";
    var jsonArray=JsonSerializer.Deserialize<JsonArray>(jsonString);
    foreach (var jsonNode in jsonArray) {
        Console.WriteLine(jsonNode["objectId"]);
        Console.WriteLine(jsonNode["fileName"]);
        Console.WriteLine(jsonNode["isSuccessful"]);
        Console.WriteLine(jsonNode["errorMessage"]);
    }
}

/*async Task TestPdfUpload() {
    string url = "http://172.20.4.15:8080/FileStorage/UploadFile";
    string filePath = @"C:\Users\aelme\Documents\PurchaseRequestData\PurchaseRequest-2.pdf";
    using var httpClient = new HttpClient();
    using var form = new MultipartFormDataContent();
    await using var fs = File.OpenRead(filePath);
    using var streamContent = new StreamContent(fs);
    using var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync());
    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
    // "file" parameter name should be the same as the server side input parameter name
    FormFileCollection collection = new FormFileCollection();
    //collection.Add();
    form.Add(fileContent, "file", Path.GetFileName(filePath));
    HttpResponseMessage response = await httpClient.PostAsync(url, form);
}*/


/*async Task UploadFile() {
    var model = await GetPurchaseRequest();
    var document = new PurchaseRequestDocument(model,"C:\\Users\\aelme\\RiderProjects\\Purchase-Request-WebApp\\ConsoleTesting\\seti_logo.png");
    document.GeneratePdf(@"C:\Users\aelme\Documents\PurchaseRequestData\PurchaseRequest.pdf");
    var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("purchase_req_db");
    var bucket = new GridFSBucket(database, new GridFSBucketOptions() { BucketName = "Nitro" });
    var stream=File.OpenRead(@"C:\Users\aelme\Documents\PurchaseRequestData\PurchaseRequest.pdf");
    MD5.Create();
    var id = await bucket.UploadFromStreamAsync("PurchaseRequest.pdf", stream);
}*/


/*async Task PdfWork() {
    QuestPDF.Settings.License = LicenseType.Community;

    
    var model = await GetPurchaseRequest();
    var document = new PurchaseRequestDocument(model,"C:\\Users\\aelme\\RiderProjects\\Purchase-Request-WebApp\\ConsoleTesting\\seti_logo.png");
    /*var documentMetadata = document.GetMetadata();
    Console.WriteLine($"Width: {documentMetadata.}");#1#
    /*document.GeneratePdf(@"C:\Users\aelme\Documents\PurchaseRequestData\PurchaseRequest.pdf");#1#
    await document.ShowInCompanionAsync();
    
    /*var model = await GetPurchaseOrderDto();
    var document = new PurchaseOrderDocument(model,"C:\\Users\\aelme\\RiderProjects\\Purchase-Request-WebApp\\ConsoleTesting\\seti_logo.png");
    //document.GeneratePdf(@"C:\Users\aelme\Documents\PurchaseRequestData\PurchaseOrder.pdf");
    await document.ShowInCompanionAsync();#1#
}*/

async Task<PurchaseRequest> GetPurchaseRequest() {
    string path = @"C:\Users\aelmendo\Documents\PurchaseRequestData\PurchaseRequestForm.xlsm";
    var client=new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("purchase_req_db");
    var collection = database.GetCollection<Contact>("contacts");
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
    items.ForEach(e=>e.TotalCost=e.Quantity * e.UnitCost);
    purchaseRequest.PurchaseItems = items;
    var vendor = await collection.OfType<Vendor>().Find(e => e.Name == "Amazon.com").FirstOrDefaultAsync();
    if (vendor != null) {
        Console.WriteLine("Vendor Found");
        purchaseRequest.Department = new Department(){Name = "Epi"};
        purchaseRequest.Description = "Consultant Computers";
        purchaseRequest.Vendor = vendor;
        purchaseRequest.ShippingType = ShippingTypes.Ground.Value;
        //purchaseRequest.Requester = "Amanda Elmore";
        purchaseRequest.Urgent = true;
        //purchaseRequest.Approver = "Rakesh Jain";
        purchaseRequest.Title = "Consultant Computers";
        purchaseRequest.Created = DateTime.Now;
        purchaseRequest.AdditionalComments = "The consultant computers are for the HQ consultants. " +
                                            "One computer with windows 11 and a display. Another computer with windows 11 and a display. " +
                                            " Also two keyboards and a mouse for each computers.  " +
                                            "Another filler line to increase the Reason For Purchase. " +
                                            " I want to make sure the wrapping works when writing to a single cell";
        purchaseRequest.Status = PrStatus.NeedsApproval;
        /*purchaseRequest.Approved = false;
        purchaseRequest.Rejected = false;*/
    }
    return purchaseRequest;
}

/*async Task<PurchaseOrderDto> GetPurchaseOrderDto() {
    string path = @"C:\Users\aelmendo\Documents\PurchaseRequestData\PurchaseRequestForm.xlsm";
    var client=new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("purchase_req_db");
    var collection = database.GetCollection<Contact>("contacts");
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
    var vendor = await collection.OfType<Vendor>().Find(e => e.Name == "Amazon.com").FirstOrDefaultAsync();
    var contact =await collection.OfType<InternalContact>().Find(e => e.Type==InternalContactType.CompanyMainContact).FirstOrDefaultAsync();
    if (vendor != null) {
        purchaseOrder.Date = DateTime.Now;
        purchaseOrder.PoNumber="2024-AE-1621";
        purchaseOrder.Department = "Support";
        purchaseOrder.Description = "Consultant Computers";
        purchaseOrder.Vendor = vendor;
        purchaseOrder.ShippingMethod = ShippingTypes.Ground.Value;
        purchaseOrder.PaymentTerms = PaymentTerm.NetTerms.Value;
        purchaseOrder.Requester = "Amanda Elmore";
        purchaseOrder.ShipTo = "SETi";
        purchaseOrder.FOB = "Destination";
        purchaseOrder.TotalCost = items.Sum(e => e.Quantity * e.UnitCost);
    }

    if (contact != null) {
        purchaseOrder.ToAddress = contact;
    }
    return purchaseOrder;
}*/

/*async Task TestExcel() {
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
}*/

async Task CreateVendors() {
    string path =@"C:\Users\aelmendo\Documents\PurchaseRequestData\Vendors.txt";
    var client=new MongoClient("mongodb://172.20.3.41:27017");
    var contactDataService = new ContactDataService(client);
    /*var database = client.GetDatabase("purchase_req_db");
    var collection = database.GetCollection<Vendor>("vendors");*/
    await using var stream=File.OpenRead(path);
    using var reader=new StreamReader(stream);
    List<Vendor> vendors = [];
    while (!reader.EndOfStream) {
        //Console.WriteLine(reader.ReadLine());
        var items = reader.ReadLine()?.Split('\t');
        if (items != null && items.Length > 0) {
            Vendor vendor = new() {
                Name = items[0], 
                City = items[1],
                StreetAddress = items[2], 
                Zip = items[3],
                Country = items[4],
                State = items[5],
                Attention = items[6],
                Phone = items[7], 
                Email = ""
            };
            vendors.Add(vendor);
        }
    }
    await contactDataService.InsertMany(vendors);
    
    InternalContact internalContact = new() {
        _id=ObjectId.GenerateNewId(),
        Type = InternalContactType.CompanyMainContact,
        Name="Sensor Electronic Technology, Inc.",
        StreetAddress="110 Atlas Ct",
        City="Columbia",
        State="SC",
        Zip="29209",
        Country = "US",
        Phone="(803) 647-9757",
        Fax="(803) 647-9772",
        Email="ap@s-et.com"
    };
    await contactDataService.InsertOne(internalContact);
    Console.WriteLine("Check database");
}

async Task CreateDepartments() {
    var client=new MongoClient("mongodb://172.20.3.41:27017");
    var departmentDataService = new DepartmentDataService(client);
    List<Department> departmentList = [
        new Department(){ Name="Support", Description="IT and Engineering Support"}, 
        new Department(){ Name="Sales", Description="Sales Purchases"}, 
        new Department(){ Name="Application", Description="Application Purchases"}, 
        new Department(){ Name="Epi-R&D", Description="Epi R&D"}, 
        new Department(){ Name="Epi-Prod", Description="Epi Production"}, 
    ];
    await departmentDataService.InsertMany(departmentList);
    Console.WriteLine("Check database");
}

async Task TestSendEmail() {
    EmailService emailService = new();
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


public record ChatMessage(DateTime Timestamp,string Username,string Message);


public class UserActionAlert:IEquatable<UserActionAlert> , IComparable<UserActionAlert> {
    public string? Item { get; set; }
    public string? Message { get; set; }
    public bool Okay { get; set; }
    
    public bool Equals(UserActionAlert? other) {
        return this.Okay == other?.Okay;
    }

    public int CompareTo(UserActionAlert? other) {
        if(other == null) return -1;
            
        if(this.Okay && other.Okay) {
            return 1;
        } else if (this.Okay && !other.Okay) {
            return -1;
        } else {
            return 0;
        }
    }
}