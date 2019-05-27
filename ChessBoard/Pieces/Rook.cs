using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace ChessBoard.Pieces
{
    public class Rook : ChessPiece
    {
        public bool canCastle = true;
        public Rook(bool white, Vector2 position, IModHelper helper)
            : base(white,"Rook",position,helper, false)
        {
            LoadCharacterTexture(helper);
        }
        public override void CalculatePossibleMoves(List<ChessPiece> board)
        {
            base.CalculatePossibleMoves(board);
            AddRookMovement(board);
        }
        public override ChessPiece Clone(IModHelper helper)
        {
            return new Rook(White, Position, helper);
        }
    }
}
