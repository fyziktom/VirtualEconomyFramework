using VEBlazor.Demo.Publishing.Server.Services;
using VEDriversLite.NFT;

namespace VEBlazor.Demo.Publishing.Server.WindowsService;

public sealed class CoruzantCoreBackgroundService : BackgroundService
{
    private IConfiguration _settings;
    private readonly DataCoreService _dataCoreService;
    private readonly ILogger<CoruzantCoreBackgroundService> _logger;

    public CoruzantCoreBackgroundService(
        IConfiguration settings,
        DataCoreService jokeService,
        ILogger<CoruzantCoreBackgroundService> logger) =>
        (_settings, _dataCoreService, _logger) = (settings, jokeService, logger);        
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        try
        {
            MainDataContext.IpfsSecret = _settings.GetValue<string>("IpfsSecret", string.Empty);
            MainDataContext.IpfsProjectID = _settings.GetValue<string>("IpfsProjectID", string.Empty);
            if (!string.IsNullOrEmpty(MainDataContext.IpfsSecret) && !string.IsNullOrEmpty(MainDataContext.IpfsProjectID))
                NFTHelpers.LoadConnectionInfo(MainDataContext.IpfsProjectID, MainDataContext.IpfsSecret);

            var acc = new List<string>();
            _settings.GetSection("ObservedAccounts").Bind(acc);
            if (acc != null)
                MainDataContext.ObservedAccounts = acc;

            Console.WriteLine("Loading Observing addresses...");
            // load observed addresses
            if (MainDataContext.ObservedAccounts.Count > 0)
            {
                for (var i = 0; i < MainDataContext.ObservedAccounts.Count; i++)
                {
                    try
                    {
                        var tab = new VEDriversLite.Bookmarks.ActiveTab(MainDataContext.ObservedAccounts[i]);
                        tab.MaxLoadedNFTItems = 1500;
                        await tab.StartRefreshing(10000, false, true);
                        MainDataContext.ObservedAccountsTabs.TryAdd(tab.Address, tab);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot start observing of account: " + MainDataContext.ObservedAccounts[i]);
                    }
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                
                /*                
                var nft = await _dataCoreService.GetNFT("feff3736186c6d0c7d1412c8d8d1fe59367e9a91797ce40c4f7bd2bad8a046e0");
                if (nft != null)
                    _logger.LogWarning("{Name}", nft.Name);
                else
                    _logger.LogError("Cannot load the NFT.");
                */

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);

            // Terminates this process and returns an exit code to the operating system.
            // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
            // performs one of two scenarios:
            // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
            // 2. When set to "StopHost": will cleanly stop the host, and log errors.
            //
            // In order for the Windows Service Management system to leverage configured
            // recovery options, we need to terminate the process with a non-zero exit code.
            Environment.Exit(1);
        }
    }
}
