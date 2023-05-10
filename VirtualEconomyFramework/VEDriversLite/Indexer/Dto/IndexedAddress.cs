﻿using System;
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
        private List<string> transactions { get; set; } = new List<string>();
        /// <summary>
        /// Get Last 100 transactions
        /// </summary>
        private List<string> Last100Transactions 
        { 
            get
            {
                if (transactions.Count > 100)
                    return transactions.Take(100).ToList();
                return transactions;
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
            if (utxos.TryGetValue(utxo.TransactionHashAndN, out var u))
            {
                u = utxo;
                LastUpdated = DateTime.UtcNow;
                return true;
            }
            else
            {
                if (utxos.TryAdd(utxo.TransactionHashAndN, utxo))
                {
                    LastUpdated = DateTime.UtcNow;
                    return true;
                }
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

        public void AddTransaction(string tx)
        {
            if (!transactions.Contains(tx))
            {
                transactions.Add(tx);
                LastUpdated = DateTime.UtcNow;
            }
        }
        public bool RemoveTransaction(string tx)
        {
            if (transactions.Contains(tx))
            {
                if(transactions.Remove(tx))
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
                return transactions.Skip(skip);
            else
                return transactions.Skip(skip).Take(take);
        } 
    }
}
