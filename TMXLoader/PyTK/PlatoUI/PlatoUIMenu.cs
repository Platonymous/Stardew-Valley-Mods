using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;

namespace TMXLoader
{
    public class PlatoUIMenu : IClickableMenu
    {
        public virtual UIElement BaseMenu { get; set; }
        public virtual Texture2D Background { get; set; }
        public virtual Color BackgroundColor { get; set; } = Color.White;
        protected int BackgroundPos = 0;
        protected virtual bool BackgroundIsMoving { get; set; } = false;

        public virtual Point LastMouse { get; set; } = Point.Zero;

        protected virtual Action<SpriteBatch> BeforeDrawAction { get; set; } = null;
        protected virtual Action<SpriteBatch> AfterDrawAction { get; set; } = null;


        public virtual string Id { get; set; }

        private float lastUIZoom = 1f;

        public PlatoUIMenu(string id, UIElement element, bool clone = false, Texture2D background = null, Color? backgroundColor = null, bool movingBackground = false)
            :base(0,0,Game1.viewport.Width,Game1.viewport.Height,false)
        {
#if ANDROID

#else
            lastUIZoom = Game1.options.desiredUIScale;
            Game1.options.desiredUIScale = Game1.options.desiredBaseZoomLevel;
            TMXLoaderMod.helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked; ;
#endif

            if (backgroundColor.HasValue)
                BackgroundColor = backgroundColor.Value;
            BackgroundIsMoving = movingBackground;
            Background = background;
            Id = id;
            BaseMenu = UIElement.GetContainer("CurrentMenu");
            if(element != null)
                BaseMenu.Add(clone ? element.Clone().WithBase(BaseMenu) : element.WithBase(BaseMenu));

            BaseMenu.UpdateBounds();
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
#if ANDROID

#else
            if (!(Game1.activeClickableMenu is PlatoUIMenu))
            {
                Game1.options.desiredUIScale = lastUIZoom;
                TMXLoaderMod.helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
            }
#endif
        }

        public virtual void ClearMenu(UIElement element = null)
        {
            BaseMenu.Clear(element);
        }

        public override void draw(SpriteBatch b)
        {
            BeforeDrawAction?.Invoke(b);
            this.drawBackground(b);
            UIHelper.DrawElement(b, BaseMenu);
            this.drawMouse(b);
            AfterDrawAction?.Invoke(b);
        }

        public override void drawBackground(SpriteBatch b)
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
        
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.xPositionOnScreen = 0;
            this.yPositionOnScreen = 0;
            this.width = newBounds.Width;
            this.height = newBounds.Height;

            UIElement.Viewportbase.UpdateBounds();
            BaseMenu.UpdateBounds();
        }

        public override void performHoverAction(int x, int y)
        {
            BaseMenu.PerformHover(new Point(x, y));
        }

        public override void update(GameTime time)
        {
            if (time.TotalGameTime.Ticks % 3 == 0)
                BackgroundPos--;

            Point m = new Point(Game1.getMouseX(), Game1.getMouseY());

            if (m != LastMouse)
            {
                LastMouse = m;
                BaseMenu.PerformMouseMove(m);
            }

            BaseMenu.PerformUpdate(time);
        }
        
        public override void receiveKeyPress(Keys key)
        {
            BaseMenu.PerformKey(key, false);
            base.receiveKeyPress(key);
        }

        public override void releaseLeftClick(int x, int y)
        {
            BaseMenu.PerformClick(new Point(x, y),false,true, false);
        }

        public override void receiveScrollWheelAction(int direction)
        {
            BaseMenu.PerformScroll(direction);
            base.receiveScrollWheelAction(direction);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            BaseMenu.PerformClick(new Point(x, y),false, false, false);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            BaseMenu.PerformClick(new Point(x, y), true, false, false);
        }

        public override void leftClickHeld(int x, int y)
        {
            BaseMenu.PerformClick(new Point(x, y), false, false, true);
        }

        public override bool readyToClose()
        {
            return base.readyToClose();
        }
    }
}
