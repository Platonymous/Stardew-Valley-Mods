using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessBoard
{
    public class LeaderBoardEntry
    {
        public string Name { get; set; } = "";
        public int Games { get; set; } = 1;
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;

        public LeaderBoardEntry(string name, bool won)
        {
            Name = name;
            Wins = won ? 1 : 0;
            Losses = won ? 0 : 1;
        }
    }
}
