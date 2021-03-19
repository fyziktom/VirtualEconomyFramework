using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEGameDrivers.Game.Chess
{
    public class GameState
    {
        public GameState()
        {
            for(var i = 1; i < 9; i++)
            {
                for(var j = 61; j < 69; j++)
                {
                    BoardState.TryAdd($"{Convert.ToChar(j)}{i}", string.Empty);
                }
            }
        }

        public string BoardStringData { get; set; } = string.Empty;
        public string LastMovePlayer { get; set; } = string.Empty;
        public Dictionary<string, string> BoardState { get; set; } = new Dictionary<string, string>();
    }
}
