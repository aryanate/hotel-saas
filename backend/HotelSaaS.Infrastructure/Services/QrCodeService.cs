using HotelSaaS.Domain.Interfaces;
using QRCoder;

namespace HotelSaaS.Infrastructure.Services;

public class QrCodeService : IQrCodeService
{
    public Task<byte[]> GenerateQrCodeAsync(string url, int size = 300)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        // Calculate approximate pixels per module to match desired size
        int pixelsPerModule = Math.Max(1, size / 35);
        byte[] qrCodeBytes = qrCode.GetGraphic(pixelsPerModule);

        return Task.FromResult(qrCodeBytes);
    }

    public byte[] GenerateQrCodePng(string text)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }
}
