using Domain.PurchaseRequests.Model;
using Domain.PurchaseRequests.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace Domain.PurchaseRequests.Pdf;

public class PurchaseRequestDocument : IDocument {
    public static Image LogoImage { get; } = Image.FromFile("C:\\Users\\aelme\\RiderProjects\\Purchase-Request-WebApp\\ConsoleTesting\\seti_logo.png");
    public PurchaseRequest Model { get; }
    public PurchaseRequestDocument(PurchaseRequest model) {
        Model = model;
    }
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container) {
        container.Page(page => {
                page.Margin(50);
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
            });
    }

    void ComposeHeader(IContainer container) {
        container.Row(row => {
            row.ConstantItem(175).Image(LogoImage);
            row.Spacing(50);
            row.RelativeItem().Column(column => {
                column.Item().Text("Purchase Request").FontSize(34).Bold().FontColor(Colors.Blue.Darken4);
            });
            
        });
    }
    void ComposeContent(IContainer container) {
        container.PaddingTop(10).Column(column => {
            column.Item().Background(Colors.Grey.Lighten1).Padding(3);
            column.Item().Table(table => {
                table.ColumnsDefinition(def => {
                    def.ConstantColumn(100);
                    def.RelativeColumn();
                });
                
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Date Of Request:").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(Model.Created.ToString("MM/dd/yyyy"));
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Requester:").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(Model.Username);
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Department:").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(Model.Department.Name);
                
            });
            column.Item().Background(Colors.Grey.Lighten1).Padding(3);
            column.Item().Table(table => {
                table.ColumnsDefinition(def => {
                    def.ConstantColumn(100);
                    def.RelativeColumn();
                });
                
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Vendor Name: ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(Model.Vendor.Name);
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Contact: ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(Model.Vendor.Contact);
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Address: ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(Model.Vendor.StreetAddress);
                table.Cell().Border(1).Padding(2).AlignLeft().Text("City,State,Zip: ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(Model.Vendor.CityStateZip);
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Phone: ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(Model.Vendor.Phone);
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Email: ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(Model.Vendor.Email);
                
            });
            column.Item().Background(Colors.Grey.Lighten1).Padding(3);
            column.Item().Table(table => {
                table.ColumnsDefinition(def => {
                    def.ConstantColumn(100);
                    def.RelativeColumn();
                });
                
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Payment Terms:  ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(Model.PaymentTerms);
            });
            column.Item().Background(Colors.Grey.Lighten1).Padding(3);
            column.Item().Table(table => {
                table.ColumnsDefinition(def => {
                    def.ConstantColumn(100);
                    def.RelativeColumn();
                });
                
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Description:  ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(Model.Title);
            });
            column.Item().Table(table => {
                table.ColumnsDefinition(def => {
                    def.RelativeColumn();
                });
                
                table.Header(header => {
                    header.Cell().Border(1).Padding(2).AlignCenter().Text("Reason For Purchase");
                });
                table.Cell().Border(1).Padding(2).AlignLeft().Text(this.Model.AdditionalComments).SemiBold();
            });
            column.Item().Background(Colors.Grey.Lighten1).Padding(3);
            column.Item().Table(table => {
                table.ColumnsDefinition(def => {
                    def.ConstantColumn(100);
                    def.RelativeColumn();
                });
                
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Delivery Method:  ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(Model.ShippingType);
            });
            column.Item().Element(ComposePurchaseItemTable);
        });
    }

    void ComposeComments(IContainer container) {
        container.ShowEntire().Padding(5).Column(column => {
            column.Spacing(5);
            column.Item().Background(Colors.Grey.Lighten3).Text("Comments or Special Instructions").FontSize(14).SemiBold();
            column.Item().Border(1).MinHeight(100).Padding(5).Text("Please order these when you have a chance.  I need these for a project" +
                               "as soon as possible.  Thank you for your help.");
        });
    }

    void ComposePurchaseItemTable(IContainer container) {
        var headerStyle = TextStyle.Default.SemiBold().FontSize(11).FontColor(Colors.White);

        container.Table(table => {
            
            table.ColumnsDefinition(columns => {
                columns.ConstantColumn(30);
                columns.RelativeColumn(4);
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Header(header => {
                
                header.Cell().Background(Colors.Blue.Darken1).AlignCenter().Text("Qty").Style(headerStyle);
                header.Cell().Background(Colors.Blue.Darken1).AlignCenter().Text("Product/Description").Style(headerStyle);
                header.Cell().Background(Colors.Blue.Darken1).AlignCenter().Text("Unit Price").Style(headerStyle);
                header.Cell().Background(Colors.Blue.Darken1).AlignCenter().Text("Total").Style(headerStyle);
                header.Cell().ColumnSpan(4)
                    .BorderBottom(1).BorderTop(1).BorderRight(1)
                    .BorderColor(Colors.Black);
            });
            foreach (var item in Model.Items) {
                table.Cell().Element(CellStyle).AlignCenter().Text($"{item.Quantity}");
                table.Cell().Element(CellStyle).AlignLeft().PaddingLeft(5).Text(item.ProductName);
                table.Cell().Element(CellStyle).AlignCenter().Text($"{item.UnitCost:C}");
                table.Cell().Element(CellStyle).AlignCenter().Text($"{item.UnitCost * item.Quantity:C}");

                static IContainer CellStyle(IContainer container) =>
                    container.BorderBottom(1).BorderTop(1).BorderRight(1).BorderLeft(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
            }
        });
    }
}