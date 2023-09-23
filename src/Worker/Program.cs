using Application.UseCases.MonitorStockPrice;
using Application.UseCases.MonitorStockPrice.Commands;
using Application.UseCases.MonitorStockPrice.Validation;
using FluentValidation;
using Infrastructure.Adapters.Notification;
using Infrastructure.Adapters.Notification.Configs;
using Infrastructure.Adapters.StockQuotation;
using Infrastructure.Adapters.StockQuotation.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SendGrid.Extensions.DependencyInjection;

using var host = CreateHostBuilder(args).Build();
using var scope = host.Services.CreateScope();

var services = scope.ServiceProvider;

try
{
    Console.WriteLine("Digite o ativo, preço de referencia de venda e compra.");
    Console.WriteLine("Exemplo: petr4.sa 22.67 22.59");
    Console.WriteLine();

    var invalidParams = true;
    var sellPrice = default(double);
    var buyPrice = default(double);
    var symbol = string.Empty;

    while (invalidParams)
    {
        var input = Console.ReadLine();
        var @params = input.Split(" ");

        if (@params.Length != 3 ||
            !double.TryParse(@params[1], out sellPrice) ||
            !double.TryParse(@params[2], out buyPrice))
        {
            Console.WriteLine("Parâmetros invalidos, digite novamente conforme exemplo.");
            Console.WriteLine();
        }else
        {
            symbol = @params[0];
            invalidParams = false;
        }
    }

    Console.WriteLine("Iniciando monitoramento.");
    Console.WriteLine();

    var usecase = services.GetService<IMonitorStockPriceUseCase>();

    while (true)
    {
        await usecase.ExecuteAsync(new StockInputCommand
        {
            Symbol = symbol,
            ReferencePriceToSell = sellPrice,
            ReferencePriceToBuy = Convert.ToDouble(buyPrice)
        }, CancellationToken.None);

        Thread.Sleep(10000);
    }
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

IHostBuilder CreateHostBuilder(string[] args)
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .Build();

    return Host.CreateDefaultBuilder()
        .ConfigureServices((_, services) =>
        {
            services.AddTransient<Program>();
            services.AddLogging();

            services.Configure<NotificationConfiguration>(options => configuration.GetSection(nameof(NotificationConfiguration)).Bind(options));
            services.Configure<StockQuotationConfiguration>(options => configuration.GetSection(nameof(StockQuotationConfiguration)).Bind(options));

            services.AddSingleton<INotificationAdapter, NotificationAdapter>();
            services.AddSingleton<IStockQuotationAdapter, StockQuotationAdapter>();
            services.AddHttpClient<IStockQuotationAdapter, StockQuotationAdapter>();

            services.AddSingleton<IValidator<StockInputCommand>, StockInputCommandValidation>();
            services.AddSingleton<IMonitorStockPriceUseCase, MonitorStockPriceUseCase>();

            services.AddSendGrid(options =>
                options.ApiKey = configuration.GetSection("NotificationConfiguration:SendsGridKey").Value
            );
        });
}