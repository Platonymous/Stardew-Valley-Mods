using Netcode;
using StardewValley;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyTK.Types
{
    class StringWorldData : IWorldState
    {
        public WorldDate Date => new WorldDate(Game1.year,Game1.currentSeason,Game1.dayOfMonth);

        public Dictionary<string,string> data = new Dictionary<string, string>();
        public NetFields NetFields => new NetFields();
        bool isPaused = false;
        bool isSubmarineLocked = false;
        int lowestMineLevel = 0;
        public bool IsPaused { get => isPaused; set => isPaused = value; }
        public bool IsSubmarineLocked { get => isSubmarineLocked; set => isSubmarineLocked = value; }
        public int LowestMineLevel { get => lowestMineLevel; set => lowestMineLevel = value; }

        public void UpdateFromGame1()
        {
            
        }

        public void WriteToGame1()
        {
            
        }
    }
}
