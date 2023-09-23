using Infrastructure.Adapters.Notification.Configs;
using Infrastructure.Adapters.Notification.Exceptions;
using Infrastructure.Adapters.Notification.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Adapters.Notification
{
    public class NotificationAdapter : INotificationAdapter
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly NotificationConfiguration _configs;
        private readonly ILogger _logger;

        public NotificationAdapter(
            ISendGridClient sendGridClient,
            ILogger<NotificationAdapter> logger,
            IOptions<NotificationConfiguration> options)
        {
            _sendGridClient = sendGridClient;
            _logger = logger;
            _configs = options.Value;
        }

        public async Task SendAsync(NotificationContent notification, CancellationToken cancellationToken)
        {
            try
            {
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress(_configs.SendFrom),
                    Subject = notification.Subject,
                    HtmlContent = notification.Content
                };

                msg.AddTo(new EmailAddress(_configs.SendTo));

                _logger.LogInformation("Sending notification to {SendTo}", _configs.SendTo);

                await _sendGridClient.SendEmailAsync(msg, cancellationToken);

                _logger.LogInformation("Notification sent successfully to {SendTo}", _configs.SendTo);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Unexpected error at NotificationAdapter");
                throw new NotificationException("Unexpected error at NotificationAdapter", ex);
            }
        }
    }
}
