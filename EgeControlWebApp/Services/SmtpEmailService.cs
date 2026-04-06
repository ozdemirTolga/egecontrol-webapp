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

            // Fallback: 587 STARTTLS başarısızsa, 25 (SSL'siz) dene
            // Not: Port 465 implicit SSL gerektirir, SmtpClient bunu desteklemez
        if (_settings.Port == 587)
            {
                try
                {
            await SendWithSettingsAsync(message, _settings.Host, 25, false, _settings.User, _settings.Password);
                    return;
                }
                catch (Exception secondEx)
                {
                    var details = $"Birincil deneme H:{_settings.Host} P:{_settings.Port} SSL:{_settings.EnableSsl} hata: {firstError?.Message}. " +
                                  $"Geri dönüş denemesi H:{_settings.Host} P:25 SSL:false hata: {secondEx.Message}.";
                    throw new Exception($"SMTP gönderimi başarısız oldu. {details}", secondEx);
                }
            }

            // Hiç geri dönüş uygulanmadıysa ilk hatayı zengin mesajla yükselt
            if (firstError != null)
            {
                throw new Exception($"SMTP gönderimi başarısız oldu (H:{_settings.Host} P:{_settings.Port} SSL:{_settings.EnableSsl}). {firstError.Message}", firstError);
            }
        }

        // SSL sertifika doğrulaması
        private static bool ValidateServerCertificate(
            object sender,
            X509Certificate? certificate,
            X509Chain? chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // Shared hosting ortamlarında sertifika hostname uyuşmazlığı yaygındır
            // (mail.egecontrol.com yerine sunucunun kendi hostname'i olabilir)
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                return true;
            }

            return false;
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

        public async Task SendAsUserAsync(SenderInfo sender, string to, string subject, string htmlBody, IEnumerable<EmailAttachment>? attachments = null, string? cc = null, string? bcc = null)
        {
            if (string.IsNullOrWhiteSpace(sender.Email))
                throw new ArgumentException("Gönderen e-posta adresi boş olamaz.");
            if (string.IsNullOrWhiteSpace(sender.SmtpPassword))
                throw new ArgumentException("SMTP şifresi ayarlanmamış. Lütfen Mail Ayarları sayfasından şifrenizi girin.");
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("E-posta alıcısı adresi boş olamaz.", nameof(to));

            using var message = new MailMessage();
            message.From = new MailAddress(sender.Email, sender.DisplayName);

            try
            {
                message.To.Add(new MailAddress(to.Trim()));
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Geçersiz e-posta adresi.", nameof(to), ex);
            }

            if (!string.IsNullOrWhiteSpace(cc))
            {
                foreach (var addr in cc.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    message.CC.Add(new MailAddress(addr.Trim()));
            }
            if (!string.IsNullOrWhiteSpace(bcc))
            {
                foreach (var addr in bcc.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    message.Bcc.Add(new MailAddress(addr.Trim()));
            }

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

            // Kullanıcının kendi e-posta adresi ve şifresiyle gönder
            Exception? firstError = null;
            try
            {
                await SendWithSettingsAsync(message, _settings.Host, _settings.Port, _settings.EnableSsl, sender.Email, sender.SmtpPassword);
                return;
            }
            catch (Exception ex)
            {
                firstError = ex;
            }

            // Fallback: port 25
            if (_settings.Port == 587)
            {
                try
                {
                    await SendWithSettingsAsync(message, _settings.Host, 25, false, sender.Email, sender.SmtpPassword);
                    return;
                }
                catch (Exception secondEx)
                {
                    var details = $"Birincil deneme H:{_settings.Host} P:{_settings.Port} SSL:{_settings.EnableSsl} hata: {firstError?.Message}. " +
                                  $"Geri dönüş denemesi H:{_settings.Host} P:25 SSL:false hata: {secondEx.Message}.";
                    throw new Exception($"SMTP gönderimi başarısız oldu. {details}", secondEx);
                }
            }

            if (firstError != null)
            {
                throw new Exception($"SMTP gönderimi başarısız oldu (H:{_settings.Host} P:{_settings.Port} SSL:{_settings.EnableSsl}). {firstError.Message}", firstError);
            }
        }
    }
}
