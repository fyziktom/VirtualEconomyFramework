using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEGameDrivers.Game;

namespace VEGameDrivers.Common
{
    public static class MainGameDataContext
    {
        public static ConcurrentDictionary<string, IGame> Games { get; set; }
    }
}
