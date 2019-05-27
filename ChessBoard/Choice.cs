using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessBoard
{
    public class Choice
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public Action<string> Action { get; set; }
        public Rectangle? Bounds { get; set; }

        public Choice(string id, string text, Action<string> action, Rectangle? bounds = null)
        {
            Id = id;
            Text = text;
            Action = action;
            Bounds = bounds;
        }

        public void Pick(ChessGame game)
        {
            Action.Invoke(Id);
            game.GameQuestion = null;
        }
    }
}
