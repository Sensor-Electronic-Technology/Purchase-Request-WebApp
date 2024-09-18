// See https://aka.ms/new-console-template for more information

using Domain.PurchaseRequests.Model;
using Domain.Users;
using MongoDB.Driver;
using SETiAuth.Domain.Shared.Constants;
using Infrastructure.Services;
using ClosedXML.Excel;
using Domain.PurchaseRequests.TypeConstants;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Companion;

//Console.WriteLine($"Address: {HttpClientConstants.LoginApiUrl}");
//await TestMongoIdString();
//await TestMongoQueryIdString();
//await TestSendEmail();
//await CreateVendors();
//await TestExcel();

QuestPDF.Settings.License = LicenseType.Community;

var model = InvoiceDocumentDataSource.GetInvoiceDetails();
var document = new InvoiceDocument(model);
await document.ShowInCompanionAsync();

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


    public class InvoiceDocument : IDocument
    {
        public static Image LogoImage { get; } = Image.FromFile("C:\\Users\\aelmendo\\RiderProjects\\Purchase-Request-WebApp\\ConsoleTesting\\seti_logo.png");
        
        public InvoiceModel Model { get; }

        public InvoiceDocument(InvoiceModel model)
        {
            Model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);
                    
                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
        }

        void ComposeHeader(IContainer container)
        {
            container.Row(row => {
                row.ConstantItem(150).Image(LogoImage);
                row.Spacing(100);
                row.RelativeItem().Column(column => {
                    column.Item().Text("Purchase Order").FontSize(34).SemiBold().FontColor(Colors.Blue.Darken2);
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column => 
            {
                column.Spacing(20);
                
                column.Item().Row(row =>
                {
                    row.RelativeItem().Component(new AddressComponent("From", Model.SellerAddress));
                    row.ConstantItem(50);
                    row.RelativeItem().Component(new AddressComponent("For", Model.CustomerAddress));
                });

                column.Item().Element(ComposeTable);

                var totalPrice = Model.Items.Sum(x => x.Price * x.Quantity);
                column.Item().PaddingRight(5).AlignRight().Text($"Grand total: {totalPrice:C}").SemiBold();

                if (!string.IsNullOrWhiteSpace(Model.Comments))
                    column.Item().PaddingTop(25).Element(ComposeComments);
            });
        }

        void ComposeTable(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold();
            
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });
                
                table.Header(header =>
                {
                    header.Cell().Text("#");
                    header.Cell().Text("Product").Style(headerStyle);
                    header.Cell().AlignRight().Text("Unit price").Style(headerStyle);
                    header.Cell().AlignRight().Text("Quantity").Style(headerStyle);
                    header.Cell().AlignRight().Text("Total").Style(headerStyle);
                    
                    header.Cell().ColumnSpan(5).PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black);
                });
                
                foreach (var item in Model.Items)
                {
                    var index = Model.Items.IndexOf(item) + 1;

                    table.Cell().Element(CellStyle).Text($"{index}");
                    table.Cell().Element(CellStyle).Text(item.Name);
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Price:C}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Quantity}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"{item.Price * item.Quantity:C}");
                    
                    static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            });
        }

        void ComposeComments(IContainer container)
        {
            container.ShowEntire().Background(Colors.Grey.Lighten3).Padding(10).Column(column => 
            {
                column.Spacing(5);
                column.Item().Text("Comments").FontSize(14).SemiBold();
                column.Item().Text(Model.Comments);
            });
        }
    }
    
    public class AddressComponent : IComponent
    {
        private string Title { get; }
        private Address Address { get; }

        public AddressComponent(string title, Address address)
        {
            Title = title;
            Address = address;
        }
        
        public void Compose(IContainer container)
        {
            container.ShowEntire().Column(column =>
            {
                column.Spacing(2);

                column.Item().Text(Title).SemiBold();
                column.Item().PaddingBottom(5).LineHorizontal(1); 
                
                column.Item().Text(Address.CompanyName);
                column.Item().Text(Address.Street);
                column.Item().Text($"{Address.City}, {Address.State}");
                column.Item().Text(Address.Email);
                column.Item().Text(Address.Phone);
            });
        }
    }

public static class InvoiceDocumentDataSource
{
    private static Random Random = new Random();

    public static InvoiceModel GetInvoiceDetails()
    {
        var items = Enumerable
            .Range(1, 8)
            .Select(i => GenerateRandomOrderItem())
            .ToList();

        return new InvoiceModel
        {
            InvoiceNumber = Random.Next(1_000, 10_000),
            IssueDate = DateTime.Now,
            DueDate = DateTime.Now + TimeSpan.FromDays(14),

            SellerAddress = GenerateRandomAddress(),
            CustomerAddress = GenerateRandomAddress(),

            Items = items,
            Comments = Placeholders.Paragraph()
        };
    }

    private static OrderItem GenerateRandomOrderItem()
    {
        return new OrderItem
        {
            Name = Placeholders.Label(),
            Price = (decimal) Math.Round(Random.NextDouble() * 100, 2),
            Quantity = Random.Next(1, 10)
        };
    }

    private static Address GenerateRandomAddress()
    {
        return new Address
        {
            CompanyName = Placeholders.Name(),
            Street = Placeholders.Label(),
            City = Placeholders.Label(),
            State = Placeholders.Label(),
            Email = Placeholders.Email(),
            Phone = Placeholders.PhoneNumber()
        };
    }
}

public class InvoiceModel
{
    public int InvoiceNumber { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }

    public Address SellerAddress { get; set; }
    public Address CustomerAddress { get; set; }

    public List<OrderItem> Items { get; set; }
    public string Comments { get; set; }
}

public class OrderItem
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class Address
{
    public string CompanyName { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public object Email { get; set; }
    public string Phone { get; set; }
}
