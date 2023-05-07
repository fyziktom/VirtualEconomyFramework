using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.Indexer.Dto
{
    public class IndexedAddress
    {
        public string Address { get; set; } = string.Empty;
        public bool Indexed { get; set; } = false;
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;
        public List<string> Transactions { get; set; } = new List<string>();
        public ConcurrentDictionary<string, TokenSupplyDto> TokenSupplies { get; set; } = new ConcurrentDictionary<string, TokenSupplyDto>();
        private ConcurrentDictionary<string, IndexedUtxo> utxos = new ConcurrentDictionary<string, IndexedUtxo>();
        public double NeblioBalance { get => utxos.Values.Where(u => u.Value > 0).Select(u => u.Value).Sum(); }

        public List<IndexedUtxo> Utxos { get => utxos.Values.OrderByDescending(u => u.Blocktime).ToList(); }

        public void AddUtxo(IndexedUtxo utxo)
        {
            if (!utxos.ContainsKey(utxo.TransactionHashAndN))
                utxos.TryAdd(utxo.TransactionHashAndN, utxo);
        }
        public void RemoveUtxo(string utxo)
        {
            if (utxos.ContainsKey(utxo))
                utxos.TryRemove(utxo, out var rutxo);
        }

        public void ClearAllUtxos()
        {
            utxos.Clear();
        }

    }
}
