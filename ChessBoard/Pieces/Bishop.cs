using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;

namespace ChessBoard.Pieces
{
    internal class Bishop : ChessPiece
    {
        public Bishop(bool white, Vector2 position, IModHelper helper)
            : base(white, "Bishop", position,helper)
        {
            Scale = 0.5f;
        }
        public override void CalculatePossibleMoves(List<ChessPiece> board)
        {
            base.CalculatePossibleMoves(board);
            AddBishopMovement(board);
        }

        public override ChessPiece Clone(IModHelper helper)
        {
            return new Bishop(White, Position, helper);
        }
    }
}
