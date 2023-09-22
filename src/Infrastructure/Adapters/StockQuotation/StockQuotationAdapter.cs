using Infrastructure.Adapters.StockQuotation.Configs;
using Infrastructure.Adapters.StockQuotation.Exceptions;
using Infrastructure.Adapters.StockQuotation.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Infrastructure.Adapters.StockQuotation
{
    public class StockQuotationAdapter : IStockQuotationAdapter
    {
        private HttpClient _httpClient;
        private ILogger<StockQuotationAdapter> _logger;
        private StockQuotationConfiguration _configs;

        public StockQuotationAdapter(
            HttpClient httpClient,
            IOptions<StockQuotationConfiguration> configs,
            ILogger<StockQuotationAdapter> logger)
        {
            _httpClient = httpClient;
            _configs = configs.Value;
            _logger = logger;
        }

        public async Task<StockData> Get(string symbol)
        {
            try
            {
                using var httpResponse = await _httpClient.GetAsync(_configs.GetUrl(symbol));

                if (!httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("An error occurred when trying to get {symbol} price data", symbol);

                    throw new StockQuotationException(
                        $"Error when trying to get {symbol} price data. " +
                        $"ErrorCode: {httpResponse.StatusCode}.");
                }

                var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
                var globalQuoteData = JsonSerializer.Deserialize<GlobalQuoteData>(jsonResponse);

                return globalQuoteData.Stock;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Unexpected error at StockQuotationAdapter");
                throw new StockQuotationException("Unexpected error at StockQuotationAdapter", ex);
            }
        }
    }
}
