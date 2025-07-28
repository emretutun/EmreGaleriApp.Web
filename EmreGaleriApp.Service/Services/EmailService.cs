    using EmreGaleriApp.Core.OptionsModel;
using EmreGaleriApp.Service.Services;
using Microsoft.Extensions.Options;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;

    namespace EmreGaleriApp.Web.Services
    {
        public class EmailService : IEmailService
        {
            private readonly EmailSettings _emailSettings;

            public EmailService(IOptions<EmailSettings> options)
            {
                _emailSettings = options.Value;
            }

            public async Task SendResetPasswordEmail(string resetEmailLink, string toEmail)
            {
                using var smtpClient = new SmtpClient
                {
                    Host = _emailSettings.Host!,
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.Email!),
                    Subject = "Şifre Sıfırlama Linki",
                    Body = $@"
                        <h4>Şifrenizi Yenilemek için aşağıdaki linke tıklayınız:</h4>
                        <p><a href=""{resetEmailLink}"">Şifre yenileme linki</a></p>",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(new MailAddress(toEmail));

                await smtpClient.SendMailAsync(mailMessage);
            }

        public async Task SendOrderApprovedEmail(string toEmail, string userName, byte[]? invoicePdf = null)
        {
            using var smtpClient = new SmtpClient
            {
                Host = _emailSettings.Host!,
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.Email!),
                Subject = "Siparişiniz Onaylandı",
                IsBodyHtml = true,
                Body = $@"<h4>Merhaba {userName},</h4>
                  <p>Siparişiniz onaylandı. Faturanız ektedir.</p>"
            };

            mailMessage.To.Add(new MailAddress(toEmail));

            if (invoicePdf != null)
            {
                mailMessage.Attachments.Add(new Attachment(new MemoryStream(invoicePdf), "fatura.pdf"));
            }

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendOrderRejectedEmail(string toEmail, string userName)
        {
            using var smtpClient = new SmtpClient
            {
                Host = _emailSettings.Host!,
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.Email!),
                Subject = "Siparişiniz Reddedildi",
                IsBodyHtml = true,
                Body = $@"<h4>Merhaba {userName},</h4>
                  <p>Siparişiniz yetkili tarafından onaylanmadı. İsterseniz bizimle iletişime geçebilirsiniz.</p>"
            };

            mailMessage.To.Add(new MailAddress(toEmail));

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using var smtpClient = new SmtpClient
            {
                Host = _emailSettings.Host!,
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.Email!),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            mailMessage.To.Add(new MailAddress(toEmail));

            await smtpClient.SendMailAsync(mailMessage);
        }

    }
}
