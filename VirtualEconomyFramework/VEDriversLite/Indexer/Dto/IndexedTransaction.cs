﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Indexer.Dto
{
    public class IndexedTransaction : IndexedItem
    {
        public string Hash { get; set; } = string.Empty;
        public string BlockHash { get; set; } = string.Empty;
        public int BlockNumber { get; set; } = -1;
        public DateTime Blocktime { get; set; } = DateTime.MinValue;
        public List<string> InvolvedAddresses { get; set; } = new List<string>();
        public List<string> Utxos { get; set; } = new List<string>();
    }
}
