using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Snake.SnakeMinigame;

namespace Snake
{
    public class SnakeObject
    {
        public Vector2 position;
        public Point Drawposition { get; set; }
        public Color DrawColor { get; set; }
        public SnakeMinigame GameInstance { get; set; }
        public Direction Facing { get; set; }
        public Point Size { get; set; }
        public SnakeObject Child { get; set; }
        public Texture2D DrawTexture { get; set; }

        public SnakeObject(Vector2 position, SnakeMinigame gameInstance)
        {
            this.position = position;
            GameInstance = gameInstance;
            DrawColor = Color.White;
            Resize();
        }

        public virtual void Resize()
        {
            int height = (int)Math.Floor(GameInstance.Board.Size.Y * 2f / (GameInstance.TiledSize.Y + 1));
            Size = new Point((int) Math.Floor(((float) GameInstance.SpriteSize.X / GameInstance.SpriteSize.Y) * height), height);
        }

        public virtual void Reposition()
        {
            Drawposition = GetDrawPosition();
        }

        public virtual Point GetDrawPosition()
        {
            return GameInstance.Board.GetDrawPosition(position, Size);
        }

        public virtual Point GetNextBoxCoordinates()
        {
            Vector2 pos = position;
            Point nextBox;
            switch (Facing)
            {
                case Direction.DOWN: nextBox = GetBoxPosition(new Vector2(pos.X, pos.Y + 1f)); break;
                case Direction.UP: nextBox = GetBoxPosition(new Vector2(pos.X, pos.Y - 1f)); break;
                case Direction.LEFT: nextBox = GetBoxPosition(new Vector2(pos.X - 1f, pos.Y)); break;
                default: nextBox = GetBoxPosition(new Vector2(pos.X + 1f, pos.Y)); break;
            }

            return nextBox;
        }

        public virtual void Turn(Direction direction)
        {
            Point box = GetBoxPosition();
            var boxC = GetNextBoxCoordinates();
            if (GameInstance.Board.Turns.ContainsKey(boxC))
                GameInstance.Board.Turns[boxC] = direction;
            else
                GameInstance.Board.Turns.Add(boxC, direction);

        }

        public virtual void HandleOutOfBounds()
        {
            if (position.X < 0)
                position.X = GameInstance.TiledSize.X;
            if (position.X > GameInstance.TiledSize.X)
                position.X = 0;
            if (position.Y< 0)
                position.Y = GameInstance.TiledSize.Y;
            if (position.Y > GameInstance.TiledSize.Y)
                position.Y = 0;
        }

        public Point GetBoxPosition()
        {
            return new Point((int)Math.Ceiling(position.X), (int)Math.Ceiling(position.Y));
        }

        public Point GetBoxPosition(Vector2 pos)
        {
            return new Point((int)Math.Ceiling(pos.X), (int)Math.Ceiling(pos.Y));
        }

        public virtual void Next()
        {
            MoveForward();

            Point box = GetBoxPosition();
            if (GameInstance.Board.Turns.ContainsKey(box))
            {
                Facing = GameInstance.Board.Turns[box];
                if (Child == null)
                    GameInstance.Board.Turns.Remove(box);
            }

            HandleOutOfBounds();

            if(Child != null)
                Child.Next();
        }

        public virtual void MoveForward()
        {
            switch (Facing)
            {
                case Direction.DOWN: position = new Vector2(position.X, (float)Math.Round(position.Y + GameInstance.StepLength,1)); break;
                case Direction.UP: position = new Vector2(position.X, (float) Math.Round(position.Y - GameInstance.StepLength,1)); break;
                case Direction.LEFT: position = new Vector2((float)Math.Round(position.X - GameInstance.StepLength,1), position.Y); break;
                default: position = new Vector2((float)Math.Round(position.X + GameInstance.StepLength,1), position.Y); break;
            }
        }

        public virtual void Update(int time)
        {
            Next();
        }

        public virtual void Draw(SpriteBatch b)
        {
            Reposition();
        }

        public virtual void CollideWith(SnakeObject obj)
        {
            /*if (obj != this && obj is SnakesHead && Child != obj)
                GameInstance.SetupBoard();*/
        }

        public virtual void Destroy()
        {
            GameInstance.Board.Remove(this);
        }

    }
}
