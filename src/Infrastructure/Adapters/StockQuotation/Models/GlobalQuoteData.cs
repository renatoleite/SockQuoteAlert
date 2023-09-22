using System.Text.Json.Serialization;

namespace Infrastructure.Adapters.StockQuotation.Models
{
    public class GlobalQuoteData
    {
        [JsonPropertyName("Global Quote")]
        public StockData Stock { get; set; }
    }
}
