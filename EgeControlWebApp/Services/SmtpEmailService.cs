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
    public bool UsePickupDirectory { get; set; } = false;
    public string? PickupDirectory { get; set; }
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
            if (string.IsNullOrWhiteSpace(_settings.From))
                throw new ArgumentException("Gönderen (From) adresi yapılandırılmamış.", nameof(_settings.From));
            message.From = new MailAddress(_settings.From, _settings.DisplayName ?? _settings.From);
            // Validate and add To address
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("E-posta alıcısı boş olamaz.", nameof(to));
            try
            {
                var toAddress = new MailAddress(to.Trim());
                message.To.Add(toAddress);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Geçersiz e-posta adresi.", nameof(to), ex);
            }
            // Add CC addresses if any (comma-separated)
            if (!string.IsNullOrWhiteSpace(cc))
            {
                foreach (var addr in cc.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    message.CC.Add(new MailAddress(addr.Trim()));
                }
            }
            // Add BCC addresses if any (comma-separated)
            if (!string.IsNullOrWhiteSpace(bcc))
            {
                foreach (var addr in bcc.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    message.Bcc.Add(new MailAddress(addr.Trim()));
                }
            }
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

            // Development/test için: e-postayı dosyaya yaz
            if (_settings.UsePickupDirectory)
            {
                var pickup = _settings.PickupDirectory;
                if (string.IsNullOrWhiteSpace(pickup))
                {
                    pickup = Path.Combine(AppContext.BaseDirectory, "MailDrop");
                }
                // Ensure absolute path
                var absolutePickup = Path.IsPathRooted(pickup)
                    ? pickup
                    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, pickup));
                Directory.CreateDirectory(absolutePickup);
                using var client = new SmtpClient
                {
                    DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                    PickupDirectoryLocation = absolutePickup
                };
                await client.SendMailAsync(message);
                return;
            }

            // Tek bir yapılandırma ile SMTP gönderimini gerçekleştiriyoruz.
            try
            {
                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_settings.User, _settings.Password),
                    Timeout = 15000 // 15 saniye timeout
                };
                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                throw new Exception($"SMTP gönderimi başarısız oldu: {ex.Message}", ex);
            }
        }
    }
}
