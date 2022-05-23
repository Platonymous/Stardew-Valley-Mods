using BmFont;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PyTK.PlatoUI
{
    public class UIHelper
    {
        private static Texture2D _plain;
        private static Texture2D _dark;
        private static Texture2D _yellow;
        private static Texture2D _yellowBox;
        private static Texture2D _yellowComb1;
        private static Texture2D _yellowComb2;
        private static Texture2D _tab;
        private static Texture2D _hourglas;
        private static Texture2D _arrowUp;
        private static Texture2D _arrowRight;
        private static Texture2D _arrowDown;
        private static Texture2D _arrowLeft;
        private static Texture2D _whitebubble;
        private static Texture2D _bounceClose;

        public static UIElement BaseHud = UIElement.GetContainer("BaseHud");

        public static UIElement BaseGame
        {
            get
            {
                if (Game1.currentMinigame is PlatoUIGame g)
                    return g.BaseMenu;
                else
                    return null;
            }
        }


        public static UIElement BaseMenu
        {
            get
            {
                if (Game1.activeClickableMenu is PlatoUIMenu m)
                    return m.BaseMenu;
                else
                    return null;
            }
        }

        public static Texture2D PlainTheme
        {
            get
            {
                if (_plain == null)
                    _plain = PyTK.PyDraw.getRectangle(3,3, Color.White);

                return _plain;
            }
        }

        public static Texture2D DarkTheme
        {
            get
            {
                if (_dark == null)
                    _dark = PyTK.PyTKMod._helper.GameContent.Load<Texture2D>("LooseSprites/Cursors").getArea(new Rectangle(473,36,24,24));

                return _dark;
            }
        }

        public static Texture2D TabTheme
        {
            get
            {
                if (_tab == null)
                    _tab = PyTK.PyTKMod._helper.GameContent.Load<Texture2D>("LooseSprites/Cursors").getArea(new Rectangle(16, 368, 16, 16));

                return _tab;
            }
        }

        public static Texture2D YellowTheme
        {
            get
            {
                if (_yellow == null)
                    _yellow = PyTKMod._helper.GameContent.Load<Texture2D>("LooseSprites/DialogBoxGreen").getArea(new Rectangle(16, 16, 160, 160));

                return _yellow;
            }
        }

        public static Texture2D YellowbBoxTheme
        {
            get
            {
                if (_yellowBox == null)
                    _yellowBox = PyTKMod._helper.GameContent.Load<Texture2D>("LooseSprites/Cursors").getArea(new Rectangle(293, 360, 24, 24));

                return _yellowBox;
            }
        }

        public static Texture2D YellowCombBoxesLeftTheme
        {
            get
            {
                if (_yellowComb1 == null)
                    _yellowComb1 = PyTKMod._helper.GameContent.Load<Texture2D>("LooseSprites/Cursors").getArea(new Rectangle(293, 360, 18, 24));

                return _yellowComb1;
            }
        }

        public static Texture2D YellowCombBoxesRightTheme
        {
            get
            {
                if (_yellowComb2 == null)
                    _yellowComb2 = PyTKMod._helper.GameContent.Load<Texture2D>("LooseSprites/Cursors").getArea(new Rectangle(311, 360, 18, 24));

                return _yellowComb2;
            }
        }

        public static Texture2D BounceClose
        {
            get
            {
                if (_bounceClose == null)
                    _bounceClose = new AnimatedTexture2D(PyTKMod._helper.GameContent.Load<Texture2D>("LooseSprites/Cursors").getArea(new Rectangle(289, 342, 121, 12)),11,12,12,true,true,1);

                return _bounceClose;
            }
        }

        public static Texture2D WhiteBubbly
        {
            get
            {
                if (_whitebubble == null)
                    _whitebubble = PyTKMod._helper.GameContent.Load<Texture2D>("LooseSprites/Cursors").getArea(new Rectangle(111, 1870, 54, 49));

                return _whitebubble;
            }
        }

        public static Texture2D Houreglas
        {
            get
            {
                if (_hourglas == null)
                    _hourglas = PyTKMod._helper.GameContent.Load<Texture2D>("LooseSprites/Cursors").getArea(new Rectangle(16, 0, 10, 16));

                return _hourglas;
            }
        }

        public static Texture2D ArrowUp
        {
            get
            {
                if (_arrowUp == null)
                    _arrowUp = PyTKMod._helper.GameContent.Load<Texture2D>("LooseSprites/Cursors").getArea(new Rectangle(76, 72, 40, 44));

                return _arrowUp;
            }
        }

        public static Texture2D ArrowDown
        {
            get
            {
                if (_arrowDown == null)
                    _arrowDown = PyTKMod._helper.GameContent.Load<Texture2D>("LooseSprites/Cursors").getArea(new Rectangle(12, 76, 40, 44));

                return _arrowDown;
            }
        }

        public static Texture2D ArrowRight
        {
            get
            {
                if (_arrowRight == null)
                    _arrowRight = PyTKMod._helper.GameContent.Load<Texture2D>("LooseSprites/Cursors").getArea(new Rectangle(12, 204, 44, 40));

                return _arrowRight;
            }
        }

        public static Texture2D ArrowLeft
        {
            get
            {
                if (_arrowLeft == null)
                    _arrowLeft = PyTKMod._helper.GameContent.Load<Texture2D>("LooseSprites/Cursors").getArea(new Rectangle(8, 268, 44, 40));

                return _arrowLeft;
            }
        }

        public static void AddToHud(UIElement element)
        {
            BaseHud.Add(element.WithBase(BaseHud));
        }

        public static void RemoveFromHud(UIElement element)
        {
            BaseHud.Remove(element);
        }

        public static void RemoveFromHud(string id)
        {
            if (BaseMenu.GetElementById(id) is UIElement element)
                BaseHud.Remove(element);
        }

        public static void RemoveTypesFromHud(bool any, params string[] type)
        {
            foreach (UIElement element in BaseMenu.GetElementsByType(any, type))
                BaseHud.Remove(element);
        }

        public static PlatoUIMenu OpenMenu(string id, UIElement element, bool clone = false, Texture2D background = null, Color? backgroundColor = null, bool movingBackground = false)
        {
            var m = new PlatoUIMenu(id, element, clone, background, backgroundColor, movingBackground);
            Game1.activeClickableMenu = m;
            return m;
        }

        public static void DrawHud(SpriteBatch b, bool before = false)
        {
            foreach (UIElement child in BaseHud.Children.Where(c => (c.Z < 0 && before) || (c.Z >= 0 && !before)))
                DrawElement(b, child);
        }

        public static void DrawElement(SpriteBatch b, UIElement element, float opacity = 1f)
        {
            element.PerfromDrawAction(b);

            foreach (UIElement child in element.Children.OrderBy(c => c.Z).Where(c => c.Z < 0))
                DrawElement(b, child, element.Opacity * opacity);

            if (!element.IsContainer && element.Visible)
            {
                if (element.Theme != null && !element.OutOfBounds)
                {
                    if (!element.Tiled)
                        b.Draw(element.Theme is ScaledTexture2D s ? s.STexture : element.Theme, element.Bounds, element.SourceRectangle, element.Color * element.Opacity * opacity,element.Rotation,element.Origin,element.SpriteEffects,0);
                    else
                        DrawTiled(b, element, opacity);
                }

                if (element is UITextElement text)
                {
                    string t = text.GetText();
                    if (t != null && t != "")
                    {
                        if (text.FontId == "")
                            b.DrawString(text.Font, text.Text, new Vector2(element.Bounds.X, element.Bounds.Y), text.TextColor * element.Opacity * opacity, 0f, Vector2.Zero, text.Scale, SpriteEffects.None, 0);
                        else
                        {
                            UIFontRenderer.DrawText(text.FontId, b, element.Bounds.X, element.Bounds.Y, text.Text, text.TextColor * element.Opacity * opacity, text.Scale, 0f, Vector2.Zero);
                        }
                    }
                }
            }

            if(!element.OutOfBounds)
                foreach (UIElement child in element.Children.OrderBy(c => c.Z).Where(c => c.Z >=0))
                    DrawElement(b, child, element.Opacity * opacity);
        }

        private static void DrawTiled(SpriteBatch b, UIElement element, float opacity = 1f)
        {
            Texture2D texture = element.Theme is ScaledTexture2D s ? s.STexture : element.Theme;

            if (element.SourceRectangle.HasValue)
            {
                texture = texture.getArea(element.SourceRectangle.Value);
                element.Theme = texture;
                element.SourceRectangle = null;
            }

            int tileSource = (int)(element.TileSize * element.TileScale);
            var topLeft = new Rectangle(0, 0, tileSource, tileSource);
            var top = new Rectangle(tileSource, 0, tileSource, tileSource);
            var topRight = new Rectangle(tileSource * 2, 0, tileSource, tileSource);
            var centerLeft = new Rectangle(0, tileSource, tileSource, tileSource);
            var center = new Rectangle(tileSource, tileSource, tileSource, tileSource);
            var centerRight = new Rectangle(tileSource * 2, tileSource, tileSource, tileSource);
            var bottomLeft = new Rectangle(0, tileSource * 2, tileSource, tileSource);
            var bottom = new Rectangle(tileSource, tileSource * 2, tileSource, tileSource);
            var bottomRight = new Rectangle(tileSource * 2, tileSource*2, tileSource, tileSource);

            Rectangle bounds = element.Bounds;

            int tilesWide = bounds.Width / element.TileSize;
            int tilesHeight = bounds.Height / element.TileSize;
            var pos = new Vector2(bounds.X, bounds.Y);

            for (int x = 0; x < tilesWide; x++)
                for (int y = 0; y < tilesHeight; y++)
                {
                    var source = center;

                    if(element.Bordered)
                    {
                        if (x == 0)
                        {
                            if (y == 0)
                                source = topLeft;
                            else if (y == tilesHeight - 1)
                                source = bottomLeft;
                            else
                                source = centerLeft;
                        }
                        else if (y == 0)
                        {
                            if (x == tilesWide - 1)
                                source = topRight;
                            else
                                source = top;
                        }
                        else if (x == tilesWide - 1)
                        {
                            if (y == tilesHeight - 1)
                                source = bottomRight;
                            else
                                source = centerRight;
                        }
                        else if (y == tilesHeight - 1)
                            source = bottom;
                        else
                            source = center;
                    }


                    var position = pos + (new Vector2(x, y) * element.TileSize);
                    b.Draw(texture, new Rectangle((int)position.X,(int)position.Y,element.TileSize, element.TileSize), source, element.Color * element.Opacity * opacity);
                }


        }

        public static Rectangle Fill(UIElement element, UIElement parent)
        {
            if (parent == null)
            {
                Rectangle r = new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height);
                return r;
            }

            Rectangle p = parent.Bounds;
            return new Rectangle(p.X, p.Y, p.Width, p.Height);
        }

        internal static object[] GetObjArray(Rectangle rectangle)
        {
            return new object[4] { rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height };
        }

        internal static int GetAbs(object[] values,int index, int basevalue)
        {
            object v = values.Length > index && values[index] != null ? values[index] : (int)0;

            bool asInt = !(v is float);

            float value = asInt ? (float) (int) v : (float) v;

            return (int)(asInt ? value : (value * basevalue));
        }

        internal static int GetAbs(object value, int basevalue)
        {
            return GetAbs(new object[] { value },0, basevalue);
        }

        internal static Point GetSize(UIElement t, UIElement p, params object[] rectangle)
        {
            int rwidth = t is UITextElement tx ? tx.TextSize.X : GetAbs(rectangle, 2, p.Bounds.Width);
            int rheight = t is UITextElement ty ? ty.TextSize.Y : GetAbs(rectangle, 3, p.Bounds.Height);

            if (t.Theme != null)
            {
                var w = t.SourceRectangle.HasValue ? t.SourceRectangle.Value.Width : t.Theme.Width;
                var h = t.SourceRectangle.HasValue ? t.SourceRectangle.Value.Height : t.Theme.Height;

                if (rwidth == 0 && rheight == 0)
                {
                    rwidth = w;
                    rheight = h;
                }

                if (rheight == 0 && rwidth != 0)
                {
                    float s = (float)rwidth / (float)w;
                    rheight = (int)(s * h);
                }

                if (rheight != 0 && rwidth == 0)
                {
                    float s = (float)rheight / (float)h;
                    rwidth = (int)(s * w);
                }
            }
            return new Point(rwidth, rheight);
        }

        public static Func<UIElement, UIElement, Rectangle> GetFixed(params object[] rectangle)
        {
            return (t, p) =>
            {
                var size = GetSize(t, p, rectangle);
                int rwidth = size.X;
                int rheight = size.Y;
                int rx = GetAbs(rectangle, 0, p.Bounds.Width);
                int ry = GetAbs(rectangle, 1, p.Bounds.Height);

                return new Rectangle(p.Bounds.X + rx, p.Bounds.Y + ry, rwidth, rheight);
            };
        }

        public static Func<UIElement, UIElement, Rectangle> GetAttachedHorizontal(UIElement element, bool xLeft, int yPos, Func<UIElement, UIElement, Rectangle> positioner)
        {
            return (t, p) =>
            {
                var rectangle = UIHelper.GetObjArray(positioner.Invoke(t, p));
                t.AttachedToElement = t.AttachedToElement == null ? element : t.AttachedToElement;
                var size = GetSize(t, p, rectangle);
                int rwidth = size.X;
                int rheight = size.Y;
                int rx = GetAbs(rectangle, 0, p.Bounds.Width);
                int ry = GetAbs(rectangle, 1, p.Bounds.Height);

                int xAdjust = xLeft ? -1 * rwidth : t.AttachedToElement.Bounds.Width;
                int yAdjust = yPos == 0 ? (t.AttachedToElement.Bounds.Height - rheight) / 2 : yPos > 0 ? (t.AttachedToElement.Bounds.Height - rheight) : 0;

                return new Rectangle(t.AttachedToElement.Bounds.X + rx + xAdjust, t.AttachedToElement.Bounds.Y + ry + yAdjust, rwidth, rheight);
            };
        }


        public static Func<UIElement, UIElement, Rectangle> GetAttachedHorizontal(UIElement element, bool xLeft, int yPos, params object[] rectangle)
        {

            return (t, p) =>
            {
                t.AttachedToElement = t.AttachedToElement == null ? element : t.AttachedToElement;
                var size = GetSize(t, p, rectangle);
                int rwidth = size.X;
                int rheight = size.Y;
                int rx = GetAbs(rectangle, 0, p.Bounds.Width);
                int ry = GetAbs(rectangle, 1, p.Bounds.Height);

                int xAdjust = xLeft ? -1 * rwidth : t.AttachedToElement.Bounds.Width;
                int yAdjust = yPos == 0 ? (t.AttachedToElement.Bounds.Height - rheight) / 2 : yPos > 0 ? (t.AttachedToElement.Bounds.Height - rheight) : 0;

                return new Rectangle(t.AttachedToElement.Bounds.X + rx + xAdjust, t.AttachedToElement.Bounds.Y + ry + yAdjust, rwidth, rheight);
            };
        }

        public static Func<UIElement, UIElement, Rectangle> GetAttachedVertical(UIElement element, bool yTop, int xPos, params object[] rectangle)
        {
            return (t, p) =>
            {
                t.AttachedToElement = t.AttachedToElement == null ? element : t.AttachedToElement;
                var size = GetSize(t, p, rectangle);
                int rwidth = size.X;
                int rheight = size.Y;
                int rx = GetAbs(rectangle, 0, p.Bounds.Width);
                int ry = GetAbs(rectangle, 1, p.Bounds.Height);

                int xAdjust = xPos == 0 ? (t.AttachedToElement.Bounds.Width - rwidth) / 2 : xPos > 0 ? (t.AttachedToElement.Bounds.Width - rwidth) : 0;
                int yAdjust = yTop ? -1 * rheight : t.AttachedToElement.Bounds.Height;

                return new Rectangle(t.AttachedToElement.Bounds.X + rx + xAdjust, t.AttachedToElement.Bounds.Y + ry + yAdjust, rwidth, rheight);
            };
        }

        public static Func<UIElement, UIElement, Rectangle> GetAttachedVertical(UIElement element, bool yTop, int xPos, Func<UIElement, UIElement, Rectangle> positioner)
        {
            return (t, p) =>
            {
                var rectangle = UIHelper.GetObjArray(positioner.Invoke(t, p));
                t.AttachedToElement = t.AttachedToElement == null ? element : t.AttachedToElement;
                var size = GetSize(t, p, rectangle);
                int rwidth = size.X;
                int rheight = size.Y;
                int rx = GetAbs(rectangle, 0, p.Bounds.Width);
                int ry = GetAbs(rectangle, 1, p.Bounds.Height);

                int xAdjust = xPos == 0 ? (t.AttachedToElement.Bounds.Width - rwidth) / 2 : xPos > 0 ? (t.AttachedToElement.Bounds.Width - rwidth) : 0;
                int yAdjust = yTop ? -1 * rheight : t.AttachedToElement.Bounds.Height;

                return new Rectangle(t.AttachedToElement.Bounds.X + rx + xAdjust, t.AttachedToElement.Bounds.Y + ry + yAdjust, rwidth, rheight);
            };
        }

        public static Func<UIElement, UIElement, Rectangle> GetAttachedDiagonal(UIElement element, bool xLeft, bool up, params object[] rectangle)
        {
            return (t, p) =>
            {
                t.AttachedToElement = t.AttachedToElement == null ? element : t.AttachedToElement;
                var size = GetSize(t, p, rectangle);
                int rwidth = size.X;
                int rheight = size.Y;
                int rx = GetAbs(rectangle, 0, p.Bounds.Width);
                int ry = GetAbs(rectangle, 1, p.Bounds.Height);

                int xAdjust = xLeft ? -1 * rwidth : t.AttachedToElement.Bounds.Width;
                int yAdjust = xLeft ? up ? t.AttachedToElement.Bounds.Height : -1 * rheight : up ? -1 * rheight : t.AttachedToElement.Bounds.Height;
                yAdjust = xLeft == up ? t.AttachedToElement.Bounds.Height : -1 * rheight;

                return new Rectangle(t.AttachedToElement.Bounds.X + rx + xAdjust, t.AttachedToElement.Bounds.Y + ry + yAdjust, rwidth, rheight);
            };
        }

        public static Func<UIElement, UIElement, Rectangle> GetViewport()
        {
            return (t, p) =>
            {
                int rwidth = Game1.viewport.Width;
                int rheight = Game1.viewport.Height;
                int x = 0;
                int y = 0;
                return new Rectangle(x, y, rwidth, rheight);
            };
        }

        public static Func<UIElement, UIElement, Rectangle> GetCentered(params object[] rectangle)
        {
            return (t, p) =>
            {
                var size = GetSize(t, p, rectangle);
                int rwidth = size.X;
                int rheight = size.Y;
                int x = ((p.Bounds.Width - rwidth) / 2) + GetAbs(rectangle, 0, p.Bounds.Width);
                int y = ((p.Bounds.Height - rheight) / 2) + GetAbs(rectangle, 1, p.Bounds.Height);
                return new Rectangle(p.Bounds.X + x, p.Bounds.Y+ y, rwidth, rheight);
            };
        }

        public static Func<UIElement, UIElement, Rectangle> GetTopLeft(params object[] rectangle)
        {
            return GetFixed(rectangle);
        }

        public static Func<UIElement, UIElement, Rectangle> GetTopCenter(params object[] rectangle)
        {
            return (t, p) =>
            {
                var size = GetSize(t, p, rectangle);
                int rwidth = size.X;
                int rheight = size.Y;
                int rx = ((p.Bounds.Width - rwidth) / 2) + GetAbs(rectangle, 0, p.Bounds.Width);
                int ry = GetAbs(rectangle, 1, p.Bounds.Height);
                return new Rectangle(p.Bounds.X + rx, p.Bounds.Y + ry, rwidth, rheight);
            };
        }

        public static Func<UIElement, UIElement, Rectangle> GetTopRight(params object[] rectangle)
        {
            return (t, p) =>
            {
                var size = GetSize(t, p, rectangle);
                int rwidth = size.X;
                int rheight = size.Y;
                int rx = (p.Bounds.Width - rwidth) + GetAbs(rectangle, 0, p.Bounds.Width);
                int ry = GetAbs(rectangle, 1, p.Bounds.Height);
                return new Rectangle(p.Bounds.X + rx , p.Bounds.Y + ry, rwidth, rheight);
            };
        }

        public static Func<UIElement, UIElement, Rectangle> GetBottomLeft(params object[] rectangle)
        {
            return (t, p) =>
            {
                var size = GetSize(t, p, rectangle);
                int rwidth = size.X;
                int rheight = size.Y;
                int rx = GetAbs(rectangle, 0, p.Bounds.Width);
                int ry = (p.Bounds.Height - rheight) + GetAbs(rectangle, 1, p.Bounds.Height);
                return new Rectangle(p.Bounds.X + rx, p.Bounds.Y + ry, rwidth, rheight);
            };
        }

        public static Func<UIElement, UIElement, Rectangle> GetBottomCenter(params object[] rectangle)
        {
            return (t, p) =>
            {
                var size = GetSize(t, p, rectangle);
                int rwidth = size.X;
                int rheight = size.Y;
                int rx = ((p.Bounds.Width - rwidth)/2) + GetAbs(rectangle, 0, p.Bounds.Width);
                int ry = (p.Bounds.Height - rheight) + GetAbs(rectangle, 1, p.Bounds.Height);
                return new Rectangle(p.Bounds.X + rx, p.Bounds.Y + ry, rwidth, rheight);
            };
        }


        public static Func<UIElement, UIElement, Rectangle> GetBottomRight(params object[] rectangle)
        {
            return (t, p) =>
            {
                var size = GetSize(t, p, rectangle);
                int rwidth = size.X;
                int rheight = size.Y;
                int rx = (p.Bounds.Width - rwidth) + GetAbs(rectangle, 0, p.Bounds.Width);
                int ry = (p.Bounds.Height - rheight) + GetAbs(rectangle, 1, p.Bounds.Height);
                return new Rectangle(p.Bounds.X + rx, p.Bounds.Y + ry, rwidth, rheight);
            };
        }



        public static Func<UIElement, UIElement, Rectangle> GetGridPositioner(int partitionX = 2, int partitionY = 1, int x = 0, int y = 0, int width = 1, int height = 1, int marginX = 0, int marginY = 0)
        {
            return (t, e) =>
            {
                int w = (e.Bounds.Width - (marginX * (partitionX + 1))) / partitionX;
                int h = (e.Bounds.Height - (marginY * (partitionY + 1))) / partitionY;
                int rx = marginX + x * (w + marginX);
                int ry = marginY + y * (h + marginY);
                return new Rectangle(e.Bounds.X + rx, e.Bounds.Y + ry, w * width + (marginX * (width - 1)), h * height + (marginY * (height - 1)));
            };
        }
    }
}
