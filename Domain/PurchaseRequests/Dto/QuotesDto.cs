namespace Domain.PurchaseRequests.Dto;

public class QuotesDto {
    public string? Username { get; set; }
    public string? PrTitle { get; set; }
    public string? PrDescription { get; set; }
    public string FileId { get; set; } = null!;
    public string? Filename { get; set; }
    public string? Url { get; set; }
}