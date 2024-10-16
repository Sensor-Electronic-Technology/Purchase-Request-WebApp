namespace Infrastructure.Services;

public static class TimeProvider {
    public static DateTime Now() {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")); 
    }
}