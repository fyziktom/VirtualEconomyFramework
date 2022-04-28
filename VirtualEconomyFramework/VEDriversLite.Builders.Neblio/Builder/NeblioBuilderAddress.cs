using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;

namespace VEDriversLite.Builders.Neblio
{
    /// <summary>
    /// Create Builder Neblio Address and load all necessary parameters 
    /// </summary>
    public class NeblioBuilderAddress
    {
        /// <summary>
        /// Main constructor which loads the address in the shape of BitcoinAddress
        /// </summary>
        /// <param name="address"></param>
        /// <exception cref="Exception"></exception>
        public NeblioBuilderAddress(string address)
        {
            Address = address;
            try
            {
                if (!string.IsNullOrEmpty(address))
                {
                    NBitcoinAddress = BitcoinAddress.Create(address, NeblioTransactionBuilder.NeblioNetwork);
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Wrong Input Address." + ex.Message);
            }
        }
        /// <summary>
        /// Address as text
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Address as NBitcoin.BitcoinAddress object
        /// </summary>
        public BitcoinAddress NBitcoinAddress { get; set; }
        /// <summary>
        /// Loaded bitcoin secret
        /// </summary>
        public BitcoinSecret Secret { get; set; }
        /// <summary>
        /// Object was refreshed
        /// </summary>

        public event EventHandler Refreshed;

        /// <summary>
        /// Loaded NeblioBuilderUtxos objects
        /// </summary>
        public ConcurrentDictionary<string, NeblioBuilderUtxo> Utxos { get; set; } = new ConcurrentDictionary<string, NeblioBuilderUtxo>();

        /// <summary>
        /// Load Utxos for the address
        /// </summary>
        /// <returns></returns>
        public async Task LoadUtxos()
        {
            //Utxos.Clear();
            var ux = await NeblioTransactionHelpers.GetAddressUtxosObjects(Address);
            // add new ones
            foreach (var u in ux)
            {
                var utx = new NeblioBuilderUtxo(u);
                await utx.LoadInfo();
                if (!Utxos.TryGetValue(u.Txid, out var ut))
                    Utxos.TryAdd(utx.Utxo.Txid + ":" + utx.Utxo.Index.ToString(), utx);
            }
            
            // remove old ones
            if (ux.Count > Utxos.Count)
                foreach(var u in Utxos)
                    if (!ux.Contains(u.Value.Utxo))
                        Utxos.TryRemove(u.Key, out var ur);
            
        }

        /// <summary>
        /// Start refreshing the data in cycle of the interval
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public async Task StartRefreshingData(int interval = 10000)
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await LoadUtxos();
                        Refreshed?.Invoke(this, null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot load Utxos. " + ex.Message);
                    }

                    await Task.Delay(interval);
                }
            });
        }

    }
}
