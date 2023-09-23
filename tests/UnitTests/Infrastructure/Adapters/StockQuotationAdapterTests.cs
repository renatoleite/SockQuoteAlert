using Bogus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Infrastructure.Adapters.StockQuotation;
using Infrastructure.Adapters.StockQuotation.Configs;
using Xunit;
using FluentAssertions;
using Moq.Protected;
using System.Net;
using Infrastructure.Adapters.StockQuotation.Exceptions;

namespace UnitTests.Infrastructure.Adapters
{
    public class StockQuotationAdapterTests
    {
        private readonly Mock<ILogger<StockQuotationAdapter>> _logger;
        private readonly Mock<IOptions<StockQuotationConfiguration>> _config;

        public StockQuotationAdapterTests()
        {
            _logger = new Mock<ILogger<StockQuotationAdapter>>();
            _config = new Mock<IOptions<StockQuotationConfiguration>>();

            _config.Setup(x => x.Value).Returns(new StockQuotationConfiguration
            {
                Key = "key",
                Route = "/test",
                Url = "http://test.com"
            });
        }

        [Fact]
        public async Task ShouldGetStockPriceSuccessfully()
        {
            // Given
            var symbol = "petr4.sa";
            var expectedPrice = "34.0300";
            var cancellationToken = CancellationToken.None;

            // Act
            var adapter = new StockQuotationAdapter(new HttpClient(GetMockedResponse()), _config.Object, _logger.Object);
            var result = await adapter.GetStockPriceAsync(symbol, cancellationToken);

            // Assert
            result.Price.Should().Be(expectedPrice);
        }

        [Fact]
        public async Task ShouldThrowStockQuotationExceptionWhenThrowAGenericException()
        {
            // Given
            var symbol = "petr4.sa";
            var cancellationToken = CancellationToken.None;

            // Act
            var adapter = new StockQuotationAdapter(new HttpClient(GetMockedResponse(false)), _config.Object, _logger.Object);
            var execution = () => adapter.GetStockPriceAsync(symbol, cancellationToken);

            // Assert
            await Assert.ThrowsAsync<StockQuotationException>(execution);
        }

        private HttpMessageHandler GetMockedResponse(bool validResponse = true)
        {
            var mockedHandler = new Mock<HttpMessageHandler>();

            mockedHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(validResponse ? @"{
                        ""Global Quote"": {
                            ""01. symbol"": ""PETR4.SA"",
                            ""02. open"": ""34.0200"",
                            ""03. high"": ""34.1900"",
                            ""04. low"": ""33.8100"",
                            ""05. price"": ""34.0300"",
                            ""06. volume"": ""30938700"",
                            ""07. latest trading day"": ""2023-09-22"",
                            ""08. previous close"": ""33.7600"",
                            ""09. change"": ""0.2700"",
                            ""10. change percent"": ""0.7998%""
                        }
                    }" : string.Empty)
                });

            return mockedHandler.Object;
        }
    }
}