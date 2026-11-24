using System.Text.Json;
using HotelSaaS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HotelSaaS.Infrastructure.Services;

public class WhatsAppMockService : IWhatsAppService
{
    private readonly ILogger<WhatsAppMockService> _logger;
    private static readonly object _lock = new();
    private readonly string _logFilePath;

    public WhatsAppMockService(ILogger<WhatsAppMockService> logger)
    {
        _logger = logger;
        var logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }
        _logFilePath = Path.Combine(logDir, "whatsapp_mock.json");
    }

    public async Task<string> SendMessageAsync(string to, string message, CancellationToken cancellationToken = default)
    {
        var externalId = "mock_" + Guid.NewGuid();

        _logger.LogInformation("[WHATSAPP MOCK] Sending message to {To}: {Message}. ExternalId: {ExternalId}",
            to, message, externalId);

        var logEntry = new
        {
            Type = "TextMessage",
            ExternalId = externalId,
            To = to,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        await AppendToLogFileAsync(logEntry);

        return externalId;
    }

    public async Task<string> SendTemplateAsync(string to, string templateName, IReadOnlyList<string> parameters, string idempotencyKey)
    {
        var externalId = "mock_" + Guid.NewGuid();

        _logger.LogInformation("[WHATSAPP MOCK] Sending template '{Template}' to {To} with key {IdempotencyKey}. Params: {Params}. ExternalId: {ExternalId}",
            templateName, to, idempotencyKey, string.Join(", ", parameters), externalId);

        var logEntry = new
        {
            Type = "Template",
            ExternalId = externalId,
            To = to,
            TemplateName = templateName,
            Parameters = parameters,
            IdempotencyKey = idempotencyKey,
            Timestamp = DateTime.UtcNow
        };

        await AppendToLogFileAsync(logEntry);

        return externalId;
    }

    public async Task<string> SendDocumentAsync(string to, string documentUrl, string caption, string idempotencyKey)
    {
        var externalId = "mock_" + Guid.NewGuid();

        _logger.LogInformation("[WHATSAPP MOCK] Sending document '{Url}' to {To} with caption '{Caption}'. ExternalId: {ExternalId}",
            documentUrl, to, caption, externalId);

        var logEntry = new
        {
            Type = "Document",
            ExternalId = externalId,
            To = to,
            DocumentUrl = documentUrl,
            Caption = caption,
            IdempotencyKey = idempotencyKey,
            Timestamp = DateTime.UtcNow
        };

        await AppendToLogFileAsync(logEntry);

        return externalId;
    }

    public async Task LogDeliveryStatusAsync(string externalId, string status, DateTime timestamp)
    {
        _logger.LogInformation("[WHATSAPP MOCK] Status update for ExternalId {ExternalId}: {Status} at {Timestamp}",
            externalId, status, timestamp);

        var logEntry = new
        {
            Type = "StatusUpdate",
            ExternalId = externalId,
            Status = status,
            Timestamp = timestamp
        };

        await AppendToLogFileAsync(logEntry);
    }

    private Task AppendToLogFileAsync(object entry)
    {
        lock (_lock)
        {
            var line = JsonSerializer.Serialize(entry);
            File.AppendAllLines(_logFilePath, new[] { line });
        }
        return Task.CompletedTask;
    }
}
