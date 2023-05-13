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
        private ConcurrentDictionary<string, DateTime> transactions { get; set; } = new ConcurrentDictionary<string, DateTime>();
        /// <summary>
        /// Get Last 100 transactions
        /// </summary>
        private List<string> Last100Transactions 
        { 
            get
            {
                if (transactions.Count > 100)
                    return transactions.OrderByDescending(t => t.Value).Select(t => t.Key).Take(100).ToList();
                return transactions.Keys.ToList();
            }
        }

        public int CountOfTransactions { get => transactions.Count; }
        public ConcurrentDictionary<string, TokenSupplyDto> TokenSupplies { get; set; } = new ConcurrentDictionary<string, TokenSupplyDto>();
        private ConcurrentDictionary<string, IndexedUtxo> utxos = new ConcurrentDictionary<string, IndexedUtxo>();
        public double NeblioBalance { get => utxos.Values.Where(u => u.Value > 0).Select(u => u.Value).Sum(); }

        public List<IndexedUtxo> Utxos { get => utxos.Values.OrderByDescending(u => u.Blocktime).ToList(); }

        public void UpdateTokenSupply(IndexedUtxo utxo, TokenSupplyDto token)
        {
            if (TokenSupplies.TryGetValue(utxo.TokenId, out var addtoken))
            {
                if (!utxo.Used)
                    addtoken.Amount += utxo.TokenAmount;
                else
                    addtoken.Amount -= utxo.TokenAmount;
            }
            else
            {
                TokenSupplies.TryAdd(utxo.TokenId, new TokenSupplyDto()
                {
                    Amount = utxo.TokenAmount,
                    ImageUrl = token.ImageUrl,
                    TokenId = token.TokenId,
                    TokenSymbol = token.TokenSymbol,
                });
            }
        }

        public bool AddUtxo(IndexedUtxo utxo)
        {
            RemoveUtxo(utxo.TransactionHashAndN);   
            if (utxos.TryAdd(utxo.TransactionHashAndN, utxo))
            {
                LastUpdated = DateTime.UtcNow;
                return true;
            }
            return false;
        }
        public bool RemoveUtxo(string utxo)
        {
            if (utxos.ContainsKey(utxo))
            {
                if (utxos.TryRemove(utxo, out var rutxo))
                {
                    LastUpdated = DateTime.UtcNow;
                    return true;
                }
            }
            return false;
        }

        public void ClearAllUtxos()
        {
            utxos.Clear();
            LastUpdated = DateTime.UtcNow;
        }

        public void AddTransaction(string tx, DateTime time)
        {
            if (!transactions.ContainsKey(tx))
            {
                transactions.TryAdd(tx, time);
                LastUpdated = DateTime.UtcNow;
            }
        }
        public bool RemoveTransaction(string tx)
        {
            if (transactions.ContainsKey(tx))
            {
                if(transactions.TryRemove(tx, out var time))
                {
                    LastUpdated = DateTime.UtcNow;
                    return true;
                }
            }
            return false;
        }

        public void ClearAllTransactions()
        {
            transactions.Clear();
            LastUpdated = DateTime.UtcNow;
        }

        public IEnumerable<string> GetTransactions(int skip = 0, int take = 0) 
        {
            if (take <= 0)
                return transactions.OrderByDescending(t => t.Value).Select(t => t.Key).Skip(skip);
            else
                return transactions.OrderByDescending(t => t.Value).Select(t => t.Key).Skip(skip).Take(take);
        } 
    }
}
