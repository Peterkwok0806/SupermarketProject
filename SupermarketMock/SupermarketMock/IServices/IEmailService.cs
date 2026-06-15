namespace SupermarketMock.Services
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string email, string code);
    }

    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;

        public string Port { get; set; } = string.Empty;

        public string User { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

}
