using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Services;

public class FileService {
    private HttpClient _httpClient;

    
    public FileService() {
        this._httpClient = new HttpClient();
        this._httpClient.BaseAddress=new Uri("http://localhost:5021/FileStorage/");
    }
    public async Task UploadFile(string path) {
        using var form = new MultipartFormDataContent();
        await using var fs = File.OpenRead(path);
        using var streamContent = new StreamContent(fs);
        using var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
        // "file" parameter name should be the same as the server side input parameter name
        form.Add(fileContent, "file", Path.GetFileName(path));
        HttpResponseMessage response = await this._httpClient.PostAsync("UploadFile", form);
    }
    
    public async Task UploadMultipleFiles(string[] paths) {
        using var form = new MultipartFormDataContent();
        if(paths.Length==0) return;
        if (paths.Length == 1) {
            await UploadFile(paths[0]);
            return;
        }
        foreach (var filePath in paths) {
             var fs = File.OpenRead(filePath);
             var streamContent = new StreamContent(fs);
             var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync());
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            // "file" parameter name should be the same as the server side input parameter name
            form.Add(fileContent, "files", Path.GetFileName(filePath));
        }
        var response = await this._httpClient.PostAsync("UploadMultipleFiles", form);
        response.EnsureSuccessStatusCode();
        Console.WriteLine(response.Headers.ToString());
    }

    public async Task DownloadFile(string objectId) {
        var response = await _httpClient.GetAsync($"DownloadFile?objectId={objectId}");
        response.EnsureSuccessStatusCode();
       var fileName= response.Content.Headers?.ContentDisposition?.FileName;
       if (string.IsNullOrEmpty(fileName)) {
           return;
       }
       var fileInfo = new FileInfo(fileName);
       await using var ms = await response.Content.ReadAsStreamAsync();
       //append path here
       await using var fs = File.Create(fileInfo.FullName);
       ms.Seek(0, SeekOrigin.Begin);
       await ms.CopyToAsync(fs);
    }

    public async Task<string?> GetFileName(string objectId) {
        var response=await this._httpClient.GetAsync($"GetFileInfo?objectId={objectId}");
        response.EnsureSuccessStatusCode();
        var content =await response.Content.ReadAsStringAsync();
        var document = JsonSerializer.Deserialize<JsonDocument>(content);
        if (document != null) {
            if(document.RootElement.TryGetProperty("filename", out var filename)) {
                return filename.GetString();
            }
        }
        return default;
    }

    public async Task DownloadFileStream(string objectId) {
        var filename=await GetFileName(objectId);
        if (string.IsNullOrEmpty(filename)) {
            return;
        }
        var fileStream = await _httpClient.GetStreamAsync($"DownloadFileStream?objectId={objectId}");
        var path=Path.Combine(@"C:\Users\aelme\Documents\PurchaseRequestData\Downloads", filename);
        await using FileStream outputFileStream = new FileStream(path, FileMode.CreateNew);
        await fileStream.CopyToAsync(outputFileStream);
    }
    
    /*public async Task<string> DownloadFile(string objectId)
    {
        if (string.IsNullOrWhiteSpace(objectId)) {
            throw new ArgumentNullException(nameof(objectId), "GUID is empty.");
        }
        //_logger.LogInformation($"Downloading File with GUID=[{guid}].");
        

        var response = await _httpClient.GetAsync($"/DownloadFile?objectId={objectId}");
        response.EnsureSuccessStatusCode();
        MultipartFormDataContent content = new MultipartFormDataContent();
        //response.Content.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
        
        await using var ms = await response.Content.ReadAsStreamAsync();
        //await using var fs = File.Create(fileInfo.FullName);
        ms.Seek(0, SeekOrigin.Begin);
        await ms.CopyToAsync(fs);

        //_logger.LogInformation($"File saved as [{fileInfo.Name}].");
        return fileInfo.FullName;
    }*/
}