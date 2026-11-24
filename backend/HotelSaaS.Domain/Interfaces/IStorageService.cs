namespace HotelSaaS.Domain.Interfaces;

/// <summary>
/// Provides cloud storage operations for file upload, retrieval, and deletion.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Uploads a file stream to storage and returns the storage key.
    /// </summary>
    /// <param name="stream">The file content stream.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="contentType">The MIME content type.</param>
    /// <param name="folder">The destination folder/prefix.</param>
    /// <returns>The storage key of the uploaded file.</returns>
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, string folder);

    /// <summary>
    /// Uploads a file stream to storage and returns the storage key / URL.
    /// </summary>
    Task<string> UploadFileAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a signed URL for temporary access to a stored file.
    /// </summary>
    /// <param name="storageKey">The storage key of the file.</param>
    /// <param name="expiryMinutes">The number of minutes until the URL expires. Defaults to 5.</param>
    /// <returns>A signed URL valid for the specified duration.</returns>
    Task<string> GetSignedUrlAsync(string storageKey, int expiryMinutes = 5);

    /// <summary>
    /// Generates a pre-signed URL for temporary access to a stored file.
    /// </summary>
    Task<string> GetPreSignedUrlAsync(string storageKey, int expiryMinutes = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently deletes a file from storage.
    /// </summary>
    /// <param name="storageKey">The storage key of the file to delete.</param>
    Task DeleteAsync(string storageKey);
}
