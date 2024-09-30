using System.Net.Http.Headers;
using System.Text.Json;
using Domain.FileStorage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Radzen;
using FileInfo = Radzen.FileInfo;


namespace Infrastructure.Services;

public record FileUploadResult {
    public string FileName { get; set; }
    public string ObjectId { get; set; }
    public string FullPath { get; set; }
}

public record MultiFileUploadResult {
    public List<string> ObjectIds { get; set; }
    public List<(string name,string path)> LocalFileInfo { get; set; }
}

public class FileService {
    private readonly IHttpClientFactory  _clientFactory;
    private readonly Uri _baseUrl;
    private IWebHostEnvironment _environment;
    
    public FileService(IHttpClientFactory client,IConfiguration configuration, IWebHostEnvironment environment) {
        this._clientFactory = client;
        this._environment = environment;
        this._baseUrl = new Uri(configuration["FileServiceUrl"] ?? "http://localhost:8080/FileStorage/");
    }
    
    public async Task<FileUploadResult?> UploadFile(FileInfo file) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        using var form = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream(1048576000);
        using var streamContent = new StreamContent(stream);
        var filebytes=await streamContent.ReadAsByteArrayAsync();
        using var fileContent = new ByteArrayContent(filebytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
        form.Add(fileContent, "file", file.Name);
        HttpResponseMessage response = await client.PostAsync("UploadFile", form);
        response.EnsureSuccessStatusCode();
        if (response.IsSuccessStatusCode) {
            FileUploadResult output = new FileUploadResult() {
                FileName = file.Name
            };
            var content =await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FileUploadResultModel>(content);
            var path=Path.Combine(@"C:\Users\aelme\Documents\PurchaseRequestData\Downloads\", file.Name);
            if (File.Exists(path)) {
                File.Delete(path);
            }
            await File.WriteAllBytesAsync(path,filebytes);
            output.ObjectId = result?.objectId;
            output.FullPath = path;
            return output;
        } else {
            return null;
        }
    }

    public async Task<MultiFileUploadResult?> UploadMultipleFiles(IList<FileInfo> files) {
        using var form = new MultipartFormDataContent();
        /*if(files.Count==0) return [];
        if (files.Count == 1) {
            var result=await UploadFile(files[0]);
            return result!=null ? [result]:[];
        }*/
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        List<(string name,byte[] fileBytes)> fileBytesList=new();
        MultiFileUploadResult output = new MultiFileUploadResult();
        foreach (var file in files) {
            var stream = file.OpenReadStream(1048576000);
            var streamContent = new StreamContent(stream);
            var filebytes=await streamContent.ReadAsByteArrayAsync();
            fileBytesList.Add((file.Name,filebytes));
            var fileContent = new ByteArrayContent(filebytes);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "files", file.Name);
        }
        var response = await client.PostAsync("UploadMultipleFiles", form);
        response.EnsureSuccessStatusCode();
        response.EnsureSuccessStatusCode();
        if (response.IsSuccessStatusCode) {
            var content =await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<FileUploadResultModel>>(content);
            if(result!=null) {
                output.ObjectIds = result.Select(e=>e?.objectId).ToList();
                output.LocalFileInfo = new();
                foreach (var file in fileBytesList) {
                    //TODO: Replace with container path
                    var path=Path.Combine(@"C:\Users\aelme\Documents\PurchaseRequestData\Downloads\", file.name);
                    Console.WriteLine($"File Path: {path}");
                    if (File.Exists(path)) {
                        File.Delete(path);
                    }
                    await File.WriteAllBytesAsync(path,file.fileBytes);
                    Console.WriteLine($"File: {file.name} written to {path}");
                    output.LocalFileInfo.Add((file.name,path));
                }
                return output;
            } else {
                Console.WriteLine("Error uploading files,Result was null");
                return null;
            }
        } else {
            Console.WriteLine("Status code was not successful");
            return null;
        }
    }

    public async Task DownloadFile(string objectId) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        var response = await client.GetAsync($"DownloadFile?objectId={objectId}");
        response.EnsureSuccessStatusCode();
       var fileName= response.Content.Headers?.ContentDisposition?.FileName;
       if (string.IsNullOrEmpty(fileName)) {
           return;
       }
       var fileInfo = new System.IO.FileInfo(fileName);
       await using var ms = await response.Content.ReadAsStreamAsync();
       //append path here
       await using var fs = File.Create(fileInfo.FullName);
       ms.Seek(0, SeekOrigin.Begin);
       await ms.CopyToAsync(fs);
    }

    public async Task<string?> GetFileName(string objectId) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        var response=await client.GetAsync($"GetFileInfo?objectId={objectId}");
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
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        var fileStream = await client.GetStreamAsync($"DownloadFileStream?objectId={objectId}");
        var path=Path.Combine(@"C:\Users\aelme\Documents\PurchaseRequestData\Downloads", filename);
        await using FileStream outputFileStream = new FileStream(path, FileMode.CreateNew);
        await fileStream.CopyToAsync(outputFileStream);
    }
}