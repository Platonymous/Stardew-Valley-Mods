using Microsoft.Xna.Framework;
using System;

namespace Arcade2048
{
    class Tile
    {
        internal Vector2 position;
        internal float value;
        internal float moveX = 0;
        internal float moveY = 0;
        internal bool isMoving = false;
        internal bool hasmerged = false;
        const int powBase = 2;
        const float winValue = 11;
        internal Color baseColor = Color.IndianRed;

        public Tile(Vector2 position, int value = 1)
        {
            this.position = position;
            this.value = value;
        }

        internal void SetMovement(float x, float y)
        {
            moveX = x;
            moveY = y;
        }

        internal void snapToGrid()
        {
            position = GridPosition;
        }

        internal Point GetCenter(Game2048 game)
        {
           return game.getTileSquare(position).Center;
        }

        internal Point GetGridCenter(Game2048 game)
        {
            return game.getTileSquare(GridPosition).Center;
        }

        internal Vector2 GridPosition => new Vector2((float)Math.Round(position.X), (float)Math.Round(position.Y));

        internal bool intersects(Tile t, Game2048 game)
        {
            if (position == GridPosition)
            {
                if (moveX != 0)
                    return (t.GridPosition == new Vector2(GridPosition.X - moveX, GridPosition.Y));
                if (moveY != 0)
                    return (t.GridPosition == new Vector2(GridPosition.X, GridPosition.Y - moveY));
            }
            else
            {
                if ((moveX != 0 && t.GridPosition.Y == GridPosition.Y))
                    return Math.Abs(GetCenter(game).X - t.GetCenter(game).X) <= game.drawTileSize + game.drawMarginSize * 2;

                if ((moveY != 0 && t.GridPosition.X == GridPosition.X))
                    return Math.Abs(GetCenter(game).Y - t.GetCenter(game).Y) <= game.drawTileSize + game.drawMarginSize * 2;
            }
            return false;
        }

        internal bool checkMerger(Game2048 game)
        {
            if (hasmerged)
                return false;

            Tile merge = game.Tiles.Find(t => t != this && t.value == value && getDistance(t.GetCenter(game), GetCenter(game)) < game.drawMarginSize);

            if (merge is Tile tile && !tile.hasmerged)
            {
                value = 0;
                game.score += tile.nextValue();
                tile.hasmerged = true;
                return false;
            }

            return false;
        }

        internal bool Move(Game2048 game)
        {
            isMoving = false;

            if (checkMerger(game))
                return false;

            if (moveX == 0 && moveY == 0)
                return false;

            Vector2 nextPosition = new Vector2(position.X + moveX, position.Y + moveY);
            Rectangle nextBox = game.getTileSquare(new Vector2(nextPosition.X, nextPosition.Y));


            if (moveX == 0 && moveY == 0)
                return false;

            bool inBounds = nextBox.X - game.drawMarginSize >= game.drawPosition.X && nextBox.Y - game.drawMarginSize >= game.drawPosition.Y && nextBox.X + nextBox.Width + game.drawMarginSize <= game.drawBoardSize + game.drawPosition.X && nextBox.Y + nextBox.Height + game.drawMarginSize <= game.drawBoardSize + game.drawPosition.Y;

            if (!inBounds)
            {
                snapToGrid();
                return false;
            }

            Vector2 next = new Vector2(GridPosition.X + (moveX < 0 ? -1 : moveX > 0 ? 1 : 0), GridPosition.Y + (moveY < 0 ? -1 : moveY > 0 ? 1 : 0));

            Tile collision = game.Tiles.Find(t => t != this && t.value > 0 && (t.value != value || hasmerged || t.hasmerged) && t.position == t.GridPosition && t.GridPosition == next && getDistance(t.GetCenter(game),GetCenter(game)) <= (t.value == value ? getDistance(t.GetGridCenter(game), GetGridCenter(game)) : getDistance(t.GetGridCenter(game),GetGridCenter(game))));

            if (collision is Tile)
            {
                snapToGrid();
                return false;
            }

            

            isMoving = true;
            position = nextPosition;

            return true;
        }

        internal static double getDistance(Point p1, Point p2)
        {
            float distX = Math.Abs(p1.X - p2.X);
            float distY = Math.Abs(p1.Y - p2.Y);
            double dist = Math.Sqrt((distX * distX) + (distY * distY));
            return dist;
        }

        internal Color Color
        {
            get{
                float percentage = (Math.Min(value, winValue) / winValue);
                return Game2048.setSaturation(baseColor,percentage * 100);
            }
        }

        internal int PowValue => (int) Math.Pow(powBase, value);

        internal int nextValue() {
            value++;
            return PowValue;
        }
        
    }
}
