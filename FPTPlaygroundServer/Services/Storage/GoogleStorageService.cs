using FPTPlaygroundServer.Common.Settings;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

namespace FPTPlaygroundServer.Services.Storage;

public class GoogleStorageService(IOptions<GoogleStorageSettings> googleStorageSettings, IHostEnvironment env)
{
    private readonly GoogleStorageSettings _googleStorageSettings = googleStorageSettings.Value;
    private readonly StorageClient _storageClient = StorageClient.Create();

    public async Task<string> UploadFileToCloudStorage(IFormFile file, string fileName)
    {
        string folderAndFileName = $"{env.EnvironmentName}/{fileName}.{Path.GetExtension(file.FileName)[1..]}";

        await _storageClient.UploadObjectAsync(
                   _googleStorageSettings.Bucket,
                   folderAndFileName,
                   file.ContentType,
                   file.OpenReadStream());

        string publicUrl = $"https://storage.googleapis.com/{_googleStorageSettings.Bucket}/{folderAndFileName}";

        return publicUrl;
    }

    public async Task DeleteFileFromCloudStorage(string publicUrl)
    {
        string bucketName = _googleStorageSettings.Bucket;
        string objectName = publicUrl.Replace($"https://storage.googleapis.com/{bucketName}/", "");

        await _storageClient.DeleteObjectAsync(bucketName, objectName);
    }
}
