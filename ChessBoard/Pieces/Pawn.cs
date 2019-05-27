using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace ChessBoard.Pieces
{
    internal class Pawn : ChessPiece
    {
        public Pawn(bool white, Vector2 position, IModHelper helper)
            : base(white,"Pawn",position,helper)
        {
            if (!white)
                Color = Color.DarkGray;
            else
                Color = Color.Gold;

            Scale = 0.75f;
        }
        public override void CalculatePossibleMoves(List<ChessPiece> board)
        {
            base.CalculatePossibleMoves(board);
            AddPawnMovement(board);
        }
        public override ChessPiece Clone(IModHelper helper)
        {
            return new Pawn(White, Position, helper);
        }
    }
}
