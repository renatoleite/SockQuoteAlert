using Application.Shared.Models;
using Application.UseCases.MonitorStockPrice.Commands;
using FluentValidation;
using Infrastructure.Adapters.StockQuotation;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.MonitorStockPrice
{

    public class MonitorStockPriceUseCase : IMonitorStockPriceUseCase
    {
        private readonly ILogger<MonitorStockPriceUseCase> _logger;
        private readonly IValidator<StockInputCommand> _validator;
        private readonly IStockQuotationAdapter _stockQuotationAdapter;

        public MonitorStockPriceUseCase(
            ILogger<MonitorStockPriceUseCase> logger,
            IValidator<StockInputCommand> validator,
            IStockQuotationAdapter stockQuotationAdapter)
        {
            _logger = logger;
            _validator = validator;
            _stockQuotationAdapter = stockQuotationAdapter;
        }

        public async Task<Output> ExecuteAsync(StockInputCommand command, CancellationToken cancellationToken)
        {
            var output = new Output();

            try
            {
                var validationResult = await _validator.ValidateAsync(command);

                output.AddValidationResult(validationResult);

                if (!output.IsValid)
                    return output;

                _logger.LogInformation("{UseCase} - Starting monitoring stock price; Symbol: {symbol}",
                    nameof(MonitorStockPriceUseCase), command.Symbol);

                var price = default(double);
                var stockData = await _stockQuotationAdapter.GetStockPriceAsync(command.Symbol, cancellationToken);

                if (stockData == null || !double.TryParse(stockData.Price, out price))
                {
                    _logger.LogWarning("{UseCase} - Was not possible to get stock price; Symbol: {symbol}",
                        nameof(MonitorStockPriceUseCase), command.Symbol);
                }

                if (price < command.ReferencePriceToBuy)
                    await SendNotification(true);
                else if (price > command.ReferencePriceToSell)
                    await SendNotification(false);

                output.AddResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{UseCase} - An unexpected error has occurred; Symbol: {symbol}",
                    nameof(MonitorStockPriceUseCase), command.Symbol);

                output.AddErrorMessage($"An unexpected error has occurred");
            }

            _logger.LogInformation("{UseCase} - Finishing monitoring stock price; Symbol: {symbol}",
                nameof(MonitorStockPriceUseCase), command.Symbol);

            return output;
        }

        private async Task SendNotification(bool buy)
        {

        }
    }
}
