namespace Domain.PurchaseRequests.Dto;

public class RequestMailMerge {
    public string PrAction { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Requester { get; set; }
    public string Approver { get; set; }
    public string PrLink { get; set; }
    public string AdditionalComments { get; set; }
}

public class ApproveMailMerge {
    public string PrAction { get; set; }
    public string Title { get; set; }
    public string Requester { get; set; }
    public string Approver { get; set; }
    public string PrLink { get; set; }
    public string CommentsTitle { get; set; }
    public string ApprovalComments { get; set; }
}

public class ReceiveMailMerge {
    public string PrAction { get; set; }
    public string Title { get; set; }
    public string Requester { get; set; }
    public string Receiver { get; set; }
    
    public string Item1 { get; set; }= "";
    public string Item2 { get; set; }= "";
    public string Item3 { get; set; } = "";
    public string Item4 { get; set; } = "";
    public string Item5 { get; set; } = "";
    public string Item6 { get; set; }= "";
    public string Item7 { get; set; }= "";
    public string Item8 { get; set; }= "";
    public string Item9 { get; set; }= "";
    public string Item10 { get; set; }= "";
    
    public string Item1L { get; set; }= "";
    public string Item2L { get; set; }= "";
    public string Item3L { get; set; }= "";
    public string Item4L { get; set; }= "";
    public string Item5L { get; set; }= "";
    public string Item6L { get; set; }= "";
    public string Item7L { get; set; }= "";
    public string Item8L { get; set; }= "";
    public string Item9L { get; set; }= "";
    public string Item10L { get; set; }= "";
    
    public string Item1R { get; set; }= "";
    public string Item2R { get; set; }= "";
    public string Item3R { get; set; }= "";
    public string Item4R { get; set; }= "";
    public string Item5R { get; set; }= "";
    public string Item6R { get; set; }= "";
    public string Item7R { get; set; }= "";
    public string Item8R { get; set; }= "";
    public string Item9R { get; set; }= "";
    public string Item10R { get; set; }= "";
    
    public string ReceiveComments { get; set; }

    public string this[int index] {
        get=> index switch {
            0 => $"{Item1}, {Item1L}, {Item1R}",
            1 => $"{Item2}, {Item2L}, {Item2R}",
            2 => $"{Item3}, {Item3L}, {Item3R}",
            3 => $"{Item4}, {Item4L}, {Item4R}",
            4 => $"{Item5}, {Item5L}, {Item5R}",
            5 => $"{Item6}, {Item6L}, {Item6R}",
            6 => $"{Item7}, {Item7L}, {Item7R}",
            7 => $"{Item8}, {Item8L}, {Item8R}",
            8 => $"{Item9}, {Item9L}, {Item9R}",
            9 => $"{Item10}, {Item10L}, {Item10R}",
            _ => "Error"
        };
    }

    public void SetItem(int index, string name, string loc, bool received) {
        switch (index) {
            case 0:
                Item1 = name;
                Item1L = loc;
                Item1R = received ? "Received" : "Not Received";
                break;
            case 1:
                Item2 = name;
                Item2L = loc;
                Item2R = received ? "Received" : "Not Received";
                break;
            case 2:
                Item3 = name;
                Item3L = loc;
                Item3R = received ? "Received" : "Not Received";
                break;
            case 3:
                Item4 = name;
                Item4L = loc;
                Item4R = received ? "Received" : "Not Received";
                break;
            case 4:
                Item5 = name;
                Item5L = loc;
                Item5R = received ? "Received" : "Not Received";
                break;
            case 5:
                Item6 = name;
                Item6L = loc;
                Item6R = received ? "Received" : "Not Received";
                break;
            case 6:
                Item7 = name;
                Item7L = loc;
                Item7R = received ? "Received" : "Not Received";
                break;
            case 7:
                Item8 = name;
                Item8L = loc;
                Item8R = received ? "Received" : "Not Received";
                break;
            case 8:
                Item9 = name;
                Item9L = loc;
                Item9R = received ? "Received" : "Not Received";
                break;
            case 9:
                Item10 = name;
                Item10L = loc;
                Item10R = received ? "Received" : "Not Received";
                break;
            default:
                break;
        }
    }
    
    
}

