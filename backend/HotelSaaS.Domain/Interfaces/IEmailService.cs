namespace HotelSaaS.Domain.Interfaces;

/// <summary>
/// Provides email sending capabilities.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email message.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="htmlBody">The HTML content of the email body.</param>
    /// <param name="attachments">Optional dictionary of filename to byte array attachments.</param>
    Task SendAsync(string to, string subject, string htmlBody, IDictionary<string, byte[]>? attachments = null);
}
