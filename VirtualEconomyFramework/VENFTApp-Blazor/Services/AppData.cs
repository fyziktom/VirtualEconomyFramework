using System;
using System.Collections.Generic;
using System.Linq;
using VEDriversLite;
using VEDriversLite.Cryptocurrencies;
using VEDriversLite.Dto;
using VEDriversLite.Bookmarks;

public class AppData
{
    public NeblioAccount Account { get; set; } = new NeblioAccount();
    public DogeAccount DogeAccount { get; set; } = new DogeAccount();
    public List<TokenOwnerDto> VENFTOwners = new List<TokenOwnerDto>();
    /// <summary>
    /// Service which gets prices of cryptocurrencies
    /// </summary>
    public PriceService ExchangePriceService { get; set; } = new PriceService();

    /// <summary>
    /// This event is fired whenever price from exchanges is refreshed. It provides dictionary of the actual available rates.
    /// </summary>
    public event EventHandler<IDictionary<CurrencyTypes, double>> PricesRefreshed;
    
    /// <summary>
    /// Register event of the PriceService PriceRefreshed. Then the event is resend by NeblioAccountBase class
    /// </summary>
    public void RegisterPriceServiceEventHandler()
    {
        if (ExchangePriceService != null)
        {
            ExchangePriceService.PricesRefreshed -= ExchangePriceService_PricesRefreshed;
            ExchangePriceService.PricesRefreshed += ExchangePriceService_PricesRefreshed;
        }
        else
            Console.WriteLine("Cannot register Event Handler for PriceRefreshed because the ExchangePriceService is null.");
    }

    private void ExchangePriceService_PricesRefreshed(object sender, IDictionary<CurrencyTypes, double> e)
    {
        PricesRefreshed?.Invoke(sender, e);
    }
}

