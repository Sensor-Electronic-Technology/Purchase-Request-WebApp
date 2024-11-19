using Microsoft.Extensions.Configuration;
using Ardalis.GuardClauses;
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

public class AppTimeProvider:System.TimeProvider{
    public AppTimeProvider(IConfiguration configuration) {
        if (TimeZoneInfo.TryFindSystemTimeZoneById(configuration["TimeZoneId"] ?? "EST", out var info)) {
            LocalTimeZone = info;
        } else {
            LocalTimeZone = System.LocalTimeZone;
        }
    }
    
    public DateTime Now() {
        return this.GetLocalNow().DateTime;
    }
    
    public int DaysSince(DateTime other) {
        var adjusted=TimeZoneInfo.ConvertTimeFromUtc(other.ToUniversalTime(),
            TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")); 
        return (this.Now()-adjusted).Days;
    } 
    
    public DateTime ToLocal(DateTime other) {
        return TimeZoneInfo.ConvertTimeFromUtc(other.ToUniversalTime(),
            TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")); 
    }
    
    public override TimeZoneInfo LocalTimeZone { get; }
}