using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace ChessBoard.Pieces
{
    internal class King : ChessPiece
    {
        public bool canCastle = true;
        public King(bool white, Vector2 position, IModHelper helper)
            : base(white,"King",position,helper)
        {
            LoadCharacterTexture(helper);
        }
        public override void CalculatePossibleMoves(List<ChessPiece> board)
        {
            base.CalculatePossibleMoves(board);
            AddKingMovement(board);
        }
        public override ChessPiece Clone(IModHelper helper)
        {
            return new King(White, Position, helper);
        }
    }
}
