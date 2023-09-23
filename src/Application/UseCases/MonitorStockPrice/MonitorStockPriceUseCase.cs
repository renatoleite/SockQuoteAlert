using Application.Shared.Models;
using Application.UseCases.MonitorStockPrice.Commands;
using FluentValidation;
using Infrastructure.Adapters.Notification;
using Infrastructure.Adapters.Notification.Models;
using Infrastructure.Adapters.StockQuotation;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.MonitorStockPrice
{
    public class MonitorStockPriceUseCase : IMonitorStockPriceUseCase
    {
        private readonly ILogger<MonitorStockPriceUseCase> _logger;
        private readonly IValidator<StockInputCommand> _validator;
        private readonly IStockQuotationAdapter _stockQuotationAdapter;
        private readonly INotificationAdapter _notificationAdapter;

        public MonitorStockPriceUseCase(
            ILogger<MonitorStockPriceUseCase> logger,
            IValidator<StockInputCommand> validator,
            IStockQuotationAdapter stockQuotationAdapter,
            INotificationAdapter notificationAdapter)
        {
            _logger = logger;
            _validator = validator;
            _stockQuotationAdapter = stockQuotationAdapter;
            _notificationAdapter = notificationAdapter;
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

                var stockPrice = default(double);
                var stockData = await _stockQuotationAdapter.GetStockPriceAsync(command.Symbol, cancellationToken);

                if (stockData == null || !double.TryParse(stockData.Price, out stockPrice))
                {
                    _logger.LogWarning("{UseCase} - Was not possible to get stock price; Symbol: {symbol}",
                        nameof(MonitorStockPriceUseCase), command.Symbol);
                }

                if (stockPrice < command.ReferencePriceToBuy)
                    await SendNotification(command.Symbol, true, cancellationToken);
                else if (stockPrice > command.ReferencePriceToSell)
                    await SendNotification(command.Symbol, false, cancellationToken);

                output.AddResult($"{command.Symbol} monitored successfully");

                _logger.LogInformation("{UseCase} - Finishing monitoring stock price; Symbol: {symbol}",
                    nameof(MonitorStockPriceUseCase), command.Symbol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{UseCase} - An unexpected error has occurred; Symbol: {symbol}",
                    nameof(MonitorStockPriceUseCase), command.Symbol);

                output.AddErrorMessage("An unexpected error has occurred");
            }

            return output;
        }

        private async Task SendNotification(string symbol, bool buy, CancellationToken cancellationToken)
        {
            var operation = buy ? "compra" : "venda";
            var notification = new NotificationContent
            {
                Subject = $"O ativo {symbol} antigiu o valor esperado para {operation}",
                Content = $"O ativo {symbol} antigiu o valor esperado para {operation}. Realize a operação."
            };

            await _notificationAdapter.SendAsync(notification, cancellationToken);
        }
    }
}
