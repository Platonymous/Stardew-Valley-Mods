using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Snake.SnakeMinigame;

namespace Snake
{
    public class TailSegment : SnakeObject, ISnakeSegement
    {
        public ISnakeSegement ChildSegment => (Child as ISnakeSegement);
        public SnakeObject Parent { get; set; }

        public TailSegment(SnakeObject parent)
            : base(parent.position, parent.GameInstance)
        {
            Parent = parent;
            Facing = parent.Facing;
            DrawColor = GameInstance.SnakeColor;
            parent.Child = this;
            AdjustPosition();
            DrawTexture = GameInstance.SpriteSheet;
        }

        public void AdjustPosition()
        {
            switch (Facing)
            {
                case Direction.DOWN: position = new Vector2(Parent.position.X, Parent.position.Y - 1.1f); break;
                case Direction.UP: position = new Vector2(Parent.position.X, Parent.position.Y + 1.1f); break;
                case Direction.LEFT: position = new Vector2(Parent.position.X + 1.1f, Parent.position.Y); break;
                default: position = new Vector2(Parent.position.X - 1.1f, Parent.position.Y); break;
            }
        }

        public override void Draw(SpriteBatch b)
        {
            base.Draw(b);

            if (GameInstance.hideObjects)
                return;

            if (Child != null)
                b.Draw(DrawTexture, new Rectangle(Drawposition.X, Drawposition.Y, Size.X, Size.Y), new Rectangle?(new Rectangle(GameInstance.SpriteSize.X * 4, 0, GameInstance.SpriteSize.X, GameInstance.SpriteSize.Y)), GameInstance.Board.GameOver ? Color.Red : DrawColor);
            else
                b.Draw(DrawTexture, new Rectangle(Drawposition.X, Drawposition.Y, Size.X, Size.Y), new Rectangle?(new Rectangle(GameInstance.SpriteSize.X * (Child != null ? (int)Facing : ((int)Facing + 2) > 3 ? (int)Facing - 2 : (int)Facing + 2), 0, GameInstance.SpriteSize.X, GameInstance.SpriteSize.Y)), GameInstance.Board.GameOver ? Color.Red : DrawColor);
        }

        public override void Next()
        {
            base.Next();
        }

        public void AddNewTailSegment()
        {
            if (ChildSegment == null)
                GameInstance.Board.Add(new TailSegment(this));
            else
                ChildSegment.AddNewTailSegment();
        }

    }
}
