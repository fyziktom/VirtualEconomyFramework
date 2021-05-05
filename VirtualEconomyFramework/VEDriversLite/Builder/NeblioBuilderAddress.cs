using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;

namespace VEDriversLite.Builder
{
    public class NeblioBuilderAddress
    {
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
        public string Address { get; set; } = string.Empty;
        public BitcoinAddress NBitcoinAddress { get; set; }
        public BitcoinSecret Secret { get; set; }

        public event EventHandler Refreshed;

        public ConcurrentDictionary<string, NeblioBuilderUtxo> Utxos { get; set; } = new ConcurrentDictionary<string, NeblioBuilderUtxo>();

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
