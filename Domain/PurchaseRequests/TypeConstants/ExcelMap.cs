using Ardalis.SmartEnum;

namespace Domain.PurchaseRequests.TypeConstants;

public class ExcelMap:SmartEnum<ExcelMap, string>{
    public static readonly ExcelMap RequestDate = new(nameof(RequestDate), "B3",3,2);
    public static readonly ExcelMap Requester = new(nameof(Requester), "B5",5,2);
    public static readonly ExcelMap Department = new(nameof(Department), "B7",7,2);
    public static readonly ExcelMap VendorName = new(nameof(VendorName), "B11",11,2);
    public static readonly ExcelMap Contact = new(nameof(Contact), "B12",12,2);
    public static readonly ExcelMap StreetAddress = new(nameof(StreetAddress), "B13",13,2);
    public static readonly ExcelMap CityStateZip = new(nameof(CityStateZip), "B14",14,2);
    public static readonly ExcelMap Phone = new(nameof(Phone), "B15",15,2);
    /*public static readonly ExcelMap Fax = new(nameof(Fax), "B15",16,2,true,nameof(Phone));*/
    public static readonly ExcelMap Email = new(nameof(Email), "B16",16,2);
    public static readonly ExcelMap PaymentTerms = new(nameof(PaymentTerms), "B18",18,2);
    public static readonly ExcelMap Description = new(nameof(Description), "B20",20,1);
    
    
    public static readonly ExcelMap PurchaseReason = new(nameof(PurchaseReason), "A22",21,2);
    public static readonly ExcelMap DeliveryMethod = new(nameof(DeliveryMethod), "B25",25,6);
    public static readonly ExcelMap  QuantityLines= new(nameof(QuantityLines), "A29",29,1,37,1,true,9,"A37");
    public static readonly ExcelMap  ProductDescription= new(nameof(ProductDescription), "B29",29,2,37,2,true,9,"B37");
    public static readonly ExcelMap  UnitCost= new(nameof(UnitCost), "C29",29,5,37,5,true,9,"C37");
    public static readonly ExcelMap  ProductTotal= new(nameof(QuantityLines), "D29",29,6,37,6,true,9,"D37");
    /*public static readonly ExcelMap  Approver= new(nameof(Approver), "F38");
    public static readonly ExcelMap  DateApproved= new(nameof(DateApproved), "F38");*/

    public bool IsMultiLine { get; set; } = false;
    public int RowCount { get; set; } = 1;
    public string? EndingAddress { get; set; }=default;
    public bool IsChild { get; set; } = false;
    public string? ParentName { get; set; } = default;
    
    public int Row{ get; set; } = 0;
    public int Column { get; set; } = 0;
    
    public int EndRow { get; set; } = 0;
    public int EndCol { get; set; } = 0;
    
    public ExcelMap(string name, string value,int row,int column,int endRow,int endCol,bool isMultiLine,int rowCount,string? endAddress) : base(name, value) {
        this.IsMultiLine = isMultiLine;
        this.RowCount = rowCount;
        this.EndingAddress = endAddress;
        this.IsChild = false;
        this.ParentName = null;
        this.Row = row;
        this.Column = column;
        this.EndRow = endRow;
        this.EndCol = endCol;
    }
    
    public ExcelMap(string name, string value,int row,int column) : base(name, value) {
        this.IsMultiLine = false;
        this.RowCount = 1;
        this.EndingAddress = null;
        this.IsChild = false;
        this.ParentName = null;
        this.Row = row;
        this.Column = column;
        this.EndRow = row;
        this.EndCol = column;
    }

}

/*
public class ExcelMap:SmartEnum<ExcelMap, string>{
    public static readonly ExcelMap RequestDate = new(nameof(RequestDate), "B3");
    public static readonly ExcelMap Requester = new(nameof(Requester), "B5");
    public static readonly ExcelMap Department = new(nameof(Department), "B7");
    public static readonly ExcelMap VendorName = new(nameof(VendorName), "B11");
    public static readonly ExcelMap Contact = new(nameof(Contact), "B12");
    public static readonly ExcelMap StreetAddress = new(nameof(StreetAddress), "B13");
    public static readonly ExcelMap CityStateZip = new(nameof(CityStateZip), "B14");
    public static readonly ExcelMap Phone = new(nameof(Phone), "B15");
    public static readonly ExcelMap Fax = new(nameof(Fax), "B15",true,nameof(Phone));
    public static readonly ExcelMap Email = new(nameof(Email), "B16");
    public static readonly ExcelMap PaymentTerms = new(nameof(PaymentTerms), "B18");
    public static readonly ExcelMap Description = new(nameof(Description), "B20");
    
    
    public static readonly ExcelMap PurchaseReason = new(nameof(PurchaseReason), "A22");
    public static readonly ExcelMap DeliveryMethod = new(nameof(DeliveryMethod), "B25");
    public static readonly ExcelMap  QuantityLines= new(nameof(QuantityLines), "A29",true,9,"A37");
    public static readonly ExcelMap  ProductDescription= new(nameof(ProductDescription), "B29",true,9,"B37");
    public static readonly ExcelMap  UnitCost= new(nameof(UnitCost), "C29",true,9,"C37");
    public static readonly ExcelMap  ProductTotal= new(nameof(QuantityLines), "D29",true,9,"D37");
    public static readonly ExcelMap  TotalCost= new(nameof(QuantityLines), "F38");
    public static readonly ExcelMap  Approver= new(nameof(Approver), "F38");
    public static readonly ExcelMap  DateApproved= new(nameof(DateApproved), "F38");
    
    
    
    /*public static readonly ExcelMap RequestDate = new(nameof(RequestDate), "B2");
    
    public static readonly ExcelMap RequestDate = new(nameof(RequestDate), "B2");
    
    public static readonly ExcelMap RequestDate = new(nameof(RequestDate), "B2");
    
    public static readonly ExcelMap RequestDate = new(nameof(RequestDate), "B2");
    
    public static readonly ExcelMap RequestDate = new(nameof(RequestDate), "B2");#1#
    public bool IsMultiLine { get; set; } = false;
    public int RowCount { get; set; } = 1;
    public string? EndingAddress { get; set; }
    public bool IsChild { get; set; } = false;
    public string? ParentName { get; set; } = "";
    public ExcelMap(string name, string value, bool isMultiLine,int rowCount,string? endAddress) : base(name, value) {
        this.IsMultiLine = isMultiLine;
        this.RowCount = rowCount;
        this.EndingAddress = endAddress;
        this.IsChild = false;
        this.ParentName = null;
    }
    
    public ExcelMap(string name, string value, bool isChild=false,string? parentName=default) : base(name, value) {
        this.IsMultiLine = false;
        this.RowCount = 1;
        this.EndingAddress = null;
        this.IsChild = isChild;
        this.ParentName = parentName;
    }
    
    public ExcelMap(string name, string value) : base(name, value) {
        this.IsMultiLine = false;
        this.RowCount = 1;
        this.EndingAddress = null;
        this.IsChild = false;
        this.ParentName = null;
    }

}*/