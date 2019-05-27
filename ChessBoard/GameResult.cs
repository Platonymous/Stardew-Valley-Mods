using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessBoard
{
    public class GameResult
    {
        public string WhitePlayer { get; set; }
        public string BlackPlayer { get; set; }
        public string Winner { get; set; }

        public string Time { get; set; }

        public GameResult(string white, string black, string winner)
        {
            WhitePlayer = white;
            BlackPlayer = black;
            Winner = winner;
            Time = Game1.dayOfMonth + ". " + Game1.currentSeason + " / " + Game1.year;
        }
    }
}
