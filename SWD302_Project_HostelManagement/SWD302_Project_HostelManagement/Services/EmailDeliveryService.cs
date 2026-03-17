using System.Net;
using System.Net.Mail;

namespace SWD302_Project_HostelManagement.Services
{
    public class EmailDeliveryService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailDeliveryService> _logger;

        public EmailDeliveryService(IConfiguration configuration, ILogger<EmailDeliveryService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Sends email via SMTP
        /// </summary>
        /// <param name="to">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body content</param>
        /// <returns>True if sent successfully, false otherwise</returns>
        public bool SendEmail(string to, string subject, string body)
        {
            try
            {
                // Read configuration from appsettings.json
                string smtpHost = _configuration["EmailSettings:SmtpHost"];
                string smtpPortStr = _configuration["EmailSettings:SmtpPort"];
                string senderEmail = _configuration["EmailSettings:SenderEmail"];
                string senderPassword = _configuration["EmailSettings:SenderPassword"];

                // Validate configuration
                if (string.IsNullOrWhiteSpace(smtpHost) || 
                    string.IsNullOrWhiteSpace(senderEmail) || 
                    string.IsNullOrWhiteSpace(senderPassword))
                {
                    _logger.LogError("Email configuration is incomplete or missing");
                    return false;
                }

                if (!int.TryParse(smtpPortStr, out int smtpPort))
                {
                    _logger.LogError("Invalid SMTP port configuration");
                    return false;
                }

                // Create SMTP client
                using (SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = 10000;

                    // Create mail message
                    using (MailMessage mailMessage = new MailMessage(senderEmail, to))
                    {
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;

                        // Send email
                        smtpClient.Send(mailMessage);
                        _logger.LogInformation($"Email sent successfully to {to}");
                        return true;
                    }
                }
            }
            catch (SmtpException ex)
            {
                _logger.LogError($"SMTP error while sending email to {to}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while sending email to {to}: {ex.Message}");
                return false;
            }
        }
    }
}
