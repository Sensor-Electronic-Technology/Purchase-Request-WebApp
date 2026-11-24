using MongoDB.Bson;

namespace Domain.Settings;

public class RemindConfig {
    public ObjectId _id { get; set; }
    public double ApprovalTimeHrs { get; set; } = 24;
    public double ApprovalTimeRepeatHrs { get; set; } = 24;
    public double OrderTimeHrs { get; set; } = 24;
    public double OrderTimeRepeatHrs { get; set; } = 24;
    public double CheckInTimeDays { get; set; } = 14;
    public double CheckInTimeRepeatDays { get; set; } = 7;
    
    public bool Equals(RemindConfig other) {
        return this.ApprovalTimeHrs == other.ApprovalTimeHrs
               && this.ApprovalTimeRepeatHrs == other.ApprovalTimeRepeatHrs
               && this.OrderTimeHrs == other.OrderTimeHrs
               && this.OrderTimeRepeatHrs == other.OrderTimeRepeatHrs
               && this.CheckInTimeDays == other.CheckInTimeDays
               && this.CheckInTimeRepeatDays == other.CheckInTimeRepeatDays;
    }

    public RemindConfig Clone() {
        return (RemindConfig)MemberwiseClone();
    }
}