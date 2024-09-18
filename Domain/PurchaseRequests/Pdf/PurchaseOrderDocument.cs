using Domain.PurchaseRequests.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class PurchaseOrderDocument : IDocument {
    public static Image LogoImage { get; } = Image.FromFile("C:\\Users\\aelme\\RiderProjects\\Purchase-Request-WebApp\\ConsoleTesting\\seti_logo.png");
    public PurchaseOrderDto Model { get; }
    public PurchaseOrderDocument(PurchaseOrderDto model) {
        Model = model;
    }
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container) {
        container
            .Page(page => {
                page.Margin(50);
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }

    void ComposeHeader(IContainer container) {
        container.Row(row => {
            row.ConstantItem(175).Image(LogoImage);
            row.Spacing(50);
            row.RelativeItem().Column(column => {
                column.Item().Text("Purchase Order").FontSize(36).Bold().FontColor(Colors.Blue.Darken4);
            });
            
        });
    }

    void ComposeOrderHeader(IContainer container) {
        container.Column(column=> {
            column.Item().Background(Colors.Grey.Darken1);
            column.Item().PaddingBottom(5).Row(row => {
                row.RelativeItem().ShowEntire().Column(column => {
                    column.Item().PaddingLeft(2).Text("110 Atlas Ct, Columbia, SC 29209");
                    column.Item().PaddingLeft(2).Text("Voice: (803) 647-9757");
                    column.Item().PaddingLeft(2).Text("Fax: (803) 647-9772");
                    column.Item().PaddingLeft(2).Text($"Website: www.s-et.com");
                    column.Item().PaddingLeft(2).Text("Email:ap@s-et.com");
                });

                row.RelativeItem().ShowEntire().Column(column => {
                    column.Item().PaddingBottom(10).AlignLeft().Text("Date: 09/07/2024").SemiBold();
                    column.Item().AlignLeft().Text("PO Number: 2024-AE-0569").SemiBold();
                });
            });
         });
    }

    void ComposeContent(IContainer container) {
        container.PaddingTop(10).Column(column => {
            column.Item().Border(1).Padding(2).Element(ComposeOrderHeader);
            
            column.Item().Border(1).Background(Colors.Grey.Lighten2).Row(row => {
                var address = new Address() {
                    CityStateZip = Model.Vendor.CityStateZip,
                    CompanyName = Model.Vendor.Name,
                    Email = Model.Vendor.Email,
                    Phone = Model.Vendor.Phone,
                    Street = Model.Vendor.StreetAddress
                };
                var toAddress = new Address() {
                    CityStateZip = "Columbia, SC 29201",
                    CompanyName = "Sensor Electronic Technology, Inc.",
                    Email = "ap@s-et.com",
                    Phone = "(803) 647-9757",
                    Street = "110 Atlas Court",
                };
                row.RelativeItem().PaddingBottom(2).Component(new AddressComponent(address,toAddress));
            });
            
            column.Item().Element(ComposeOrderInfoTable);
            column.Item().Element(ComposePurchaseItemTable);
            
            var totalPrice = Model.Items.Sum(x => x.UnitCost * x.Quantity);
            column.Item().PaddingRight(5).AlignRight().Text($"Total: {totalPrice:C}").SemiBold();
            column.Item().PaddingTop(10).Element(ComposeComments);
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

    void ComposeFooter(IContainer container) {
        container.Column(col => {
            col.Item().Text("If you have any questions about this purchase order, please contact").SemiBold()
                .FontSize(16)
                .AlignCenter();
            col.Item().Text("Sharon Pace, (803) 647-9757 Ext. 154, ap@s-et.com").AlignCenter();
            col.Item().Text("Page 1/1").AlignCenter();
            /*row.ConstantItem(100).Text("Page 1/1").AlignRight();*/
        });
    }

    void ComposeOrderInfoTable(IContainer container) {
        container.Table(table => {
            var headerStyle = TextStyle.Default.SemiBold().FontSize(11).FontColor(Colors.White);
            table.ColumnsDefinition(columns => {
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Header(header => {
                header.Cell().Background(Colors.Blue.Darken1).AlignCenter().Text("Requester").Style(headerStyle);
                header.Cell().Background(Colors.Blue.Darken1).AlignCenter().Text("Ship Via").Style(headerStyle);
                header.Cell().Background(Colors.Blue.Darken1).AlignCenter().Text("Payment Terms").Style(headerStyle);
                header.Cell().ColumnSpan(3)
                    .BorderBottom(1).BorderTop(1).BorderRight(1)
                    .BorderColor(Colors.Black);
            });

            table.Cell().Element(CellStyle).AlignCenter().Text($"Andrew Elmendorf");
            table.Cell().Element(CellStyle).AlignLeft().PaddingLeft(5).Text("Ground");
            table.Cell().Element(CellStyle).AlignCenter().Text($"Net 30 Days");
            static IContainer CellStyle(IContainer container) =>
                container.BorderBottom(1).BorderTop(1).BorderRight(1).BorderLeft(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
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
            var index = 1;
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