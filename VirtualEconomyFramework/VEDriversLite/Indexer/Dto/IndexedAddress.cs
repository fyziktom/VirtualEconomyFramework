using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Indexer.Dto
{
    public class IndexedAddress
    {
        public string Address { get; set; } = string.Empty;
        public bool Indexed { get; set; } = false;
        public List<string> Transactions { get; set; } = new List<string>();
    }
}
