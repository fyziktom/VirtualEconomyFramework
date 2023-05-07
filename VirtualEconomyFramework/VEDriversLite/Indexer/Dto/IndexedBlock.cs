using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Indexer.Dto
{
    public class IndexedBlock : IndexedItem
    {
        public string Hash { get; set; } = string.Empty;
        public double Number { get; set; } = -1;
        public double Height { get; set; } = -1;
        public DateTime Time { get; set; } = DateTime.MinValue;
        public List<string> Transactions { get; set; } = new List<string>();
        public int CountOfTxs { get => Transactions.Count; }
    }
}
