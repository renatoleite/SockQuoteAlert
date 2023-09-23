using Application.UseCases.MonitorStockPrice;
using Application.UseCases.MonitorStockPrice.Commands;
using Application.UseCases.MonitorStockPrice.Validation;
using FluentAssertions;
using Infrastructure.Adapters.Notification;
using Infrastructure.Adapters.Notification.Models;
using Infrastructure.Adapters.StockQuotation;
using Infrastructure.Adapters.StockQuotation.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UnitTests.Application.UseCases
{
    public class MonitorStockPriceUseCaseTests
    {
        private Mock<INotificationAdapter> _notificationAdapter;
        private Mock<IStockQuotationAdapter> _stockQuotationAdapter;
        private Mock<ILogger<MonitorStockPriceUseCase>> _logger;
        private StockInputCommandValidation _validator;
        private IMonitorStockPriceUseCase _useCase;

        public MonitorStockPriceUseCaseTests()
        {
            _notificationAdapter = new Mock<INotificationAdapter>();
            _stockQuotationAdapter = new Mock<IStockQuotationAdapter>();
            _logger = new Mock<ILogger<MonitorStockPriceUseCase>>();
            _validator = new StockInputCommandValidation();

            _useCase = new MonitorStockPriceUseCase(
                _logger.Object,
                _validator,                
                _stockQuotationAdapter.Object,
                _notificationAdapter.Object);
        }

        [Fact]
        public async Task ShouldSendEmailToBuy()
        {
            // Given
            var symbol = "petr4.sa";
            var priceToBuy = 10;
            var priceToSell = 20;
            var cancellationToken = CancellationToken.None;
            var expectedResult = GetExpectedNotification(symbol, true);

            _stockQuotationAdapter
                .Setup(x => x.GetStockPriceAsync(symbol, cancellationToken))
                .ReturnsAsync(new StockData
                {
                    Symbol = symbol,
                    Price = "5"
                });

            // Act
            var output = await _useCase.ExecuteAsync(
                new StockInputCommand
                {
                    Symbol = symbol,
                    ReferencePriceToBuy = priceToBuy,
                    ReferencePriceToSell = priceToSell
                }, cancellationToken);

            // Assert
            output.IsValid.Should().BeTrue();

            _notificationAdapter.Verify(x => x.SendAsync(
                It.Is<NotificationContent>(p => p.Subject == expectedResult.Subject && p.Content == expectedResult.Content),
                cancellationToken), Times.Once);
        }

        [Fact]
        public async Task ShouldSendEmailToSell()
        {
            // Given
            var symbol = "petr4.sa";
            var priceToBuy = 10;
            var priceToSell = 20;
            var cancellationToken = CancellationToken.None;
            var expectedResult = GetExpectedNotification(symbol, false);

            _stockQuotationAdapter
                .Setup(x => x.GetStockPriceAsync(symbol, cancellationToken))
                .ReturnsAsync(new StockData
                {
                    Symbol = symbol,
                    Price = "30"
                });

            // Act
            var output = await _useCase.ExecuteAsync(
                new StockInputCommand
                {
                    Symbol = symbol,
                    ReferencePriceToBuy = priceToBuy,
                    ReferencePriceToSell = priceToSell
                }, cancellationToken);

            // Assert
            output.IsValid.Should().BeTrue();

            _notificationAdapter.Verify(x => x.SendAsync(
                It.Is<NotificationContent>(p => p.Subject == expectedResult.Subject && p.Content == expectedResult.Content),
                cancellationToken), Times.Once);
        }

        [Fact]
        public async Task ShouldReturnErrorValidationIfCommandIsEmpty()
        {
            // Given
            var command = new StockInputCommand();
            var cancellationToken = CancellationToken.None;

            // Act
            var output = await _useCase.ExecuteAsync(command, cancellationToken);

            // Assert
            output.IsValid.Should().BeFalse();
            output.ErrorMessages.Should().ContainEquivalentOf("'Symbol' must not be empty.");
            output.ErrorMessages.Should().ContainEquivalentOf("'Reference Price To Sell' must be greater than '0'.");
            output.ErrorMessages.Should().ContainEquivalentOf("'Reference Price To Buy' must be greater than '0'.");
        }

        [Fact]
        public async Task ShouldLogErrorWhenThrowAnException()
        {
            // Given
            var symbol = "petr4.sa";
            var priceToBuy = 10;
            var priceToSell = 20;
            var cancellationToken = CancellationToken.None;

            _stockQuotationAdapter
                .Setup(x => x.GetStockPriceAsync(symbol, cancellationToken))
                .Throws(new Exception());

            // Act
            var output = await _useCase.ExecuteAsync(
                new StockInputCommand
                {
                    Symbol = symbol,
                    ReferencePriceToBuy = priceToBuy,
                    ReferencePriceToSell = priceToSell
                }, cancellationToken);

            // Assert
            output.IsValid.Should().BeFalse();
            output.ErrorMessages.Should().ContainEquivalentOf("An unexpected error has occurred");
        }

        private NotificationContent GetExpectedNotification(string symbol, bool buy)
        {
            var operation = buy ? "compra" : "venda";
            return new NotificationContent
            {
                Subject = $"O ativo {symbol} antigiu o valor esperado para {operation}",
                Content = $"O ativo {symbol} antigiu o valor esperado para {operation}. Realize a operação."
            };
        }
    }
}
