using CRMS_API.Services.Interfaces;

namespace CRMS_API.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendConfirmationEmailAsync(string toEmail, string confirmationLink)
        {
            _logger.LogWarning($"--- NEW USER REGISTRATION ---");
            _logger.LogInformation($"TO: {toEmail}");
            _logger.LogInformation($"DEV-ONLY CONFIRMATION LINK: {confirmationLink}");
            _logger.LogWarning($"-------------------------------");

            return Task.CompletedTask;
        }
    }
}