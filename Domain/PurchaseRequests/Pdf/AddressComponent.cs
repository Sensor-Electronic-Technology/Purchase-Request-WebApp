using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Domain.PurchaseRequests.Pdf;

public class AddressComponent : IComponent {
    private Address VendorAddress { get; }
    private Address ToAddress { get; }
    private string _attention;
    public AddressComponent(Address vendorAddress,Address toAddress,string attention) {
        this.VendorAddress = vendorAddress;
        this.ToAddress = toAddress;
        this._attention=attention;
    }

    public void Compose(IContainer container) {
        container.Table(table => {
            var headerStyle = TextStyle.Default.SemiBold().FontSize(9);
            table.ColumnsDefinition(columns => {
                columns.RelativeColumn();
                columns.RelativeColumn();
            });
                
            table.Header(header => {
                header.Cell().Background(Colors.Blue.Darken1).AlignLeft().PaddingLeft(5).Text("Vendor").FontColor(Colors.White);
                header.Cell().Background(Colors.Blue.Darken1).AlignLeft().Text("Ship To").FontColor(Colors.White);
                /*header.Cell().ColumnSpan(2).PaddingTop(5).BorderBottom(1).BorderTop(1).BorderRight(1).BorderColor(Colors.Black);*/
            });       
            
            table.Cell().ShowEntire().Column(column => {
                column.Item().PaddingLeft(2).Text(VendorAddress.CompanyName);
                column.Item().PaddingLeft(2).Text(VendorAddress.Street);
                column.Item().PaddingLeft(2).Text(VendorAddress.CityStateZip);
                column.Item().PaddingLeft(2).Text(VendorAddress.Email);
                column.Item().PaddingLeft(2).Text(VendorAddress.Phone);
            });
            
            table.Cell().ShowEntire().Column(column => {
                column.Item().PaddingLeft(2).Text(this._attention);
                column.Item().Text(ToAddress.CompanyName);
                column.Item().Text(ToAddress.Street);
                column.Item().Text(ToAddress.CityStateZip);
                column.Item().Text(ToAddress.Email);
                column.Item().Text(ToAddress.Phone);
            });
        });
    }
}