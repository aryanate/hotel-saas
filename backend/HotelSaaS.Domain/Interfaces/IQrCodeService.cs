namespace HotelSaaS.Domain.Interfaces;

/// <summary>
/// Provides QR code generation capabilities.
/// </summary>
public interface IQrCodeService
{
    /// <summary>
    /// Generates a QR code image for the specified URL.
    /// </summary>
    /// <param name="url">The URL to encode in the QR code.</param>
    /// <param name="size">The size in pixels of the generated QR code image. Defaults to 300.</param>
    /// <returns>The QR code image as a PNG byte array.</returns>
    Task<byte[]> GenerateQrCodeAsync(string url, int size = 300);

    /// <summary>
    /// Generates a QR code PNG image synchronously for the specified text.
    /// </summary>
    byte[] GenerateQrCodePng(string text);
}
