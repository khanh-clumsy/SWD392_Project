using System.Net;
using System.Net.Mail;
using SWD302_Project_HostelManagement.Models;
using SWD302_Project_HostelManagement.Services;

namespace SWD302_Project_HostelManagement.Proxies
{
    public class EmailProxy
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailProxy> _logger;
        private readonly EmailDeliveryService _emailDeliveryService;

        public EmailProxy(IConfiguration configuration, ILogger<EmailProxy> logger, EmailDeliveryService emailDeliveryService)
        {
            _configuration = configuration;
            _logger = logger;
            _emailDeliveryService = emailDeliveryService;
        }

        /// <summary>
        /// Sends email for a notification
        /// </summary>
        /// <param name="recipientEmail">The email address of the recipient</param>
        /// <param name="notification">The notification containing the message content</param>
        /// <returns>True if email sent successfully, false otherwise</returns>
        public bool SendEmail(string recipientEmail, Notification notification)
        {
            // Validate email is not empty
            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                _logger.LogError($"Invalid recipient email: {recipientEmail}");
                return false;
            }

            // Validate email format using MailAddress
            try
            {
                var mailAddress = new MailAddress(recipientEmail);
            }
            catch (FormatException)
            {
                _logger.LogError($"Invalid email format: {recipientEmail}");
                return false;
            }

            // Get message content from notification
            string messageContent = notification.MessageContent;

            // Validate notification content is not empty
            if (string.IsNullOrWhiteSpace(messageContent))
            {
                _logger.LogError("Notification content is empty");
                return false;
            }

            // Forward to EmailDeliveryService (SMTP)
            try
            {
                bool result = _emailDeliveryService.SendEmail(
                    to: recipientEmail,
                    subject: notification.Subject,
                    body: messageContent
                );

                if (result)
                {
                    _logger.LogInformation($"Email Delivery Service confirmed: sent to {recipientEmail}");
                    return true;
                }
                else
                {
                    _logger.LogError($"Email Delivery Service returned failure for: {recipientEmail}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred while sending email to {recipientEmail}: {ex.Message}");
                return false;
            }
        }
    }
}
