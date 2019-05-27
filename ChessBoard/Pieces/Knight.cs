using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace ChessBoard.Pieces
{
    internal class Knight : ChessPiece
    {
        public Knight(bool white, Vector2 position, IModHelper helper)
            : base(white,"Knight",position,helper)
        {
            OffsetY = -0.25f;

        }
        public override void CalculatePossibleMoves(List<ChessPiece> board)
        {
            base.CalculatePossibleMoves(board);
            int minX = (int) Math.Max(Position.X-2,0);
            int maxX = (int) Math.Min(Position.X+2,7);
            int minY = (int) Math.Max(Position.Y-2,0);
            int maxY = (int)Math.Min(Position.Y + 2, 7);
            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    if ((Math.Abs(Position.X - x) == 2 && Math.Abs(Position.Y - y) == 1) || (Math.Abs(Position.X - x) == 1 && Math.Abs(Position.Y - y) == 2))
                        if (!board.Exists(c => c.Position.X == x && c.Position.Y == y && c.White == White))
                            Moves.Add(new Vector2(x, y));
        }
        public override ChessPiece Clone(IModHelper helper)
        {
            return new Knight(White, Position, helper);
        }
    }
}
