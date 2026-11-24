namespace HotelSaaS.Domain.Interfaces;

/// <summary>
/// Provides WhatsApp messaging capabilities including template messages and document delivery.
/// </summary>
public interface IWhatsAppService
{
    /// <summary>
    /// Sends a plain text message via WhatsApp.
    /// </summary>
    Task<string> SendMessageAsync(string to, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a WhatsApp template message to a recipient.
    /// </summary>
    /// <param name="to">The recipient's mobile number in E.164 format.</param>
    /// <param name="templateName">The approved template name.</param>
    /// <param name="parameters">The template parameter values.</param>
    /// <param name="idempotencyKey">A unique key to prevent duplicate sends.</param>
    /// <returns>The external message ID returned by the WhatsApp provider.</returns>
    Task<string> SendTemplateAsync(string to, string templateName, IReadOnlyList<string> parameters, string idempotencyKey);

    /// <summary>
    /// Sends a document via WhatsApp.
    /// </summary>
    /// <param name="to">The recipient's mobile number in E.164 format.</param>
    /// <param name="documentUrl">The publicly accessible URL of the document.</param>
    /// <param name="caption">An optional caption for the document.</param>
    /// <param name="idempotencyKey">A unique key to prevent duplicate sends.</param>
    /// <returns>The external message ID returned by the WhatsApp provider.</returns>
    Task<string> SendDocumentAsync(string to, string documentUrl, string caption, string idempotencyKey);

    /// <summary>
    /// Logs a delivery status update received from the WhatsApp provider webhook.
    /// </summary>
    /// <param name="externalId">The external message ID.</param>
    /// <param name="status">The delivery status string from the provider.</param>
    /// <param name="timestamp">The timestamp of the status update.</param>
    Task LogDeliveryStatusAsync(string externalId, string status, DateTime timestamp);
}
