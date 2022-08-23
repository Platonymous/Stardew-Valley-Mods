using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Types;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessBoard.Pieces
{
    public class ChessPiece
    {
        public List<Vector2> Moves { get; set; } = new List<Vector2>();
        public Vector2 Position { get; set; }

        public Color Color { get; set; } = Color.White;

        public Color Hightlight { get; set; } = Color.White;

        public float Scale { get; set; } = 1f;

        public float OffsetY { get; set; } = 0;

        public AnimatedTexture2D Texture { get; set; }
        public string Name { get; set; }

        public bool White { get; set; }

        public string ColorName => this.White ? "White" : "Black";

        public int TileHeight { get; set; } = 1;

        private bool loadedTexture = false;

        public ChessPiece(bool white, string name, Vector2 position, IModHelper helper, bool loadTexture = true)
        {
            White = white;
            Position = position;
            Name = name;
            loadedTexture = false;
            if (loadTexture)
            {
                loadedTexture = true;
                Texture2D texture = helper.GameContent.Load<Texture2D>($"PlatonymousChess/{Name}_{ColorName}");
                Texture = ((new PyTK.Types.AnimatedTexture2D(texture, texture.Height, texture.Height, 6, startPaused: true)));
            }
        }

        public void LoadCharacterTexture(IModHelper helper)
        {
            Texture2D texture = helper.GameContent.Load<Texture2D>($"PlatonymousChess/{Name}_{ColorName}");
            Texture = ((new PyTK.Types.AnimatedTexture2D(texture, texture.Height / 2, texture.Height, 6, startPaused: true)));
            Scale = 0.75f;
            OffsetY = 0.1f;
            TileHeight = 2;
        }

        public virtual void CalculatePossibleMoves(List<ChessPiece> board)
        {
            Moves = new List<Vector2>();
        }

        public void AddRookMovement(List<ChessPiece> board)
        {
            var horizontal = board.Where(p => p.Position.Y == Position.Y && p.Position.X != Position.X).OrderBy(k => Math.Abs(k.Position.X - Position.X));
            var vertical = board.Where(p => p.Position.X == Position.X && p.Position.Y != Position.Y).OrderBy(k => Math.Abs(k.Position.Y - Position.Y));
            int minX = 0;
            int maxX = 7;
            int minY = 0;
            int maxY = 7;
            if (horizontal != null && horizontal.Count() > 0)
                foreach (var c in horizontal)
                    if (minX == 0 && c.Position.X < Position.X)
                        minX =(int) c.Position.X;
                    else if (maxX == 7 && c.Position.X > Position.X)
                        maxX = (int)c.Position.X;


            if (vertical != null && vertical.Count() > 0)
                foreach (var c in vertical)
                    if (minY == 0 && c.Position.Y < Position.Y)
                        minY = (int)c.Position.Y;
                    else if (maxY == 7 && c.Position.Y > Position.Y)
                        maxY = (int)c.Position.Y;

            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    if (x != Position.X && y == Position.Y || y != Position.Y && x == Position.X)
                        if (!board.Exists(c => c.Position.X == x && c.Position.Y == y && c.White == White))
                            Moves.Add(new Vector2(x, y));
        }

        public void AddKingMovement(List<ChessPiece> board)
        {
            int minX = Math.Max((int)Position.X - 1,0);
            int maxX = Math.Min((int)Position.X + 1,7);
            int minY = Math.Max((int)Position.Y - 1,0);
            int maxY = Math.Min((int)Position.Y + 1,7);

            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                        if (!board.Exists(c => c.Position.X == x && c.Position.Y == y && c.White == White))
                            Moves.Add(new Vector2(x, y));
        }

        public void AddPawnMovement(List<ChessPiece> board)
        {
            int minX = (int)Math.Max(Position.X - 1, 0);
            int maxX = (int)Math.Min(Position.X + 1, 7);
            int y = White ? (int)Position.Y - 1 : (int)Position.Y + 1;
            for (int x = minX; x <= maxX; x++)
                if (!board.Exists(c => c.Position.X == x && c.Position.Y == y && c.White == White))
                {
                    int m = (int)(Math.Abs(Position.X - x) + Math.Abs(Position.Y - y));

                    if ((m == 1 && !board.Exists(c => c.Position.X == x && c.Position.Y == y)) ||  (m == 2 && board.Exists(c => c.Position.X == x && c.Position.Y == y && c.White != White)))
                        Moves.Add(new Vector2(x, y));
                }

            if (Position.Y == 6 && White && !board.Exists(c => c.Position.X == Position.X && c.Position.Y == Position.Y - 2))
                Moves.Add(new Vector2(Position.X, Position.Y - 2));

            if (Position.Y == 1 && !White && !board.Exists(c => c.Position.X == Position.X && c.Position.Y == Position.Y + 2))
                Moves.Add(new Vector2(Position.X, Position.Y + 2));
        }

        private void AddBishopDirection(Vector2 direction, List<ChessPiece> board)
        {
            for (Vector2 xy = new Vector2(Position.X + direction.X, Position.Y + direction.Y); ((direction.X < 0 && xy.X >= 0) || (direction.X > 0 && xy.X < 8)) && ((direction.Y < 0 && xy.Y >= 0) || (direction.Y > 0 && xy.Y < 8)); xy += direction)
                if (!board.Exists(c => c.Position == xy && c.White == White))
                {
                    Moves.Add(xy);

                    if (board.Exists(c => c.Position == xy && c.White != White))
                        break;
                }
                else
                    break;
        }

        public void AddBishopMovement(List<ChessPiece> board)
        {
            AddBishopDirection(new Vector2(-1, 1), board);
            AddBishopDirection(new Vector2(1, 1), board);
            AddBishopDirection(new Vector2(-1, -1), board);
            AddBishopDirection(new Vector2(1, -1), board);
        }

        public virtual ChessPiece Clone(IModHelper helper)
        {
            return new ChessPiece(White, Name, Position, helper, loadedTexture);
        }


    }
}
