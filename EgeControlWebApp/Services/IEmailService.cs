using System.Collections.Generic;
using System.Threading.Tasks;

namespace EgeControlWebApp.Services
{
    public record EmailAttachment(string FileName, string ContentType, byte[] Content);

    /// <summary>
    /// Gönderen bilgisi - kullanıcı kendi mail adresi ve şifresi ile gönderir
    /// </summary>
    public class SenderInfo
    {
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
    }

    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string htmlBody, IEnumerable<EmailAttachment>? attachments = null, string? cc = null, string? bcc = null);
        Task SendAsUserAsync(SenderInfo sender, string to, string subject, string htmlBody, IEnumerable<EmailAttachment>? attachments = null, string? cc = null, string? bcc = null);
    }
}
