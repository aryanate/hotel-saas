using HotelSaaS.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HotelSaaS.Infrastructure.Services;

public class LocalStorageService : IStorageService
{
    private readonly string _storageRoot;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(IConfiguration configuration, ILogger<LocalStorageService> logger)
    {
        _logger = logger;
        var basePath = configuration["Storage:LocalBasePath"] ?? "uploads";

        _storageRoot = Path.IsPathRooted(basePath)
            ? basePath
            : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", basePath);

        if (!Directory.Exists(_storageRoot))
        {
            Directory.CreateDirectory(_storageRoot);
        }
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string folder)
    {
        var cleanFileName = Path.GetFileName(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}_{cleanFileName}";
        var folderPath = Path.Combine(_storageRoot, folder);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var destinationFilePath = Path.Combine(folderPath, uniqueFileName);

        using (var fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            await stream.CopyToAsync(fileStream);
        }

        var relativePath = $"uploads/{folder}/{uniqueFileName}".Replace('\\', '/');
        _logger.LogInformation("File uploaded locally to {Path}", destinationFilePath);
        return relativePath;
    }

    public Task<string> UploadFileAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        return UploadAsync(stream, fileName, "application/octet-stream", folder);
    }

    public Task<string> GetSignedUrlAsync(string storageKey, int expiryMinutes = 5)
    {
        // For local development, return the relative URL
        var relativeUrl = storageKey.StartsWith('/') ? storageKey : $"/{storageKey}";
        return Task.FromResult(relativeUrl);
    }

    public Task<string> GetPreSignedUrlAsync(string storageKey, int expiryMinutes = 5, CancellationToken cancellationToken = default)
    {
        return GetSignedUrlAsync(storageKey, expiryMinutes);
    }

    public Task DeleteAsync(string storageKey)
    {
        try
        {
            var relative = storageKey.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase)
                ? storageKey.Substring("uploads/".Length)
                : storageKey.TrimStart('/');

            var filePath = Path.Combine(_storageRoot, relative.Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted at {Path}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {Key}", storageKey);
        }

        return Task.CompletedTask;
    }
}
