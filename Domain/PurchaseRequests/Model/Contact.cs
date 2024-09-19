using MongoDB.Bson;

namespace Domain.PurchaseRequests.Model;

public enum InternalContactType {
    CompanyMainContact,
    HQ
}

public abstract class Contact {
    public ObjectId _id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Fax { get; set; }
    public string StreetAddress { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Zip { get; set; }
}

public class Vendor : Contact {
    public string Attention { get; set; }
}

public class InternalContact:Contact {
    public InternalContactType Type { get; set; }
}