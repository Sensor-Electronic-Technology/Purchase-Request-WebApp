using Domain.PurchaseRequests.Dto;
using Domain.PurchaseRequests.Dto.ActionInputs;
using Domain.PurchaseRequests.Model;
using Domain.PurchaseRequests.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace Domain.PurchaseRequests.Pdf;

public class PurchaseRequestDocument : IDocument {
    private Image _logoImage;
    private PurchaseRequestInput _model;
    private string _logoPath;
    public PurchaseRequestDocument(PurchaseRequestInput model, string logoPath) {
        this._model = model;
        this._logoPath = logoPath;
        this._logoImage = Image.FromFile(this._logoPath);
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
            row.ConstantItem(175).Image(_logoImage);
            row.Spacing(50);
            row.RelativeItem().Column(column => {
                column.Item().Text("Purchase Request")
                    .FontSize(34).Bold().FontColor(Colors.Blue.Darken4);
            });
            
        });
    }
    void ComposeContent(IContainer container) {
        container.PaddingTop(10).Column(column => {
            column.Item().Background(Colors.Grey.Lighten1).PaddingVertical(3);
            column.Item().Table(table => {
                table.ColumnsDefinition(def => {
                    def.ConstantColumn(100);
                    def.RelativeColumn();
                });
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Date Of Request:").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(DateTime.Now.ToString("MM/dd/yyyy"));
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Requester:").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(this._model.RequesterName ?? "");
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Department:").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(this._model.Department?.Name ?? "");
                
            });
            column.Item().Background(Colors.Grey.Lighten1).PaddingVertical(3);
            column.Item().Table(table => {
                table.ColumnsDefinition(def => {
                    def.ConstantColumn(100);
                    def.RelativeColumn();
                });
                
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Vendor Name: ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(this._model.Vendor?.Name ?? "");
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Contact: ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(this._model.Vendor?.Attention ?? "");
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Address: ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(this._model.Vendor?.StreetAddress ?? "");
                table.Cell().Border(1).Padding(2).AlignLeft().Text("City,State,Zip: ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft()
                    .Text($"{this._model.Vendor?.City ?? ""},{this._model.Vendor?.State ?? ""},{this._model.Vendor?.Zip ?? ""}");
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Phone: ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(this._model.Vendor?.Phone ?? "");
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Email: ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(this._model.Vendor?.Email ?? "");
                
            });
            /*column.Item().Background(Colors.Grey.Lighten1).Padding(3);
            column.Item().Table(table => {
                table.ColumnsDefinition(def => {
                    def.ConstantColumn(100);
                    def.RelativeColumn();
                });
                
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Payment Terms:  ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(this._model.PaymentTerms);
            });*/
            column.Item().Background(Colors.Grey.Lighten1).PaddingVertical(3);
            column.Item().Table(table => {
                table.ColumnsDefinition(def => {
                    def.ConstantColumn(100);
                    def.RelativeColumn();
                });
                
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Description:  ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(this._model.Title);
            });
            column.Item().Table(table => {
                table.ColumnsDefinition(def => {
                    def.RelativeColumn();
                });
                
                table.Header(header => {
                    header.Cell().Background(Colors.Grey.Lighten2).Border(1).Padding(2).AlignCenter().Text("Reason For Purchase");
                });
                if (!string.IsNullOrWhiteSpace(this._model.Description)) {
                    table.Cell().Border(1).Padding(2).AlignLeft().Text(this._model.Description).SemiBold();
                } else {
                    table.Cell().Border(1).Padding(2).AlignLeft().Text("").SemiBold();
                    table.Cell().Border(1).Padding(2).AlignLeft().Text("").SemiBold();
                    table.Cell().Border(1).Padding(2).AlignLeft().Text("").SemiBold();
                }
                
            });
            column.Item().Background(Colors.Grey.Lighten1).Padding(3);
            column.Item().Table(table => {
                table.ColumnsDefinition(def => {
                    def.ConstantColumn(100);
                    def.RelativeColumn();
                });
                
                table.Cell().Border(1).Padding(2).AlignLeft().Text("Delivery Method:  ").SemiBold();
                table.Cell().Border(1).Padding(2).PaddingLeft(5).AlignLeft().Text(this._model.ShippingType);
            });
            column.Item().Background(Colors.Grey.Lighten1).PaddingVertical(3);
            column.Item().Element(ComposePurchaseItemTable);
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
                
                header.Cell().Background(Colors.Blue.Lighten1).AlignCenter().Text("Qty").Style(headerStyle);
                header.Cell().Background(Colors.Blue.Lighten1).AlignCenter().Text("Product/Description").Style(headerStyle);
                header.Cell().Background(Colors.Blue.Lighten1).AlignCenter().Text("Unit Price").Style(headerStyle);
                header.Cell().Background(Colors.Blue.Lighten1).AlignCenter().Text("Total").Style(headerStyle);
                header.Cell().ColumnSpan(4)
                    .BorderBottom(1).BorderTop(1).BorderRight(1)
                    .BorderColor(Colors.Black);
            });
            foreach (var item in _model.PurchaseItems) {
                table.Cell().Element(CellStyle).AlignCenter().Text($"{item.Quantity}");
                table.Cell().Element(CellStyle).AlignLeft().PaddingLeft(5).Text(item.ProductName);
                table.Cell().Element(CellStyle).AlignCenter().Text(string.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", item.UnitCost));
                table.Cell().Element(CellStyle).AlignCenter().Text(string.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", item.TotalCost));
                static IContainer CellStyle(IContainer container) =>
                    container.BorderBottom(1).BorderTop(1).BorderRight(1).BorderLeft(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
            }
            
            table.Cell().AlignCenter().Text($"");
            table.Cell().AlignLeft().PaddingLeft(5).Text("");
            table.Cell().Element(TotalCellStyle).AlignCenter().Text($"Total").SemiBold();
            table.Cell().Element(TotalCellStyle).AlignCenter().Text(string.Format(new System.Globalization.CultureInfo("en-US"), "{0:C}", this._model.TotalCost)).SemiBold();
            static IContainer TotalCellStyle(IContainer container) =>
                container.BorderBottom(1).BorderTop(1).BorderRight(1).BorderLeft(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
        });
    }
}