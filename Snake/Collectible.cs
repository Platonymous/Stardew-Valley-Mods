using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Extensions;
using StardewValley;
using System;

namespace Snake
{
    public class Collectible : SnakeObject
    {
        public Texture2D Shadow { get; set; }
        public int Index { get; set; }

        public Collectible(Vector2 position, int index, bool isCollectible, SnakeMinigame gameInstance) : base(position, gameInstance)
        {
            Index = index;
            LoadTextures();
            DrawColor = Color.Yellow;
            Resize();
        }

        public void LoadTextures()
        {
            DrawTexture = Game1.objectSpriteSheet.getTile(Index);
            Shadow = Game1.objectSpriteSheet.getTile(Index).setSaturation(0).setLight(0);
        }

        public override void Resize()
        {
            int height = (int)Math.Floor(GameInstance.Board.Size.Y * 2f / (GameInstance.TiledSize.Y + 1));
            Size = new Point(height, height);
            Reposition();
        }

        public override void Draw(SpriteBatch b)
        {
            base.Draw(b);

            if (GameInstance.hideObjects)
                return;
            b.Draw(Shadow, new Rectangle(Drawposition.X, Drawposition.Y, Size.X, Size.Y), DrawColor * 0.3f);
            b.Draw(DrawTexture, new Rectangle(Drawposition.X, Drawposition.Y - Size.Y / 10, Size.X, Size.Y), DrawColor);
        }

        public override void CollideWith(SnakeObject obj)
        {
            if (obj is SnakesHead sh)
            {
                sh.AddNewTailSegment();
                sh.score += 10;
                Game1.playSound("coin");
            }

            Destroy();
            GameInstance.Board.SpawnCollectible();
        }


    }
}
