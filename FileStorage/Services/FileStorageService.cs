﻿using System.Security.Cryptography;
using System.Text.Json;
using FileStorage.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.GridFS;

namespace FileStorage.Services;

public class FileStorageService {
    private readonly FileValidationService _validationService;
    private readonly UploadFileSettings _settings;
    private GridFSBucket _fileBucket;
    private IMongoDatabase _database;
    
    public FileStorageService(FileValidationService validationService, 
        UploadFileSettings settings,
        DatabaseSettings dbSettings,
        IMongoClient mongoClient) {
        this._validationService = validationService;
        this._settings = settings;
        this._database = mongoClient.GetDatabase(dbSettings.FileDatabase);
        this._fileBucket = new GridFSBucket(this._database, new GridFSBucketOptions {
            BucketName = dbSettings.FileCollection,
            //    ChunkSizeBytes = 1048576, // 1MB
            //    WriteConcern = WriteConcern.WMajority,
            //    ReadPreference = ReadPreference.Secondary
        });
        //this._database=mongoClient.GetDatabase(settings.DatabaseName);
    }
    
    public string GetMd5HashCode(byte[] bytes) {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    public async Task<GridFSFileInfo?> GetFileInfoById(ObjectId id) {
        var filter=Builders<GridFSFileInfo>.Filter.And(Builders<GridFSFileInfo>.Filter.Eq(x => x.Id, id));
        var cursor = await this._fileBucket.FindAsync(new BsonDocument {{"_id", id}});
        var result = (await cursor.ToListAsync()).FirstOrDefault();
        return result;
    }
    
    public async Task<GridFSFileInfo?> GetFileInfoByMd5HashCode(string md5HashCode) {
        if (string.IsNullOrEmpty(md5HashCode) && md5HashCode.Length != 32) {
            return null;
        }
        var cursor = await this._fileBucket.FindAsync(new BsonDocument {{"md5", md5HashCode}});
        var result = (await cursor.ToListAsync()).FirstOrDefault();
        return result;
    }

    public async Task<FileUploadResultModel> UploadFromBytesAsync(string? fileName, string? contentType,
        byte[] bytes, CancellationToken cancellationToken = default) {
        var result = new FileUploadResultModel() { FileName = fileName };
        if (!this._settings.AllowDuplicateFile) {
            var md5 = this.GetMd5HashCode(bytes);
            var existingFile = await this.GetFileInfoByMd5HashCode(md5);
            if(existingFile!=null) {
                result.ObjectId = existingFile.Id.ToString();
                return result;
            }
        }

        var options = new GridFSUploadOptions() {
            Metadata = new BsonDocument() {
                { "ContentType", contentType }, 
                { "UntrustedFileName", fileName }
            }
        };
        var firstBytes = bytes.Take(64).ToArray();
        var validateResult = this._validationService.ValidateFile(firstBytes, fileName, bytes.Length,
            FileSizeEnum.Small, cancellationToken);
        if(validateResult!=ValidationFileEnum.Ok) {
            result.IsSuccessful = false;
            result.ErrorMessage = this._validationService.GetValidationMessage(validateResult);
            return result;
        }

        var newFileName = Path.GetRandomFileName();
        try {
            var id = await this._fileBucket.UploadFromBytesAsync(newFileName, bytes, options, cancellationToken);
            result.IsSuccessful = true;
            result.ObjectId = id.ToString();
            return result;
        }catch(Exception ex) {
            result.IsSuccessful = false;
            result.ErrorMessage = ex.Message + "" + ex.InnerException;
            return result;
        }
    }
    
    public async Task<FileUploadResultModel> UploadSmallFileFromStreamAsync(string? fileName, string? contentType,
        Stream stream,
        CancellationToken cancellationToken = default) {
        var options = new GridFSUploadOptions {
            Metadata = new BsonDocument { { "ContentType", contentType }, { "UntrustedFileName", fileName } }
        };
        var newFileName = Path.GetRandomFileName();

        var result = await UploadFromStreamAsync(fileName, newFileName, FileSizeEnum.Small, stream, options,
            cancellationToken);
        return result;
    }
    
    public async Task<FileUploadResultModel> UploadLargeFileFromStreamAsync(string? fileName, string? contentType,
        Stream stream,
        CancellationToken cancellationToken = default) {
        var options = new GridFSUploadOptions {
            Metadata = new BsonDocument { { "ContentType", contentType }, { "UntrustedFileName", fileName } }
        };
        var newFileName = Path.GetRandomFileName();
        var result = await UploadFromStreamAsync(fileName, newFileName, FileSizeEnum.Large, stream, options,
            cancellationToken);
        return result;
    }
    

    public async Task<FileUploadResultModel> UploadFromStreamAsync(
        string? fileName,
        string newFileName,
        FileSizeEnum fileSize,
        Stream source,
        GridFSUploadOptions? options = null,
        CancellationToken cancellationToken = default(CancellationToken)) {
        
        Ensure.IsNotNull<string>(fileName, nameof(fileName));
        Ensure.IsNotNull<string>(newFileName, nameof(newFileName));
        Ensure.IsNotNull<Stream>(source, nameof(source));
        MD5 hasher = MD5.Create();
        hasher.Initialize();
        var result = new FileUploadResultModel() { FileName = fileName };
        var whiteListResult = _validationService.ValidateFileWhiteList(fileName);
        if (whiteListResult != ValidationFileEnum.Ok) {
            result.IsSuccessful = false;
            result.ErrorMessage = _validationService.GetValidationMessage(whiteListResult);
            return result;
        }

        options ??= new GridFSUploadOptions();
        
        var chunkSizeBytes = options.ChunkSizeBytes ?? 261120; // 255KB

        var id = ObjectId.GenerateNewId();
        await using GridFSUploadStream<ObjectId> destination = await this._fileBucket
            .OpenUploadStreamAsync(id, newFileName, options, cancellationToken).ConfigureAwait(false);
        var buffer = new byte[chunkSizeBytes];
        var isFilledFirstBytes = false;
        long lengthOfFile = 0;
        Exception? sourceException;

        while (true) {
            var bytesRead = 0;
            sourceException = (Exception)null;
            try {
                bytesRead=await source.ReadAsync(buffer,0,buffer.Length,cancellationToken)
                    .ConfigureAwait(false);
            } catch (Exception? ex) {
                sourceException = ex;
            }

            if (sourceException == null) {
                if (bytesRead != 0) {
                    hasher.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                    if (!isFilledFirstBytes) {
                        var firstBytes = buffer.Take(64).ToArray();
                        var fileExtension = Path.GetExtension(fileName);
                        var signatureResult = this._validationService.ValidateFileSignature(firstBytes, fileExtension);
                        if(signatureResult!=ValidationFileEnum.Ok) {
                            try {
                                await destination.AbortAsync(cancellationToken).ConfigureAwait(false);
                            } catch { } finally {
                                await destination.CloseAsync(cancellationToken).ConfigureAwait(false);
                                await this.DeleteFileAsync(destination.Id, cancellationToken);
                            }

                            buffer = (byte[])null;
                            result.IsSuccessful = false;
                            result.ErrorMessage = this._validationService.GetValidationMessage(signatureResult);
                            return result;
                        }
                        isFilledFirstBytes = true;
                    }
                    lengthOfFile += bytesRead;
                    var fileLengthResult = this._validationService.ValidateFileMaxLength(lengthOfFile, fileSize);
                    if (fileLengthResult != ValidationFileEnum.Ok) {
                        try {
                            await destination.AbortAsync(cancellationToken).ConfigureAwait(false);
                        } catch { } finally {
                            await destination.CloseAsync(cancellationToken).ConfigureAwait(false);
                            await this.DeleteFileAsync(destination.Id, cancellationToken);
                        }

                        buffer = (byte[])null;
                        result.IsSuccessful = false;
                        result.ErrorMessage = _validationService.GetValidationMessage(fileLengthResult);
                        return result;
                    }
                    await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                } else {
                    hasher.TransformFinalBlock(new byte[0], 0, 0);
                    string md5hashCode = BitConverter.ToString(hasher.Hash).Replace("-", "").ToLowerInvariant();

                    if (!this._settings.AllowDuplicateFile) {
                        var existedFile = await GetFileInfoByMd5HashCode(md5hashCode);
                        if (existedFile != null) {
                            try {
                                await destination.AbortAsync(cancellationToken).ConfigureAwait(false);
                            } catch { } finally {
                                await destination.CloseAsync(cancellationToken).ConfigureAwait(false);
                            }

                            result.ObjectId = existedFile.Id.ToString();
                            return result;
                        }
                    }
                    
                    var fileLengthResult = _validationService.ValidateFileMinLength(lengthOfFile, fileSize);
                    if (fileLengthResult != ValidationFileEnum.Ok) {
                        try {
                            await destination.AbortAsync(cancellationToken).ConfigureAwait(false);
                        } catch { } finally {
                            await destination.CloseAsync(cancellationToken).ConfigureAwait(false);
                        }

                        result.IsSuccessful = false;
                        result.ErrorMessage = _validationService.GetValidationMessage(fileLengthResult);
                        return result;
                    }

                    await destination.CloseAsync(cancellationToken).ConfigureAwait(false);
                    buffer = (byte[])null;

                    result.ObjectId = destination.Id.ToString();
                    return result;
                }
                
            } else {
                try {
                    await destination.AbortAsync(cancellationToken).ConfigureAwait(false);
                } catch { } finally {
                    await destination.CloseAsync(cancellationToken).ConfigureAwait(false);
                }

                break;
            }
        }
        await destination.CloseAsync(cancellationToken).ConfigureAwait(false);
        buffer = (byte[])null;

        result.IsSuccessful = false;
        result.ErrorMessage = sourceException.Message + "_" + sourceException.InnerException;
        return result;
    }
    
    public async Task<FileDeleteResult> DeleteFileAsync(ObjectId objectId,
        CancellationToken cancellationToken=default) {
        if (await this.DeleteChunkAsync(objectId)) {
            await this._fileBucket.DeleteAsync(objectId, cancellationToken);
            return new FileDeleteResult() { Success = true };
        }
        return new FileDeleteResult(){Success = false, Message = "Failed to delete file chunks, Deletion aborted"};
    }

    private async Task<bool> DeleteChunkAsync(ObjectId objectId) {
        FilterDefinition<BsonDocument> filter = new BsonDocument("files_id", objectId);
        var chunksCollection =
            this._database.GetCollection<BsonDocument>(this._fileBucket.Options.BucketName + ".chunks");
        var deleteResult = await chunksCollection.DeleteManyAsync(filter);
        if (deleteResult.IsAcknowledged) {
            return deleteResult.DeletedCount > 0;
        }
        return false;
    }
    
    public async Task<byte[]> DownloadAsBytesAsync(ObjectId objectId,
        CancellationToken cancellationToken = default) {
        var options = new GridFSDownloadOptions { Seekable = true, };
        var bytes = await this._fileBucket.DownloadAsBytesAsync(objectId, options, cancellationToken);
        return bytes;
    }

    public async Task DownloadToStreamAsync(ObjectId objectId, Stream destination,
        CancellationToken cancellationToken = default) {
        await this._fileBucket.DownloadToStreamAsync(objectId, destination, null, cancellationToken);
    }
    
}