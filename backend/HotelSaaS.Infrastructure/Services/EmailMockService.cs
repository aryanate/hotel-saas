using HotelSaaS.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HotelSaaS.Infrastructure.Services;

public class EmailMockService : IEmailService
{
    private readonly ILogger<EmailMockService> _logger;

    public EmailMockService(ILogger<EmailMockService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string htmlBody, IDictionary<string, byte[]>? attachments = null)
    {
        var attachmentNames = attachments != null ? string.Join(", ", attachments.Keys) : "none";

        _logger.LogInformation("[EMAIL MOCK] Sending email to '{To}'. Subject: '{Subject}'. Attachments: {Attachments}. Body preview: {Preview}",
            to, subject, attachmentNames, htmlBody.Length > 200 ? htmlBody[..200] + "..." : htmlBody);

        return Task.CompletedTask;
    }
}
