using Artista.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;

namespace Artista.Menu
{
    public class SelectCanvasMenu : IClickableMenu
    {
        private IModHelper Helper { get; }
        private IMonitor Monitor { get; }

        private Texture2D Curtain { get; set; } = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
        private Easel Easel { get; set; } = null;

        private Rectangle? OldBounds { get; set; } = null;

            private List<SizeChoice> Choices { get; } = new List<SizeChoice>();

        public SelectCanvasMenu(Easel easal, IModHelper helper, IMonitor monitor)
       : base(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height, true)
        {
            Easel = easal;
            Helper = helper;
            Monitor = monitor;
            SetBounds(new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height));

        }

        private void SetBounds(Rectangle viewport)
        {
            if (!OldBounds.HasValue || OldBounds.Value != viewport)
            {
               
                Color[] color2 = new Color[1] { Color.Black };

                Curtain.SetData(color2);
                OldBounds = viewport;
                upperRightCloseButton = new ClickableTextureComponent(new Rectangle(viewport.Right - 50, viewport.Top - 50, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);

                Rectangle last = new Rectangle((viewport.Width - 480) / 2, ((viewport.Height - 340) / 2) - 80, 200, 60);

                Choices.Add(new SizeChoice(1, 1, 1));
                Choices.Add(new SizeChoice(1, 2, 1));
                Choices.Add(new SizeChoice(2, 2, 1));
                Choices.Add(new SizeChoice(1, 1, 2));
                Choices.Add(new SizeChoice(1, 2, 2));
                Choices.Add(new SizeChoice(2, 2, 2));

                int c = 0;
                foreach (var size in Choices)
                {
                    if(c == 3)
                        last = new Rectangle(last.Left + 280, ((viewport.Height - 340) / 2) - 80, last.Width, last.Height);

                    size.Rectangle = new Rectangle(last.Left, last.Bottom + 80, last.Width, last.Height);
                    last = size.Rectangle;
                    c++;
                }
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            foreach (var size in Choices)
            {
                if (size.Rectangle.Contains(x, y))
                {
                    var art = size.Art;
                    Easel.SetArt(art);
                    exitThisMenu();
                    Game1.activeClickableMenu = new PaintMenu(art, Easel, Helper, Monitor);
                    return;
                }
            }
            base.releaseLeftClick(x, y);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            SetBounds(newBounds);
            base.gameWindowSizeChanged(oldBounds, newBounds);
        }

        private void drawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
        {
            b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos, boxWidth, boxHeight), new Rectangle(306, 320, 16, 16), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 20, boxWidth, 24), new Rectangle(275, 313, 1, 6), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth - 20, 32), new Rectangle(275, 328, 1, 8), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle(264, 325, 8, 1), Color.White);
            b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle(293, 324, 7, 1), Color.White);
            b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos - 28), new Rectangle(261, 311, 14, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
            b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos - 28), new Rectangle(291, 311, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
            b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos + boxHeight - 8), new Rectangle(291, 326, 12, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
            b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos + boxHeight - 4), new Rectangle(261, 327, 14, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
        }

        public override void draw(SpriteBatch b)
        {
            if (!OldBounds.HasValue || OldBounds.Value.Width != Game1.viewport.Width || OldBounds.Value.Height != Game1.viewport.Height)
                return;

            drawBackground(b);
            foreach (var size in Choices)
                drawBox(b, size.Rectangle.Left, size.Rectangle.Top, size.Rectangle.Width, size.Rectangle.Height);

            foreach (var size in Choices)
                Utility.drawTextWithColoredShadow(b, size.Text, Game1.dialogueFont, new Vector2(size.Rectangle.Left + 20, size.Rectangle.Top + 10), Color.Maroon, Color.DarkGoldenrod * 0.35f);

            base.draw(b);
            drawMouse(b);
        }

        public override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
        {
            draw(b);
        }

        public override bool shouldDrawCloseButton()
        {
            return true;
        }

        public override void drawBackground(SpriteBatch b)
        {
            var viewport = new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height);

            b.Draw(Curtain, viewport, Color.White * 0.6f);

        }
    }
}
