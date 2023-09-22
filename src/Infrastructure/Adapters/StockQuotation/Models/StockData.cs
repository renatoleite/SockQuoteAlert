using System.Text.Json.Serialization;

namespace Infrastructure.Adapters.StockQuotation.Models
{
    public class StockData
    {
        [JsonPropertyName("01. symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("05. price")]
        public string Price { get; set; }
    }
}
