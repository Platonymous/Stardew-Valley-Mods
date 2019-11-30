using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Minigames;
using System;

namespace PyTK.PlatoUI
{
    class PlatoUIGame : IMinigame
    {
        public virtual bool Quit { get; set; } = false;
        public virtual UIElement BaseMenu { get; set; }
        public virtual Texture2D Background { get; set; }
        public virtual Color BackgroundColor { get; set; } = Color.White;
        protected int BackgroundPos = 0;
        protected virtual bool BackgroundIsMoving { get; set; } = false;
        public virtual string Id { get; set; }

        public virtual bool DrawMouse { get; set; } = true;

        public virtual bool QuitOnESC { get; set; } = true;

        public virtual bool DoMainGameUpdate { get; set; } = false;

        public virtual bool OverrideFreeMouseMovement { get; set; } = false;

        public virtual Point LastMouse { get; set; } = Point.Zero;

        public PlatoUIGame(string id, UIElement element, bool drawMouse = true, bool quitOnESC = true, bool clone = false, Texture2D background = null, Color? backgroundColor = null, bool movingBackground = false)
        {
            Id = id;
            DrawMouse = drawMouse;
            QuitOnESC = quitOnESC;
            if (backgroundColor.HasValue)
                BackgroundColor = backgroundColor.Value;
            BackgroundIsMoving = movingBackground;
            Background = background;
            Id = id;
            BaseMenu = UIElement.GetContainer("CurrentMinigame");
            if (element != null)
                BaseMenu.Add(clone ? element.Clone().WithBase(BaseMenu) : element.WithBase(BaseMenu));

            UIElement.Viewportbase.UpdateBounds();
            BaseMenu.UpdateBounds();
        }

        public virtual void changeScreenSize()
        {
            BaseMenu.UpdateBounds();
        }

        public virtual bool doMainGameUpdates()
        {
            return DoMainGameUpdate;
        }

        public virtual void draw(SpriteBatch b)
        {
            this.drawBackground(b);
            UIHelper.DrawElement(b, BaseMenu);
            if(DrawMouse)
                this.drawMouse(b);
        }

        public virtual void drawBackground(SpriteBatch b)
        {
            if (Background is Texture2D)
            {
                if (BackgroundIsMoving)
                {
                    if (BackgroundPos < 0 - Background.Width)
                        BackgroundPos = 0;
                    for (int x = BackgroundPos; x < Game1.viewport.Width + Background.Width * 2; x += Background.Width)
                        for (int y = BackgroundPos; y < Game1.viewport.Height + Background.Width * 2; y += Background.Width)
                            b.Draw(Background, new Vector2(x, y), BackgroundColor);
                }
                else
                {
                    float scale = Math.Max(Game1.viewport.Width / Background.Width, Game1.viewport.Height / Background.Height);
                    int x = (Game1.viewport.Width - Background.Width) / 2;
                    int y = (Game1.viewport.Height - Background.Height) / 2;
                    b.Draw(Background, new Rectangle(x, y, Math.Max((int)(Background.Width * scale), Game1.viewport.Width), Math.Max((int)(Background.Height * scale), Game1.viewport.Height)), BackgroundColor);
                }
            }
        }

        public virtual void drawMouse(SpriteBatch b)
        {
            b.Draw(Game1.mouseCursors, new Vector2((float)Game1.getMouseX(), (float)Game1.getMouseY()), new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.mouseCursor, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)(4.0 + (double)Game1.dialogueButtonScale / 150.0), SpriteEffects.None, 1f);
        }

        public virtual void leftClickHeld(int x, int y)
        {
            BaseMenu.PerformClick(new Point(x, y), false, false, true);
        }

        public virtual string minigameId()
        {
            return Id;
        }

        public virtual bool overrideFreeMouseMovement()
        {
            return OverrideFreeMouseMovement;
        }

        public virtual void receiveEventPoke(int data)
        {

        }

        public virtual void receiveKeyPress(Keys k)
        {
            BaseMenu.PerformKey(k, false);
        }

        public virtual void receiveKeyRelease(Keys k)
        {
            BaseMenu.PerformKey(k, true);

            if (k == Keys.Escape && QuitOnESC)
                quitGame();
        }

        public virtual void receiveLeftClick(int x, int y, bool playSound = true)
        {
            BaseMenu.PerformClick(new Point(x, y), false, false, false);
        }

        public virtual void receiveRightClick(int x, int y, bool playSound = true)
        {
            BaseMenu.PerformClick(new Point(x, y), true, false, false);

        }

        public virtual void releaseLeftClick(int x, int y)
        {
            BaseMenu.PerformClick(new Point(x, y), false, true, false);
        }

        public virtual void releaseRightClick(int x, int y)
        {
            BaseMenu.PerformClick(new Point(x, y), true, true, false);
        }

        public virtual bool tick(GameTime time)
        {
            BaseMenu.PerformUpdate(time);

            if (UIElement.DragElement != null)
            {
                Point m = new Point(Game1.getMouseX(), Game1.getMouseY());

                if (m != LastMouse)
                {
                    LastMouse = m;
                    BaseMenu.PerformMouseMove(m);
                }
            }

            return Quit;
        }

        public virtual void quitGame()
        {
            Quit = true;
        }

        public virtual void unload()
        {
           
        }

        public bool forceQuit()
        {
            Quit = true;
            return true;
        }
    }
}
