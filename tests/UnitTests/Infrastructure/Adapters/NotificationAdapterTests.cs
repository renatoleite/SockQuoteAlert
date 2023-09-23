using Infrastructure.Adapters.Notification;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Infrastructure.Adapters.Notification.Configs;
using Microsoft.Extensions.Options;
using Infrastructure.Adapters.Notification.Models;
using Bogus;
using SendGrid;
using SendGrid.Helpers.Mail;
using Infrastructure.Adapters.Notification.Exceptions;

namespace UnitTests.Infrastructure.Adapters
{
    public class NotificationAdapterTests
    {
        private readonly Mock<ISendGridClient> _sendGridClient;
        private readonly Mock<ILogger<NotificationAdapter>> _logger;
        private readonly Mock<IOptions<NotificationConfiguration>> _config;
        private readonly Faker _faker;

        public NotificationAdapterTests()
        {
            _faker = new Faker();
            _logger = new Mock<ILogger<NotificationAdapter>>();
            _config = new Mock<IOptions<NotificationConfiguration>>();
            _sendGridClient = new Mock<ISendGridClient>();

            _config.Setup(x => x.Value).Returns(new NotificationConfiguration
            {
                SendFrom = "test@test.com",
                SendTo = "test@test.com",
                SendsGridConnectionString = "xpto"
            });
        }

        [Fact]
        public async Task ShouldSendNotificationSuccessfully()
        {
            // Given
            var content = GetFakeNotificationContent();
            var cancellationToken = CancellationToken.None;
            var sendTo = _config.Object.Value.SendTo;

            // Act
            var adapter = new NotificationAdapter(_sendGridClient.Object, _logger.Object, _config.Object);

            await adapter.SendAsync(content, cancellationToken);

            // Assert
            _sendGridClient.Verify(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task ShouldThrowNotificationExceptionWhenThrowAGenericException()
        {
            // Given
            var content = GetFakeNotificationContent();
            var cancellationToken = CancellationToken.None;
            var sendTo = _config.Object.Value.SendTo;

            _sendGridClient
                .Setup(x => x.SendEmailAsync(It.IsAny<SendGridMessage>(), cancellationToken))
                .Throws(new Exception());

            // Act
            var adapter = new NotificationAdapter(_sendGridClient.Object, _logger.Object, _config.Object);

            var execution = () => adapter.SendAsync(content, cancellationToken);

            // Assert
            await Assert.ThrowsAsync<NotificationException>(execution);
        }

        private NotificationContent GetFakeNotificationContent() => new NotificationContent
        {
            Content = _faker.Random.String2(100),
            Subject = _faker.Random.String2(10)
        };
    }
}
