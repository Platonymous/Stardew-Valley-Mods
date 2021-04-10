using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using PlatoTK;
using System.Linq;

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
            var plato = SnakeMinigame.Helper.GetPlatoHelper();
            DrawTexture = plato.Content.Textures.ExtractTile(Game1.objectSpriteSheet, Index);
            Shadow = new Texture2D(Game1.graphics.GraphicsDevice, DrawTexture.Width, DrawTexture.Height);
            Color[] data = new Color[Shadow.Width * Shadow.Height];
            Shadow.GetData<Color>(data);

            for (int i = 0; i < data.Length; i++)
                data[i] = data[i].A != 0 ? Color.Black : data[i];

            Shadow.SetData(data);

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
