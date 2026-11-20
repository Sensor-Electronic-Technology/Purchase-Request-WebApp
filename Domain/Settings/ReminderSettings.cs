namespace Domain.Settings;

public class ReminderSettings {
    public const string SectionName = "ReminderSettings";
    public double ApprovalTimeMins { get; set; } = 24 * 60;
    public double ApprovalTimeRepeatMins { get; set; } = 24 * 60;
    public double OrderTimeMins { get; set; } = 24 * 60;
    public double OrderTimeRepeatMins { get; set; } = 24 * 60;
    public double CheckInTimeMins { get; set; } = 24 * 60 * 14;
    public double CheckInTimeRepeatMins { get; set; } = 24 * 60 * 7;
}