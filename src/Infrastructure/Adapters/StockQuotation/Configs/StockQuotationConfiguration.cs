namespace Infrastructure.Adapters.StockQuotation.Configs
{
    public class StockQuotationConfiguration
    {
        public string Key { get; init; }
        public string Url { get; init; }
        public string Route { get; init; }

        public string GetUrl(string symbol) =>
            $"{Url}{Route}"
            .Replace("{key}", Key)
            .Replace("{symbol}", symbol);
    }
}
