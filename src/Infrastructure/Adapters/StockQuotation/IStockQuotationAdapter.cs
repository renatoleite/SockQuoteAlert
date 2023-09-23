using Infrastructure.Adapters.StockQuotation.Models;

namespace Infrastructure.Adapters.StockQuotation
{
    public interface IStockQuotationAdapter
    {
        public Task<StockData> GetStockPriceAsync(string symbol, CancellationToken cancellationToken);
    }
}
