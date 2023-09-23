using Application.UseCases.MonitorStockPrice.Commands;
using FluentValidation;

namespace Application.UseCases.MonitorStockPrice.Validation
{
    public class StockInputCommandValidation : AbstractValidator<StockInputCommand>
    {
        public StockInputCommandValidation()
        {
            RuleFor(x => x.Symbol).NotEmpty();
            RuleFor(x => x.ReferencePriceToSell).GreaterThan(0);
            RuleFor(x => x.ReferencePriceToBuy).GreaterThan(0);
        }
    }
}