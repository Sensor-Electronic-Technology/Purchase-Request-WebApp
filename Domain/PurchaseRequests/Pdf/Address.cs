﻿namespace Domain.PurchaseRequests.Pdf;

public class Address {
    public string CompanyName { get; set; }
    public string Street { get; set; }
    public string CityStateZip { get; set; }
    public object Email { get; set; }
    public string Phone { get; set; }
}