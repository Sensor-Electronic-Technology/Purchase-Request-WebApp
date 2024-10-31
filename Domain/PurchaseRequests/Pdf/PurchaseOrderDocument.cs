using Domain.PurchaseRequests.Dto;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Domain.PurchaseRequests.Pdf;

public class PurchaseOrderDocument : IDocument {
    private Image _logoImage;
    private PurchaseOrderDto _model;
    private string _logoPath;
    public PurchaseOrderDocument(PurchaseOrderDto model,string logoPath) {
        this._model = model;
        this._logoPath = logoPath;
        this._logoImage = Image.FromFile(this._logoPath);
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
            row.ConstantItem(175).Image(_logoImage);
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
                    column.Item().PaddingLeft(2).Text($"{this._model.ToAddress?.StreetAddress ?? "110 Atlas Ct"}," +
                                                      $" {this._model.ToAddress?.City ?? "Columbia"}," +
                                                      $" {this._model.ToAddress?.State ?? "SC"} " +
                                                      $" {this._model.ToAddress?.Zip ?? "29209"}");
                    column.Item().PaddingLeft(2).Text($"Voice: {this._model.ToAddress?.Phone ?? "(803) 647-9757"}");
                    column.Item().PaddingLeft(2).Text($"Website: www.s-et.com");
                    column.Item().PaddingLeft(2).Text($"Email: {this._model.ToAddress?.Email ?? "ap@s-et.com"}");
                });

                row.RelativeItem().ShowEntire().Column(column => {
                    column.Item().PaddingBottom(10).AlignLeft().Text($"Date: {this._model.Date:d}").SemiBold();
                    column.Item().AlignLeft().Text($"PO Number: {this._model.PoNumber}").SemiBold();
                });
            });
        });
    }

    void ComposeContent(IContainer container) {
        container.PaddingTop(10).Column(column => {
            column.Item().Border(1).Padding(2).Element(ComposeOrderHeader);
            
            column.Item().Border(1).Background(Colors.Grey.Lighten2).Row(row => {
                /*CityStateZip = $"{this._model.Vendor.City ?? ""}, {this._model.Vendor.State ?? ""} {this._model.Vendor.Zip ?? ""}",*/
                var address = new Address() {
                    CityStateZip = $"{this._model.Vendor?.City ?? ""}, {this._model.Vendor?.State ?? ""} {this._model.Vendor?.Zip ?? ""}",
                    CompanyName = _model.Vendor?.Name ?? "",
                    Email = _model.Vendor?.Email ?? "",
                    Phone = _model.Vendor?.Phone ?? "",
                    Street = _model.Vendor?.StreetAddress ?? "",
                };
                if (this._model.ToAddress != null) {
                    var toAddress = new Address() {
                        CityStateZip = $"{this._model.ToAddress?.City ?? ""}, {this._model.ToAddress?.State ?? ""} {this._model.ToAddress?.Zip ?? ""}",
                        CompanyName = _model.ToAddress?.Name ?? "",
                        Email = _model.ToAddress?.Email ?? "",
                        Phone = _model.ToAddress?.Phone ?? "",
                        Street = _model.ToAddress?.StreetAddress ?? "",
                    };
                    row.RelativeItem().PaddingBottom(2).Component(new AddressComponent(address,toAddress,this._model.ShipTo));
                } else {
                    var toAddress = new Address() {
                        CityStateZip = "Columbia, SC 29201",
                        CompanyName = "Sensor Electronic Technology, Inc.",
                        Email = "ap@s-et.com",
                        Phone = "(803) 647-9757",
                        Street = "110 Atlas Court",
                    };
                    row.RelativeItem().PaddingBottom(2).Component(new AddressComponent(address,toAddress,this._model.ShipTo));
                }
            });
            
            column.Item().Element(ComposeOrderInfoTable);
            column.Item().Element(ComposePurchaseItemTable);
            
            /*var totalPrice = _model.Items.Sum(x => x.UnitCost * x.Quantity);
        column.Item().PaddingRight(5).AlignRight().Text($"Total: {totalPrice:C}").SemiBold();*/
            column.Item().PaddingTop(10).Element(ComposeComments);
        });
    }

    void ComposeComments(IContainer container) {
        container.ShowEntire().Padding(5).Column(column => {
            column.Spacing(5);
            column.Item().Background(Colors.Grey.Lighten3).Text("Comments or Special Instructions").FontSize(14).SemiBold();
            column.Item().Border(1).MinHeight(100).Padding(5).Text(this._model.Comments);
        });
    }

    void ComposeFooter(IContainer container) {
        container.Column(col => {
            col.Item().Text("If you have any questions about this purchase order, please contact").SemiBold()
                .FontSize(16)
                .AlignCenter();
            col.Item().Text("Sharon Pace, (803) 647-9757 Ext. 154, ap@s-et.com").AlignCenter();
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

            table.Cell().Element(CellStyle).AlignCenter().Text(this._model.Requester?.Name ?? "");
            table.Cell().Element(CellStyle).AlignLeft().PaddingLeft(5).Text(this._model.ShippingMethod);
            table.Cell().Element(CellStyle).AlignCenter().Text(this._model.PaymentTerms);
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
            foreach (var item in _model.Items) {
                table.Cell().Element(CellStyle).AlignCenter().Text($"{item.Quantity}");
                table.Cell().Element(CellStyle).AlignLeft().PaddingLeft(5).Text(item.ProductName);
                /*table.Cell().Element(CellStyle).AlignCenter().Text($"{item.UnitCost:C}");*/
                table.Cell().Element(CellStyle).AlignCenter().Text(string.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", item.UnitCost));
                table.Cell().Element(CellStyle).AlignCenter().Text(string.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", item.TotalCost));
                /*table.Cell().Element(CellStyle).AlignCenter().Text($"{item.UnitCost * item.Quantity:C}");*/
                static IContainer CellStyle(IContainer container) =>
                    container.BorderBottom(1).BorderTop(1).BorderRight(1).BorderLeft(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
            }
            var totalPrice = _model.Items.Sum(x => x.UnitCost * x.Quantity);
            table.Cell().AlignCenter().Text($"");
            table.Cell().AlignLeft().PaddingLeft(5).Text("");
            table.Cell().Element(TotalCellStyle).AlignCenter().Text($"Total");
            table.Cell().Element(TotalCellStyle).AlignCenter().Text(string.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", this._model.TotalCost));
            
            static IContainer TotalCellStyle(IContainer container) =>
                container.BorderBottom(1).BorderTop(1).BorderRight(1).BorderLeft(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
        });
    }
}