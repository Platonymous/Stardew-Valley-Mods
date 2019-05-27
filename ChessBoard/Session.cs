using Microsoft.Xna.Framework;
using PyTK.Types;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessBoard
{
    public class Session
    {
        public List<ValueChangeRequest<Point, Point>> Turns = new List<ValueChangeRequest<Point, Point>>();
        public string WhitePlayer { get; set; } = "White";
        public string BlackPlayer { get; set; } = "Black";
        public bool Open { get; set; } = true;

        public bool NextTurnWhite { get; set; } = true;
        public string Id { get; set; } = "Console";
        public Session(string id, bool open)
        {
            Id = id;
            WhitePlayer = open ? ChessGame.whitePlayerDefault : Game1.player.Name;
            BlackPlayer = ChessGame.blackPlayerDefault;
            Open = open;
        }
    }
}
