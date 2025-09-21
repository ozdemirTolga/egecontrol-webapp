using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
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
            // E-posta adres kontrolü
            if (string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentException("E-posta alıcısı adresi boş olamaz.", nameof(to));
            }

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
            // UTF-8 içeriği doğru göndermek için encoding ayarları
            message.SubjectEncoding = System.Text.Encoding.UTF8;
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.HeadersEncoding = System.Text.Encoding.UTF8;
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

            // SMTP gönderimi: ana deneme + gerektiğinde 465'e geri dönüş (implicit SSL)
            Exception? firstError = null;
            try
            {
                await SendWithSettingsAsync(message, _settings.Host, _settings.Port, _settings.EnableSsl, _settings.User, _settings.Password);
                return;
            }
            catch (Exception ex)
            {
                firstError = ex;
            }

            // Fallback: 587 TLS başarısızsa 465 implicit SSL dene
        if (_settings.EnableSsl && _settings.Port == 587)
            {
                try
                {
            await SendWithSettingsAsync(message, _settings.Host, 465, true, _settings.User, _settings.Password);
                    return;
                }
                catch (Exception secondEx)
                {
                    var details = $"Birincil deneme H:{_settings.Host} P:{_settings.Port} SSL:{_settings.EnableSsl} hata: {firstError?.Message}. " +
                                  $"Geri dönüş denemesi H:{_settings.Host} P:465 SSL:true hata: {secondEx.Message}.";
                    throw new Exception($"SMTP gönderimi başarısız oldu. {details}", secondEx);
                }
            }

            // Hiç geri dönüş uygulanmadıysa ilk hatayı zengin mesajla yükselt
            if (firstError != null)
            {
                throw new Exception($"SMTP gönderimi başarısız oldu (H:{_settings.Host} P:{_settings.Port} SSL:{_settings.EnableSsl}). {firstError.Message}", firstError);
            }
        }

        // SSL sertifika doğrulaması için güvenli callback metodu
        private static bool ValidateServerCertificate(
            object sender,
            X509Certificate? certificate,
            X509Chain? chain,
            SslPolicyErrors sslPolicyErrors)
        {
            // Development ortamında tüm sertifikaları kabul et
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                return true;
            }

            // Production ortamında sadece RemoteCertificateNameMismatch hatalarını göz ardı et
            // Diğer SSL hataları (expired, self-signed vs.) için false döndür
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                return true; // Host name mismatch hatalarını göz ardı et
            }

            return false; // Diğer tüm SSL hataları için false
        }

        private static async Task SendWithSettingsAsync(MailMessage message, string host, int port, bool enableSsl, string user, string password)
        {
            // Geliştirme ortamında sertifika esnekliği korunuyor (ValidateServerCertificate içinde)
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(user, password),
                Timeout = 30000
            };

            await client.SendMailAsync(message);
        }
    }
}
