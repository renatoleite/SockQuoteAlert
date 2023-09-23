namespace Application.UseCases.MonitorStockPrice.Commands
{
    public class StockInputCommand
    {
        public string Symbol { get; set; }
        public double ReferencePriceToSell { get; set; }
        public double ReferencePriceToBuy { get; set; }
    }
}
