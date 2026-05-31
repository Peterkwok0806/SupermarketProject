using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;

namespace SupermarketMock.Services
{
    public class EmailService: IEmailService
    {
        private readonly SupermarketContext _context;
        private readonly SmtpSettings _smtpSettings;

        public EmailService(SupermarketContext context, IOptions<SmtpSettings> smtpSettings)
        {
            _context = context;
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendVerificationEmailAsync(string email, string code)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("SuperMart", "noreply@supermart.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = "SuperMart - 您的驗證碼";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"<h2>您的驗證碼是：</h2>
                              <h1 style='color:#6b21a8; font-size:48px; letter-spacing:5px;'>{code}</h1>
                              <p>此驗證碼將在 15 分鐘後失效。</p>
                              <p style='color:gray; font-size:12px;'>如果您沒有申請此驗證碼，請忽略此信件。</p>"
            };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();

            int port = int.TryParse(_smtpSettings.Port, out var p) ? p : 587;

            await smtp.ConnectAsync(_smtpSettings.Host, port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpSettings.User, _smtpSettings.Password);
            await smtp.SendAsync(emailMessage);
            await smtp.DisconnectAsync(true);
        }
    }
}
