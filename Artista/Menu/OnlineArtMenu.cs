using Artista.Artpieces;
using Artista.Objects;
using Artista.Online;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Artista.Menu
{

    public class OnlineArtMenu : IClickableMenu
    {
        private IModHelper Helper { get; }
        private IMonitor Monitor { get; }

        private Texture2D Curtain { get; set; } = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);

        private Rectangle? OldBounds { get; set; } = null;

        private ListArtRequest CurrentList { get; set; }

        private OnlineArtAPI Api { get; set; }

        List<OnlineArtPieceEntry> Items = new List<OnlineArtPieceEntry>();
        private List<TextChoice> Choices { get; } = new List<TextChoice>();

        private TextChoice OwnedChoice { get; set; }

        private int Page { get; set; } = 1;

        private bool SettingBounds { get; set; } = false;
        private bool BoundsSet { get; set; } = false;

        public string Competition { get; set; } = "default";

        public bool isCompetition => Competition != "default";

        public bool Owned { get; set; } = false;

        public Texture2D Cursors { get; set; }

        public ListCompetitons Competitions { get; set; }

        public OnlineArtMenu(IModHelper helper, IMonitor monitor, OnlineArtAPI api, bool owned = false)
       : base(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height, true)
        {
            Helper = helper;
            Monitor = monitor;
            Cursors = Helper.GameContent.Load<Texture2D>("LooseSprites/Cursors");
            Owned = owned;

            Api = api;

            Competitions = Api.GetCompetitions();
            
            SetBounds(new Rectangle(0,0, Game1.uiViewport.Width, Game1.uiViewport.Height), true);
        }

        public void GetArt()
        {
            CurrentList = Api.GetArt(Page, isCompetition ? Competition : "default", 12, Owned && Api.CurrentUser != null);
        }

        private void AddPage(int p)
        {
            Page += p;
            
                if (Page == 0)
                    Page = CurrentList.totalPages;
                else if (Page > CurrentList.totalPages)
                    Page = 1;


            GetArt();

            SetBounds(new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), true);
            
        }

        private void SetBounds(Rectangle viewport, bool force = false)
        {
            if (SettingBounds)
            {
                Game1.delayedActions.Add(new DelayedAction(200, () => SetBounds(viewport, force)));
                return;
            }

            if (!OldBounds.HasValue || OldBounds.Value != viewport || force)
            {
                
                Color[] color2 = new Color[1] { Color.Black };

                Curtain.SetData(color2);
                OldBounds = viewport;


                
                Items.Clear();
                SettingBounds = true;
                int perLine = 6;
                int lines = 2;
                int frameWidth = viewport.Width / perLine;
                int frameHeight = (viewport.Height - 300) / lines;

                Task.Run(() =>
                {
                    GetArt();

                    if (CurrentList == null)
                        exitThisMenu();
                    
                    Rectangle last = new Rectangle(((viewport.Width - (frameWidth * perLine)) / 2) - frameWidth, 20 + (((viewport.Height - 300) - (frameHeight * lines)) / 2), frameWidth, frameHeight);

                    int c = 0;
                    foreach (var frame in CurrentList.items)
                    {
                        var artsav = SavedArtpiece.FromJson(frame.artwork);
                        var art = artsav.ArtType == (int)ArtType.Painting ? new Painting(artsav) : new Painting(artsav);

                        if (c != 0 && c % perLine == 0)
                            last = new Rectangle(((viewport.Width - (frameWidth * perLine)) / 2) - frameWidth, last.Bottom, last.Width, last.Height);

                        last = new Rectangle(last.Right, last.Top, last.Width, last.Height);

                        if (art.TileWidth == art.TileHeight)
                        {
                            int lheight = frameHeight - 20;
                            int lwidth = frameWidth - 20;
                            int size = Math.Min(lwidth, lheight);
                            Items.Add(new OnlineArtPieceEntry(art, new Rectangle(last.Left + ((frameWidth - size) / 2), last.Top + ((frameHeight - size) / 2), size, size), false, frame));
                        }
                        else
                        {
                            int lheight = frameHeight - 20;
                            int lwidth = lheight / 2;
                            Items.Add(new OnlineArtPieceEntry(art, new Rectangle(last.Left + ((frameWidth - lwidth) / 2), last.Top + ((frameHeight - lheight) / 2), lwidth, lheight), false, frame));
                        }
                        c++;
                    }

                    Choices.Clear();
                    Choices.Add(new TextChoice("Previous Page", () => AddPage(-1)));
                    Choices.Add(new TextChoice("Next Page", () => AddPage(1)));
                    OwnedChoice = new TextChoice("Your Art", () => SwitchedOwned());
                    OwnedChoice.Text = Owned ? "All Art" : "Your Art";
                    Choices.Add(OwnedChoice);
                    if (Competitions.items.Count > 0)
                    {
                        Choices.Add(new TextChoice(isCompetition ? "Back" : "Competiton", () => SwitchCompetiton()));
                    }
                    Choices.Add(new TextChoice("Close", () => Game1.activeClickableMenu?.exitThisMenu()));
                    Rectangle menulast = new Rectangle(((viewport.Width - (frameWidth * perLine)) / 2) - 90, (lines * frameHeight) + 150 + (((viewport.Height - 300) - (frameHeight * lines)) / 2), 70, 45);

                    c = 0;
                    foreach (var choice in Choices)
                    {
                        choice.Rectangle = new Rectangle(menulast.Right + 80, menulast.Top, (choice.Text.Length * 15 + 25), menulast.Height);
                        menulast = choice.Rectangle;
                        c++;
                    }

                    SettingBounds = false;
                });

              
            }
        }

        public void SwitchedOwned()
        {
            Owned = !Owned;
            OwnedChoice.Text = Owned ? "All Art" : "Your Art";
            Page = 1;
            GetArt();

            SetBounds(new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), true);
        }

        public void SwitchCompetiton()
        {
            if (isCompetition)
            {
                Competition = "default";
                Page = 1;
                CurrentList = Api.GetArt(Page);
            }
            else
            {
                Competition = Competitions.items.FirstOrDefault()?.id ?? "default";
                Page = 0;
                CurrentList = Api.GetArt(Page, Competitions.items[Page]?.id ?? "default");
            }

            SetBounds(new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), true);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            bool removed = false;

            if (!SettingBounds)
                foreach (var frame in Items)
            {
                if (frame.Rectangle.Contains(x, y) && Game1.player.freeSpotsInInventory() > 0)
                {
                    if (frame.Orp.owner == Api.CurrentUser.id && Api.DeleteArt(frame.Orp))
                    {
                        if (Api.DownloadArt(frame.Orp) is Artpiece art && art.GetItem() is Item i)
                        {
                            Game1.player.addItemToInventory(i);
                            frame.Visible = false;
                            removed = true;
                            break;
                        }
                    }
                }
            }

            if (removed)
            {
                CurrentList = Api.GetArt(Page);
                SetBounds(new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), true);
                return;
            }

            base.receiveRightClick(x, y, playSound);
        }

        public override void releaseLeftClick(int x, int y)
        {
            if (!SettingBounds)
                foreach (var frame in Items)
            {
                if (frame.Rectangle.Contains(x, y) && Game1.player.freeSpotsInInventory() > 0)
                {
                    exitThisMenu();

                    if(Api.DownloadArt(frame.Orp) is Artpiece art && art.GetItem() is Item i)
                        Game1.player.addItemToInventory(i);

                    return;
                }
            }


            foreach (var choice in Choices)
            {
                if (choice.Rectangle.Contains(x, y))
                {
                    choice.Action();
                    return;
                }
            }

            base.releaseLeftClick(x, y);
        }

        public override void performHoverAction(int x, int y)
        {
            if (!SettingBounds)
                Items.ForEach(f => f.Highlighted = false);

            if (!SettingBounds)
                foreach (var frame in Items)
            {
                if (frame.Rectangle.Contains(x, y))
                {
                    frame.Highlighted = true;

                    return;
                }
            }

            base.performHoverAction(x, y);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            SetBounds(new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), true);
            base.gameWindowSizeChanged(oldBounds, newBounds);
        }

        private void drawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight, float opacity = 1f)
        {
            b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos, boxWidth, boxHeight), new Rectangle(306, 320, 16, 16), Color.White * opacity);
            b.Draw(Game1.mouseCursors, new Rectangle(xPos, yPos - 20, boxWidth, 24), new Rectangle(275, 313, 1, 6), Color.White * opacity);
            b.Draw(Game1.mouseCursors, new Rectangle(xPos + 12, yPos + boxHeight, boxWidth - 20, 32), new Rectangle(275, 328, 1, 8), Color.White * opacity);
            b.Draw(Game1.mouseCursors, new Rectangle(xPos - 32, yPos + 24, 32, boxHeight - 28), new Rectangle(264, 325, 8, 1), Color.White * opacity);
            b.Draw(Game1.mouseCursors, new Rectangle(xPos + boxWidth, yPos, 28, boxHeight), new Rectangle(293, 324, 7, 1), Color.White * opacity);
            b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos - 28), new Rectangle(261, 311, 14, 13), Color.White * opacity, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
            b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos - 28), new Rectangle(291, 311, 12, 11), Color.White * opacity, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
            b.Draw(Game1.mouseCursors, new Vector2(xPos + boxWidth - 8, yPos + boxHeight - 8), new Rectangle(291, 326, 12, 12), Color.White * opacity, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
            b.Draw(Game1.mouseCursors, new Vector2(xPos - 44, yPos + boxHeight - 4), new Rectangle(261, 327, 14, 11), Color.White * opacity, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
        }

        public override void draw(SpriteBatch b)
        {
            if (!OldBounds.HasValue || OldBounds.Value.Width != Game1.viewport.Width || OldBounds.Value.Height != Game1.viewport.Height)
                return;

            drawBackground(b);
            if (!SettingBounds)
                foreach (var frame in Items)
                {
                    b.Draw(frame.Art.GetFullTexture(), frame.Rectangle, Color.White);
                    if (frame.Winner)
                    {
                        b.Draw(Cursors, new Rectangle(frame.Rectangle.Right - 16, frame.Rectangle.Bottom - 24, 32,32), new Rectangle(192, 128, 64, 64), Color.White);
                    }
                    else if(frame.Orp.collection == (Competitions.items.FirstOrDefault()?.id ?? "none"))
                    {
                        b.Draw(Cursors, new Rectangle(frame.Rectangle.Right - 16, frame.Rectangle.Bottom - 24, 32, 32), new Rectangle(0, 410, 16, 16), Color.White);

                    }
                }


            if (!SettingBounds)
                foreach (var frame in Items.Where(f => f.Highlighted))
            {
                b.Draw(Curtain, frame.Rectangle, Color.White * 0.3f);
                Utility.drawTextWithColoredShadow(b, "Download", Game1.dialogueFont, new Vector2(frame.Rectangle.Left + ((frame.Rectangle.Width - 170) / 2), frame.Rectangle.Top + ((frame.Rectangle.Height - 60) / 2)), Color.White, Color.DarkGoldenrod * 0.35f);
                if(frame.Orp.owner == Api.CurrentUser.id)
                {
                    Utility.drawTextWithColoredShadow(b, "Right-Click to Delete", Game1.smallFont, new Vector2(frame.Rectangle.Left + ((frame.Rectangle.Width - 230) / 2), frame.Rectangle.Top + 60 + ((frame.Rectangle.Height - 60) / 2)), Color.White, Color.DarkGoldenrod * 0.35f);

                }
            }

            if (isCompetition && Competitions?.items?.FirstOrDefault() != null)
            {
                Competiton comp = Competitions.items.FirstOrDefault();
                string text = "Currently no competition!";
                if (comp != null)
                    text = $"Theme: {comp.title} Ends: {comp.end}";
                var rect = Choices.FirstOrDefault().Rectangle;
                Utility.drawTextWithColoredShadow(b, text, Game1.smallFont, new Vector2(rect.Left,rect.Top - 80), Color.White, Color.DarkGoldenrod * 0.35f);

            }

            foreach (var choice in Choices)
                drawBox(b, choice.Rectangle.Left, choice.Rectangle.Top, choice.Rectangle.Width, choice.Rectangle.Height, choice.Opacity);

            foreach (var choice in Choices)
                Utility.drawTextWithColoredShadow(b, choice.Text, Game1.smallFont, new Vector2(choice.Rectangle.Left + 20, choice.Rectangle.Top + 10), Color.Maroon * choice.Opacity, Color.DarkGoldenrod * 0.35f * choice.Opacity);


            base.draw(b);
            drawMouse(b);
        }

        public override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
        {
            draw(b);
        }

        public override bool shouldDrawCloseButton()
        {
            return false;
        }

        public override void drawBackground(SpriteBatch b)
        {
            var viewport = new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height);

            b.Draw(Curtain, viewport, Color.White * 0.6f);

        }
    }
}
