using Application.Shared.Models;
using Application.UseCases.MonitorStockPrice.Commands;

namespace Application.UseCases.MonitorStockPrice
{
    public interface IMonitorStockPriceUseCase
    {
        Task<Output> ExecuteAsync(StockInputCommand command, CancellationToken cancellationToken);
    }
}
