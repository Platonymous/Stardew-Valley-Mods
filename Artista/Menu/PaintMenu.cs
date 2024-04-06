using Artista.Artpieces;
using Artista.Objects;
using Artista.Online;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artista.Menu
{

    public class PaintMenu : IClickableMenu
    {
        private IModHelper Helper { get; }

        private IMonitor Monitor { get; }

        public Artpiece Art { get; set; }

        private Rectangle? CanvasRect { get; set; } = null;

        private float CanvasScale { get; set; } = 1;

        private Texture2D CanvasBackground { get; set; } = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);

        private Texture2D Curtain { get; set; } = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);

        private Texture2D ColorPicker { get; set; }
        private Texture2D ColorPickerDetail { get; set; }

        private Rectangle? ColorPickerRect { get; set; } = null;
        private Rectangle? ColorPickerDetailRect { get; set; } = null;

        private Rectangle? ActiveColorRect { get; set; } = null;   

        private Rectangle? ColorPickerBackground { get; set; } = null;

        private Rectangle? OldBounds { get; set; } = null;

        private Color ActiveColor { get; set; } = Color.Red;

        private List<Color> Palette { get; set; } = new List<Color>();
        private List<Rectangle> PaletteRect { get; set; } = new List<Rectangle>();
        private Texture2D PaletteTexture { get; set; } = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);

        private Vector2 ColorPoint { get; set; } = new Vector2(5, 25);
        private Vector2 ColorPoint2 { get; set; } = new Vector2(15, 15);

        private Texture2D Indicator { get; set; } = null;

        private Rectangle? SaveRect { get; set; } = null;

        private Rectangle? ResetRect { get; set; } = null;
        private Rectangle? NewRect { get; set; } = null;
        private Rectangle? FinishRect { get; set; } = null;
        private Rectangle? ShareRect { get; set; } = null;
        private Rectangle? CompeteRect { get; set; } = null;
        private Rectangle? OnlineRect { get; set; } = null;

        private bool Visible { get; set; } = false;

        private bool Started { get; set; } = false;
        private Easel Easel { get; set; } = null;

        public Point? LinePoint { get; set; } = null;

        const int ButtonHeight = 40;
        const int ButtonWidth = 170;
        const int ButtonMargin = 100;
        const int ButtonDistance = 60;
        const int ButtonPadding = 10;

        public bool SettingColorPicker { get; set; } = false;
        public bool SettingColorPickerDetail { get; set; } = false;

        public ListCompetitons Competitons { get; set; }

        public Texture2D OldDetails { get; set; }

        public PaintMenu(Artpiece piece,Easel easal, IModHelper helper, IMonitor monitor) 
        :base(0,0,Game1.uiViewport.Width,Game1.uiViewport.Height,true)
        {
            OldDetails = PaletteTexture;
            Easel = easal;
            Art = piece;
            Art.Refresh();
            Helper = helper;
            Monitor = monitor;
            Competitons = ArtistaMod.Singleton.OnlineApi.GetCompetitions();
            Indicator = GetCircle(100, Color.White);
            SetBounds(new Rectangle(Game1.uiViewport.X,Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height));
            PickColor(0);
        }

        public string GetID()
        {
            return Easel.TileLocation.X + "." + Easel.TileLocation.Y + "." + Game1.currentLocation.Name;
        }

        public void Reverse()
        {
            Art?.Reverse();
        }

        public void ReceivePaintMPInfo(PaintMPInfo info)
        {
            if (info.ID == GetID() && SavedArtpiece.TryParseColorFromString(info.Color, out Color c))
            {
                if (info.Fill)
                    Art?.Fill(info.X, info.Y, c);
                else if (info.Line)
                    Art?.DrawLine(new Point(info.X, info.Y), new Point(info.X2, info.Y2), c);
                else
                    Art?.Paint(info.X, info.Y, c);
            }
        }

        public override void update(GameTime time)
        {
            if (!Visible)
                return;

            var pos = Helper.Input.GetCursorPosition().GetScaledScreenPixels();


            if (!Helper.Input.IsDown(ArtistaMod.Config.LineButton))
            {
                LinePoint = null;
            }

            if (!Helper.Input.IsDown(ArtistaMod.Config.FillButton) && Art.Type == ArtType.Painting && CanvasRect.HasValue && (Helper.Input.IsDown(SButton.MouseLeft) || Helper.Input.IsDown(SButton.MouseRight)) && CanvasRect.Value.Contains(pos.X, pos.Y))
            {
                    int x = (int)((pos.X - CanvasRect.Value.X) / CanvasScale);

                    int y = (int)((pos.Y - CanvasRect.Value.Y) / CanvasScale);
                var c = Helper.Input.IsDown(SButton.MouseRight) ? Color.Transparent : ActiveColor;
                    Art.Paint(x, y, c);

                if (Context.IsMultiplayer)
                    Helper.Multiplayer.SendMessage(new PaintMPInfo { ID = GetID(), Color = SavedArtpiece.ColorToString(c), X = x, Y = y }, "Platonymous.Artista.Paint", new string[] { "Platonymous.Artista" }); ;

                    if(!Palette.Contains(ActiveColor))
                        PopulatePalette();
            }
            else if (Helper.Input.IsDown(SButton.MouseLeft) && ColorPickerDetailRect.HasValue && ColorPickerDetailRect.Value.Contains(pos.X, pos.Y))
            {
                int x1 = (int)(((pos.X - ColorPickerDetailRect.Value.X) * 500f) / ColorPickerDetailRect.Value.Width);
                int y1 = (int)(((pos.Y - ColorPickerDetailRect.Value.Y) * 500f) / ColorPickerDetailRect.Value.Height);

                PickColor(x1, y1, (int)pos.X, (int)pos.Y);
            }

            base.update(time);
        }

        public override void releaseLeftClick(int x, int y)
        {
            if (!Visible)
                return;


            if (Helper.Input.IsDown(ArtistaMod.Config.FillButton) && Art.Type == ArtType.Painting && CanvasRect.HasValue && CanvasRect.Value.Contains(x, y))
            {
                int x1 = (int)((x - CanvasRect.Value.X) / CanvasScale);

                int y1 = (int)((y - CanvasRect.Value.Y) / CanvasScale);
                var c = ActiveColor;
                Art.Fill(x1, y1, c);

                if (Context.IsMultiplayer)
                    Helper.Multiplayer.SendMessage(new PaintMPInfo { ID = GetID(), Color = SavedArtpiece.ColorToString(c), X = x1, Y = y1, Fill = true }, "Platonymous.Artista.Paint", new string[] { "Platonymous.Artista" }); ;

                if (!Palette.Contains(ActiveColor))
                    PopulatePalette();
            }
            else if (Helper.Input.IsDown(ArtistaMod.Config.LineButton) && Art.Type == ArtType.Painting && CanvasRect.HasValue && CanvasRect.Value.Contains(x, y))
            {
                if (!LinePoint.HasValue)
                {
                    int x1 = (int)((x - CanvasRect.Value.X) / CanvasScale);

                    int y1 = (int)((y - CanvasRect.Value.Y) / CanvasScale);

                    LinePoint = new Point(x1, y1);
                }
                else
                {
                    int x1 = (int)((x - CanvasRect.Value.X) / CanvasScale);

                    int y1 = (int)((y - CanvasRect.Value.Y) / CanvasScale);

                    Art?.DrawLine(LinePoint.Value, new Point(x1, y1), ActiveColor);
                    if (Context.IsMultiplayer)
                        Helper.Multiplayer.SendMessage(new PaintMPInfo { ID = GetID(), Color = SavedArtpiece.ColorToString(ActiveColor), X = LinePoint.Value.X, Y = LinePoint.Value.Y, X2 = x1, Y2 = y1, Line = true }, "Platonymous.Artista.Paint", new string[] { "Platonymous.Artista" }); ;

                    LinePoint = new Point(x1, y1);

                    if (!Palette.Contains(ActiveColor))
                        PopulatePalette();
                }
            }
            else if (ColorPickerRect.HasValue && ColorPickerRect.Value.Contains(x, y))
            {
                int x1 = (int)(((x - ColorPickerRect.Value.X) * 360f) / ColorPickerRect.Value.Width);
                PickColor(x1);
            }
            else if (ColorPickerDetailRect.HasValue && ColorPickerDetailRect.Value.Contains(x, y))
            {
                int x1 = (int)(((x - ColorPickerDetailRect.Value.X) * 500f) / ColorPickerDetailRect.Value.Width);
                int y1 = (int)(((y - ColorPickerDetailRect.Value.Y) * 500f) / ColorPickerDetailRect.Value.Height);

                PickColor(x1, y1, x, y);
            } else if (ColorPickerRect.HasValue && Palette.Count() > 0 && PaletteRect.FirstOrDefault(p => p.Contains(x, y)) is Rectangle rect && PaletteRect.IndexOf(rect) is int p && p >= 0 && Palette.Count() > p)
            {
                ActiveColor = Palette[p];
                if (ColorToHSV(ActiveColor, out double hue, out double sat, out double val))
                {
                    SetupColorBase(new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height), false);
                }
            } else if (SaveRect.HasValue && SaveRect.Value.Contains(x, y))
            {
                Easel.SetArt(Art);
                exitThisMenu();
            }
            else if (OnlineRect.HasValue && OnlineRect.Value.Contains(x, y))
            {
                Easel.SetArt(Art);
                exitThisMenu();
                Task.Run(() => {
                var menu = new OnlineArtMenu(Helper, Monitor, ArtistaMod.Singleton.OnlineApi);
                    Game1.activeClickableMenu = menu;
                });
            }
            else if (NewRect.HasValue && NewRect.Value.Contains(x, y))
            {
                exitThisMenu();
                Easel.SetArt(null);
                Game1.activeClickableMenu = new SelectCanvasMenu(Easel, Helper, Monitor);
            }
            else if (ResetRect.HasValue && ResetRect.Value.Contains(x, y))
            {
                Art?.Reset();
                PopulatePalette();
            }
            else if (FinishRect.HasValue && FinishRect.Value.Contains(x, y) && Game1.player.freeSpotsInInventory() > 0)
            {
                exitThisMenu();

                Game1.activeClickableMenu = new NamingMenu((s) =>
                {
                    if (Art.Author == Game1.player.Name || string.IsNullOrEmpty(Art.Author) || Art?.Author == ArtistaMod.Singleton.OnlineApi.CurrentUser.name)
                        Art.SetName(s);

                    if (Art.GetItem() is Item i)
                        Game1.player.addItemToInventory(i);

                    if ((Art.Author == Game1.player.Name || string.IsNullOrEmpty(Art.Author) || Art?.Author == ArtistaMod.Singleton.OnlineApi.CurrentUser.name) && ArtistaMod.Singleton.OnlineApi.CurrentUser is User usr)
                        Art.SetAuthor(usr.name);

                    Easel.SetArt(null);
                    Game1.activeClickableMenu.exitThisMenu();
                }, "Name your painting", Art.Name);
                
            }
            else if (ShareRect.HasValue && ShareRect.Value.Contains(x, y) && Game1.player.freeSpotsInInventory() > 0)
            {
                exitThisMenu();
                Game1.activeClickableMenu = new NamingMenu((s) =>
                {
                    Art.SetName(s);
                    Game1.activeClickableMenu.exitThisMenu();

                    ArtistaMod.Singleton.OnlineApi.SetUser(() =>
                    {
                        if (ArtistaMod.Singleton.OnlineApi.CurrentUser is User usr)
                            Art.SetAuthor(usr.name);

                        if (string.IsNullOrEmpty(Art.OnlineId))
                        {
                            Art.Author = ArtistaMod.Singleton.OnlineApi.CurrentUser.name;
                            if (ArtistaMod.Singleton.OnlineApi.UploadArt(Art) is OnlineArtpiece o)
                            {
                                Art.Save();
                            }
                        }
                        else
                        {
                        }

                        Game1.activeClickableMenu = new OnlineArtMenu(Helper, Monitor, ArtistaMod.Singleton.OnlineApi, owned: true);
                    });
                    

                }, "Name your painting", Art.Name);
            }
            else if (CompeteRect.HasValue && CompeteRect.Value.Contains(x, y) && Game1.player.freeSpotsInInventory() > 0)
            {
                exitThisMenu();
                Game1.activeClickableMenu = new NamingMenu((s) =>
                {
                    Art.SetName(s);
                    Game1.activeClickableMenu.exitThisMenu();

                    ArtistaMod.Singleton.OnlineApi.SetUser(() =>
                    {
                        if (ArtistaMod.Singleton.OnlineApi.CurrentUser is User usr)
                            Art.SetAuthor(usr.name);

                        if (string.IsNullOrEmpty(Art.OnlineId))
                        {
                            Art.Author = ArtistaMod.Singleton.OnlineApi.CurrentUser.name;
                            if (ArtistaMod.Singleton.OnlineApi.UploadArt(Art, Competitons.items.FirstOrDefault()?.id ?? "default") is OnlineArtpiece o)
                            {
                                Art.Save();
                            }
                        }
                        else
                        {
                        }

                        Game1.activeClickableMenu = new OnlineArtMenu(Helper, Monitor, ArtistaMod.Singleton.OnlineApi, true);
                    });


                }, "Name your painting", Art.Name);
            }


            base.releaseLeftClick(x, y);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            SetBounds(new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height));
            base.gameWindowSizeChanged(oldBounds, newBounds);
        }

        private double GetHue(float r, float g, float b)
        {
            double h = 0;
            float num1 = Math.Min(Math.Min(r, g), b);
            float num2 = Math.Max(Math.Max(r, g), b);
            float num3 = num2 - num1;
            if ((double)num2 != 0.0)
            {
                h = (double)r != (double)num2 ? ((double)g != (double)num2 ? (float)(4.0 + ((double)r - (double)g) / (double)num3) : (float)(2.0 + ((double)b - (double)r) / (double)num3)) : (g - b) / num3;
                h *= 60f;
                if ((double)h >= 0.0)
                    return h;
                h += 360f;
            }
            else
                h = 0f;

            return h;
        }

        private bool ColorToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = GetHue(color.R, color.G, color.B);
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;

            return true;
        }
        private Color ColorFromHSV(double h, double s, double v)
        {
            return Helper.Reflection.GetMethod(new ColorPicker("converter", 0, 0), "HsvToRgb").Invoke<Color>(h, s, v);
        }

        private void PopulatePalette()
        {
            Color[] colors = new Color[1] { Color.White };
            int x = CanvasRect.Value.Left;
            int y = CanvasRect.Value.Bottom;
            int h = 40;
            PaletteTexture.SetData(colors);
            PaletteRect.Clear();
            Palette.Clear();
            foreach (var color in Art.Canvas)
                if (color != Color.Transparent && !Palette.Contains(color))
                    Palette.Add(color);

            int w = CanvasRect.Value.Width / (Math.Max(1, Palette.Count));
            int fw = 0;
            for (int i = 0; i < Palette.Count();i ++)
            {
                PaletteRect.Add(new Rectangle(x, y, i == Palette.Count() - 1 ? CanvasRect.Value.Width - fw : w, h));
                fw += w;
                x += w;
            }
        }

        private void PickColor(int x)
        {
            if (ColorToHSV(ActiveColor, out double hue, out double sat, out double val))
            {
                ActiveColor = ColorFromHSV(x, sat, val);
                SetupColorBase(new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height), false);
                PopulatePalette();
            }
        }

        private void PickColor(int x, int y, int xpos,int ypos)
        {
            if (ColorToHSV(ActiveColor, out double h, out double s, out double v))
            {
                double limit = 0.10d;
                double xd = ((int)((x / (ColorPickerDetail.Width * 1d)) / limit)) * limit;
                double yd = ((int)((y / (ColorPickerDetail.Height * 1d)) / limit)) * limit;
                ActiveColor = ColorFromHSV(h, 1d - yd, 1d - xd);
            }
            //SetupColorBase(new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height), false, false);
            ColorPoint2 = new Vector2(xpos - ColorPickerDetailRect.Value.X, ypos - ColorPickerDetailRect.Value.Y);

            PopulatePalette();
        }

        private void SetupColorBase(Rectangle viewport, bool redraw = true, bool redrawDetail = true)
        {
            if (SettingColorPicker)
            {
                Game1.delayedActions.Add(new DelayedAction(200, () => SetupColorBase(viewport, redraw)));
                return;
            }
            if (ColorPicker == null)
                redraw = true;

            SettingColorPicker = redraw;
            ColorPickerRect = new Rectangle(80, 60, viewport.Height / 3, 50);
     
                ColorPicker = redraw ? new Texture2D(Game1.graphics.GraphicsDevice, 360, 1) : ColorPicker;
                Color[] colors = new Color[360];
                if (ColorToHSV(ActiveColor, out double hue, out double sat, out double val))
                {
                    ColorPoint = new Vector2(((float)hue / 360f) * ColorPickerRect.Value.Width, 25);
                    if (redraw)
                        for (int i = 0; i < 360; i++)
                            colors[i] = redraw ? ColorFromHSV(i, 1, 1) : Color.Transparent;
                }

                if (redraw)
                    ColorPicker.SetData(colors);

                SettingColorPicker = false;
                SetupColorDetail(ActiveColor, redrawDetail);
        }

        private void SetupColorDetail(Color? point = null, bool redraw = true)
        {
            if (SettingColorPickerDetail)
            {
                Game1.delayedActions.Add(new DelayedAction(1, () => SetupColorDetail(point, redraw)));
                return;
            }

            OldDetails = ColorPickerDetail ?? PaletteTexture;

            if (ColorPickerDetail == null)
                redraw = true;

            SettingColorPickerDetail = redraw;
            if (!point.HasValue)
                point = ActiveColor;

            ColorPickerDetailRect = new Rectangle(ColorPickerRect.Value.Left, ColorPickerRect.Value.Bottom + 10, ColorPickerRect.Value.Width, ColorPickerRect.Value.Width);

                float cpdSize = 500f;
                ColorPickerDetail = redraw ? new Texture2D(Game1.graphics.GraphicsDevice, (int)cpdSize, (int)cpdSize) : ColorPickerDetail;
                Color[] colors2 = new Color[ColorPickerDetail.Width * ColorPickerDetail.Height];
                double limit = 0.10d;
                bool setActive = false;
            Task.Run(() =>
            {
                if (ColorToHSV(ActiveColor, out double hue, out double sat, out double val))
                    for (int yc = 0; yc < ColorPickerDetail.Height; yc++)
                        for (int xc = 0; xc < ColorPickerDetail.Width; xc++)
                        {
                            double xd = ((int)((xc / (ColorPickerDetail.Width * 1d)) / limit)) * limit;
                            double yd = ((int)((yc / (ColorPickerDetail.Height * 1d)) / limit)) * limit;
                            var color = ColorFromHSV(hue, 1d - yd, 1d - xd);

                            if (!setActive && point.HasValue && color == point)
                            {
                                int wi = (ColorPickerDetailRect.Value.Width / (int)(1d / limit)) / 2;
                                ColorPoint2 = new Vector2(wi + (int)((xc / cpdSize) * ColorPickerDetailRect.Value.Width), wi + (int)((yc / cpdSize) * ColorPickerDetailRect.Value.Height));
                                setActive = true;
                            }

                            colors2[(yc * ColorPickerDetail.Width) + xc] = color;
                        }
                if (redraw)
                    ColorPickerDetail.SetData(colors2);

                SettingColorPickerDetail = false;
            });
            ActiveColorRect = new Rectangle(ColorPickerDetailRect.Value.Left, ColorPickerDetailRect.Value.Bottom + 10, ColorPickerDetailRect.Value.Width, 40);
            
            ColorPickerBackground = new Rectangle(
                    ColorPickerRect.Value.Left - 10,
                    ColorPickerRect.Value.Top - 10,
                    ColorPickerRect.Value.Width + 20,
                    (ActiveColorRect.Value.Bottom - ColorPickerRect.Value.Top) + 20);

        }

        private void SetBounds(Rectangle viewport)
        {
            if (!OldBounds.HasValue || OldBounds.Value != viewport)
            {
                ColorPickerRect = new Rectangle(80, 60, viewport.Height / 3, 50);

                int y = 80;
                    CanvasScale = (viewport.Height - (250)) / Art.Height;
                    int x = (int)(ColorPickerRect.Value.Right + 120);
                    int h = (int)(Art.Height * CanvasScale);
                    int w = (int)(Art.Width * CanvasScale);
                    CanvasRect = new Rectangle(x, y, w, h);
                    Color[] color = new Color[1] { Art.CanvasColor };
                    Color[] color2 = new Color[1] { Color.Black };

                    CanvasBackground.SetData(color);
                    Curtain.SetData(color2);
                    OldBounds = viewport;
                    PopulatePalette();
                    upperRightCloseButton = new ClickableTextureComponent(new Rectangle(CanvasRect.Value.Right, CanvasRect.Value.Top - 48, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);

                    SaveRect = new Rectangle(CanvasRect.Value.Right + ButtonMargin, CanvasRect.Value.Top, ButtonWidth, ButtonHeight);
                    FinishRect = new Rectangle(SaveRect.Value.Left, SaveRect.Value.Bottom + ButtonDistance, SaveRect.Value.Width, SaveRect.Value.Height);
                    ResetRect = new Rectangle(SaveRect.Value.Left, FinishRect.Value.Bottom + ButtonDistance, SaveRect.Value.Width, SaveRect.Value.Height);

                if (Art?.Author == Game1.player.Name || string.IsNullOrEmpty(Art?.Author) || Art?.Author == ArtistaMod.Singleton.OnlineApi.CurrentUser.name)
                {
                    ShareRect = new Rectangle(SaveRect.Value.Left, ResetRect.Value.Bottom + ButtonDistance, SaveRect.Value.Width, SaveRect.Value.Height);
                    if (Competitons.items.Count > 0)
                    {
                        CompeteRect = new Rectangle(SaveRect.Value.Left, ShareRect.Value.Bottom + ButtonDistance, SaveRect.Value.Width, SaveRect.Value.Height);
                        OnlineRect = new Rectangle(SaveRect.Value.Left, CompeteRect.Value.Bottom + ButtonDistance, SaveRect.Value.Width, SaveRect.Value.Height);
                    }
                    else
                    {
                        OnlineRect = new Rectangle(SaveRect.Value.Left, ShareRect.Value.Bottom + ButtonDistance, SaveRect.Value.Width, SaveRect.Value.Height);

                    }
                }
                else
                {
                    OnlineRect = new Rectangle(SaveRect.Value.Left, ResetRect.Value.Bottom + ButtonDistance, SaveRect.Value.Width, SaveRect.Value.Height);
                }

                if (ArtistaMod.Config.FreeCanvas)
                        NewRect = new Rectangle(SaveRect.Value.Left, OnlineRect.Value.Bottom + ButtonDistance, SaveRect.Value.Width, SaveRect.Value.Height);


                Task.Run(() =>
                {
                    SetupColorBase(viewport);
                });
            }
        }

        private void drawBox(SpriteBatch b, int xPos,int yPos, int boxWidth, int boxHeight)
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
            if (!CanvasRect.HasValue || !OldBounds.HasValue || OldBounds.Value.Width != Game1.uiViewport.Width || OldBounds.Value.Height != Game1.uiViewport.Height)
                return;

            drawBackground(b);
            if (ColorPickerDetailRect.HasValue)
            {
                b.Draw(Game1.mouseCursors, new Rectangle(ColorPickerDetailRect.Value.Right - 10, ColorPickerDetailRect.Value.Center.Y - 95, 150, 195), new Rectangle(306, 320, 16, 16), Color.White);
                b.Draw(Game1.mouseCursors, new Rectangle(ColorPickerDetailRect.Value.Right - 10, ColorPickerDetailRect.Value.Center.Y - 100, 150, 24), new Rectangle(275, 313, 1, 6), Color.White);
                b.Draw(Game1.mouseCursors, new Rectangle(ColorPickerDetailRect.Value.Right - 10, ColorPickerDetailRect.Value.Center.Y + 100, 150, 24), new Rectangle(275, 313, 1, 6), Color.White);
            }

            drawBox(b, CanvasRect.Value.Left, CanvasRect.Value.Top - 4, CanvasRect.Value.Width, CanvasRect.Value.Height + 44);
            b.Draw(CanvasBackground, CanvasRect.Value, Color.White * 0.3f);

            if (ColorPickerBackground.HasValue)
            {
                drawBox(b, ColorPickerBackground.Value.Left, ColorPickerBackground.Value.Top - 4, ColorPickerBackground.Value.Width, ColorPickerBackground.Value.Height + 4);
            }

            drawBox(b, SaveRect.Value.Left, SaveRect.Value.Top, SaveRect.Value.Width, SaveRect.Value.Height);
            drawBox(b, FinishRect.Value.Left, FinishRect.Value.Top, FinishRect.Value.Width, FinishRect.Value.Height);
            drawBox(b, ResetRect.Value.Left, ResetRect.Value.Top, ResetRect.Value.Width, ResetRect.Value.Height);
            if (ShareRect.HasValue)
                drawBox(b, ShareRect.Value.Left, ShareRect.Value.Top, ShareRect.Value.Width, ShareRect.Value.Height);
            if(CompeteRect.HasValue)
                drawBox(b, CompeteRect.Value.Left, CompeteRect.Value.Top, CompeteRect.Value.Width, CompeteRect.Value.Height);
            drawBox(b, OnlineRect.Value.Left, OnlineRect.Value.Top, OnlineRect.Value.Width, OnlineRect.Value.Height);

            if (NewRect.HasValue)
                drawBox(b, NewRect.Value.Left, NewRect.Value.Top, NewRect.Value.Width, NewRect.Value.Height);

            Utility.drawTextWithColoredShadow(b, "Save", Game1.smallFont, new Vector2(SaveRect.Value.Left + ButtonPadding * 2, SaveRect.Value.Top + ButtonPadding), Color.Maroon, Color.DarkGoldenrod * 0.35f);
            Utility.drawTextWithColoredShadow(b, "Frame", Game1.smallFont, new Vector2(FinishRect.Value.Left + ButtonPadding * 2, FinishRect.Value.Top + ButtonPadding), Color.Maroon, Color.DarkGoldenrod * 0.35f);
            Utility.drawTextWithColoredShadow(b, "Reset", Game1.smallFont, new Vector2(ResetRect.Value.Left + ButtonPadding * 2, ResetRect.Value.Top + ButtonPadding), Color.Maroon , Color.DarkGoldenrod * 0.35f);
            if (ShareRect.HasValue)
                Utility.drawTextWithColoredShadow(b, "Share", Game1.smallFont, new Vector2(ShareRect.Value.Left + ButtonPadding * 2, ShareRect.Value.Top + 10), Color.Maroon, Color.DarkGoldenrod * 0.35f);
            if (CompeteRect.HasValue)
                Utility.drawTextWithColoredShadow(b, "Compete", Game1.smallFont, new Vector2(CompeteRect.Value.Left + ButtonPadding * 2, CompeteRect.Value.Top + 10), Color.Maroon, Color.DarkGoldenrod * 0.35f);
            Utility.drawTextWithColoredShadow(b, "Visit Online", Game1.smallFont, new Vector2(OnlineRect.Value.Left + ButtonPadding * 2, OnlineRect.Value.Top + 10), Color.Maroon, Color.DarkGoldenrod * 0.35f);

            if (NewRect.HasValue)
                Utility.drawTextWithColoredShadow(b, "New", Game1.dialogueFont, new Vector2(NewRect.Value.Left + ButtonPadding * 2, NewRect.Value.Top + ButtonPadding), Color.Maroon, Color.DarkGoldenrod * 0.35f);

            if (!SettingColorPicker && ColorPickerRect.HasValue && ColorPicker != null)
            {
                b.Draw(ColorPicker, ColorPickerRect.Value, Color.White);
            }
            if (!SettingColorPickerDetail && ColorPickerDetailRect.HasValue && ColorPickerDetail != null)
            {
                b.Draw(ColorPickerDetail, ColorPickerDetailRect.Value, Color.White);
            }
            else if(ColorPickerDetailRect.HasValue && OldDetails != null)
            {
                b.Draw(OldDetails, ColorPickerDetailRect.Value, Color.White);
            }

            if (ActiveColorRect.HasValue && PaletteTexture != null)
            {
                b.Draw(PaletteTexture, ActiveColorRect.Value, ActiveColor);
            }

            if (!SettingColorPickerDetail && !SettingColorPicker && ColorPickerDetailRect.HasValue && ActiveColorRect.HasValue && ColorPickerRect.HasValue && ColorPicker != null && ColorPickerDetail != null)
            {
                b.Draw(Indicator, new Rectangle(
                (int)(ColorPickerRect.Value.X + ColorPoint.X - 5),
                (int)(ColorPickerRect.Value.Y + ColorPoint.Y - 5),
                10,
                10), Color.White);

                b.Draw(Indicator, new Rectangle(
                   (int)(ColorPickerDetailRect.Value.X + ColorPoint2.X - 10),
                   (int)(ColorPickerDetailRect.Value.Y + ColorPoint2.Y - 10),
                   20,
                   20), Color.White);


            b.Draw(Indicator, new Rectangle(
               (int)(ColorPickerDetailRect.Value.X + ColorPoint2.X - 8),
               (int)(ColorPickerDetailRect.Value.Y + ColorPoint2.Y - 8),
               16,
               16), ActiveColor);

            }

            b.Draw(Art.GetTexture(), CanvasRect.Value, Color.White);

            for(int c = 0; c < Palette.Count; c++)
            {
                var color = Palette[c];
                var rect = PaletteRect[c];
                b.Draw(PaletteTexture, rect,color);
            }

            base.draw(b);
            drawMouse(b);
            if (!Visible && !Started)
            {
                Started = true;
                Game1.delayedActions.Add(new DelayedAction(1000, () => Visible = true));
            }
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

        internal float GetSquaredDistance(Point point1, Point point2)
        {
            float a = (point1.X - point2.X);
            float b = (point1.Y - point2.Y);
            return (a * a) + (b * b);
        }

        public Texture2D GetCircle(int radius, Color color)
        {
            int diameter = radius * 2;
            Rectangle bounds = new Rectangle(0, 0, diameter + 1, diameter + 1);
            Point c = bounds.Center;
            int sDist = radius * radius;
            return GetRectangle(bounds.Width, bounds.Height, (x, y, w, h) => GetSquaredDistance(new Point(x, y), c) > sDist ? Color.Transparent : color);
        }

        public Texture2D GetRectangle(int width, int height, Color color)
        {
            Texture2D rect = new Texture2D(Game1.graphics.GraphicsDevice, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i)
                data[i] = color;
            rect.SetData(data);
            return rect;
        }

        internal Texture2D GetRectangle(int width, int height, Func<int, int, int, int, Color> colorPicker)
        {
            Texture2D rect = new Texture2D(Game1.graphics.GraphicsDevice, width, height);

            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i)
            {
                int x = i % width;
                int y = (i - x) / width;
                data[i] = colorPicker(x, y, width, height);
            }
            rect.SetData(data);
            return rect;
        }

        protected override void cleanupBeforeExit()
        {
            Easel.Save();

            base.cleanupBeforeExit();
        }
    }
}
