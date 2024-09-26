using FileStorage.Model;
using FileStorage.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileStorage.Controllers;

public class PathResponse {
    public string Path { get; set; }

    public PathResponse() { }
}

public partial class UploadController : Controller {
    private readonly IWebHostEnvironment _environment;
    private readonly FileStorageService _fileStorageService;
    private readonly ILogger<UploadController> _logger;

    public UploadController(IWebHostEnvironment environment, 
        FileStorageService fileStorageService, 
        ILogger<UploadController> logger) {
        this._environment = environment;
        this._fileStorageService = fileStorageService;
        this._logger = logger;
    }

    // Single file upload
    [HttpPost("upload/single")]
    public async Task<IActionResult> Single(IFormFile file,CancellationToken cancellationToken) {
        var filename = file.FileName;
        var contentType = file.ContentType;
        var stream = file.OpenReadStream();
        var result =
            await _fileStorageService.UploadSmallFileFromStreamAsync(filename, contentType, stream,
                cancellationToken);
        return Ok(result);
    }
    
    // Multiple files upload
    [HttpPost("upload/multiple")]
    public async Task<IActionResult> Multiple(IFormFile[] files, CancellationToken cancellationToken) {
        var filesUploadResult = new List<FileUploadResultModel>();

        foreach (var file in files) {
            var filename = file.FileName;

            var contentType = file.ContentType;
            var result = await _fileStorageService.UploadSmallFileFromStreamAsync(filename, contentType,
                file.OpenReadStream(),
                cancellationToken);
            filesUploadResult.Add(result);
        }

        return Ok(filesUploadResult);
    }

    // Multiple files upload with parameter
    [HttpPost("upload/{id}")]
    public IActionResult Post(IFormFile[] files, int id) {
        try {
            // Put your code here
            return StatusCode(200);
        } catch (Exception ex) {
            return StatusCode(500, ex.Message);
        }
    }

    // Image file upload (used by HtmlEditor components)
    [HttpPost("upload/image")]
    public IActionResult Image(IFormFile file) {
        try {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            using (var stream = new FileStream(Path.Combine(_environment.WebRootPath, fileName), FileMode.Create)) {
                // Save the file
                file.CopyTo(stream);

                // Return the URL of the file
                var url = Url.Content($"~/{fileName}");

                return Ok(new { Url = url });
            }
        } catch (Exception ex) {
            return StatusCode(500, ex.Message);
        }
    }
}