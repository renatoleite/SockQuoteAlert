using Infrastructure.Adapters.StockQuotation.Models;

namespace Infrastructure.Adapters.StockQuotation
{
    public interface IStockQuotationAdapter
    {
        public Task<StockData> Get(string symbol);
    }
}
