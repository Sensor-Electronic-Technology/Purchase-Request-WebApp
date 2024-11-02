namespace Infrastructure.Services;

public static class TimeProvider {
    public static DateTime Now() {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")); 
    }

    public static int DaysSince(DateTime other) {
        var adjusted=TimeZoneInfo.ConvertTimeFromUtc(other.ToUniversalTime(),
            TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")); 
        return (TimeProvider.Now()-adjusted).Days;
    }
    
    public static DateTime ToLocal(DateTime other) {
        return TimeZoneInfo.ConvertTimeFromUtc(other.ToUniversalTime(),
            TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")); 
    }
}