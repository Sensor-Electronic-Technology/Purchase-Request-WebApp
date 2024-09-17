using MongoDB.Bson;

namespace Domain.PurchaseRequests.Model;

public class Vendor {
    public ObjectId _id { get; set; }
    public string Name { get; set; }
    public string StreetAddress { get; set; }
    public string CityStateZip { get; set; }
    public string Contact { get; set; }
    public string Phone { get; set; }
    public string Fax { get; set; }
    public string Email { get; set; }

    
}