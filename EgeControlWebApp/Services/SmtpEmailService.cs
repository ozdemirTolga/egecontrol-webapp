using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace EgeControlWebApp.Services
{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
    }

    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public SmtpEmailService(IOptions<SmtpSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendAsync(string to, string subject, string htmlBody, IEnumerable<EmailAttachment>? attachments = null, string? cc = null, string? bcc = null)
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_settings.From, _settings.DisplayName ?? _settings.From);
            message.To.Add(to);
            if (!string.IsNullOrWhiteSpace(cc)) message.CC.Add(cc);
            if (!string.IsNullOrWhiteSpace(bcc)) message.Bcc.Add(bcc);
            message.Subject = subject;
            message.Body = htmlBody;
            message.IsBodyHtml = true;

            if (attachments != null)
            {
                foreach (var att in attachments)
                {
                    var stream = new MemoryStream(att.Content);
                    var a = new Attachment(stream, att.ContentType) { Name = att.FileName, ContentId = att.FileName };
                    message.Attachments.Add(a);
                }
            }

            async Task SendInternalAsync(string host, int port, bool enableSsl)
            {
                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = enableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_settings.User, _settings.Password),
                    Timeout = 100000
                };

                // Log connection details for debugging
                System.Diagnostics.Debug.WriteLine($"Attempting SMTP connection to {host}:{port}, SSL={enableSsl}, User={_settings.User}");
                
                await client.SendMailAsync(message);
            }

            // Try different configurations
            var configs = new[]
            {
                (host: _settings.Host, port: _settings.Port, ssl: _settings.EnableSsl),
                (host: _settings.Host, port: 587, ssl: true),  // STARTTLS
                (host: _settings.Host, port: 465, ssl: true),  // Implicit SSL
                (host: _settings.Host, port: 25, ssl: false),  // Plain SMTP
                (host: _settings.Host, port: 2525, ssl: false) // Alternative port
            };

            Exception? lastException = null;
            var attempts = new List<string>();

            foreach (var config in configs)
            {
                try
                {
                    await SendInternalAsync(config.host, config.port, config.ssl);
                    return; // Success!
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    attempts.Add($"{config.host}:{config.port} SSL={config.ssl} -> {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Failed: {config.host}:{config.port} SSL={config.ssl} - {ex.Message}");
                }
            }

            // All attempts failed
            var attemptDetails = string.Join(" | ", attempts);
            throw new Exception($"SMTP gönderimi başarısız oldu. Denenen konfigürasyonlar: {attemptDetails}. Son hata: {lastException?.Message} {lastException?.InnerException?.Message}");
        }
    }
}
