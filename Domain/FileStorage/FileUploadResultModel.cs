namespace Domain.FileStorage;

public class FileUploadResultModel {
    public string objectId { get; set; }
    public string? fileName { get; set; } = null!;
    public bool isSuccessful { get; set; } = true;
    public string errorMessage { get; set; } = null!;
}