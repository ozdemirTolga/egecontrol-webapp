using System.Collections.Generic;
using System.Threading.Tasks;

namespace EgeControlWebApp.Services
{
    public record EmailAttachment(string FileName, string ContentType, byte[] Content);

    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string htmlBody, IEnumerable<EmailAttachment>? attachments = null, string? cc = null, string? bcc = null);
    }
}
