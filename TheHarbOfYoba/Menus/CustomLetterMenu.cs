using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Tools;
using StardewValley.Menus;
using StardewValley;
using System;
using System.Collections.Generic;

namespace TheHarpOfYoba.Menus
{
    public class CustomLetterMenu : IClickableMenu
    {
        private int questID = -1;
        private string learnedRecipe = "";
        private string cookingOrCrafting = "";
        private List<string> mailMessage = new List<string>();
        private List<ClickableComponent> itemsToGrab = new List<ClickableComponent>();
        public const int letterWidth = 320;
        public const int letterHeight = 180;
        public Texture2D letterTexture;
        private string mailTitle;
        private int page;
        private float scale;
        private bool isMail;
        private ClickableTextureComponent backButton;
        private ClickableTextureComponent forwardButton;
        private ClickableComponent acceptQuestButton;
        public const float scaleChange = 0.003f;

        public CustomLetterMenu(string text)
          : base((int)Utility.getTopLeftPositionForCenteringOnScreen(320 * Game1.pixelZoom, 180 * Game1.pixelZoom, 0, 0).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(320 * Game1.pixelZoom, 180 * Game1.pixelZoom, 0, 0).Y, 320 * Game1.pixelZoom, 180 * Game1.pixelZoom, true)
        {
            Game1.playSound("shwip");
            this.backButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize / 2, this.yPositionOnScreen + this.height - Game1.tileSize / 2 - 16 * Game1.pixelZoom, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), (float)Game1.pixelZoom, false);
            this.forwardButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - Game1.tileSize / 2 - 12 * Game1.pixelZoom, this.yPositionOnScreen + this.height - Game1.tileSize / 2 - 16 * Game1.pixelZoom, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), (float)Game1.pixelZoom, false);
            Game1.temporaryContent = Game1.content.CreateTemporary();
            this.letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
            this.mailMessage = SpriteText.getStringBrokenIntoSectionsOfHeight(text, this.width - Game1.tileSize / 2, this.height - Game1.tileSize * 2);
        }

        public CustomLetterMenu(string mail, string mailTitle, List<Item> items)
          : base((int)Utility.getTopLeftPositionForCenteringOnScreen(320 * Game1.pixelZoom, 180 * Game1.pixelZoom, 0, 0).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(320 * Game1.pixelZoom, 180 * Game1.pixelZoom, 0, 0).Y, 320 * Game1.pixelZoom, 180 * Game1.pixelZoom, true)
        {
            mail = mail.Replace("@", Game1.player.Name);
            this.isMail = true;
            
            Game1.playSound("shwip");
            this.backButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize / 2, this.yPositionOnScreen + this.height - Game1.tileSize / 2 - 16 * Game1.pixelZoom, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), (float)Game1.pixelZoom, false);
            this.forwardButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - Game1.tileSize / 2 - 12 * Game1.pixelZoom, this.yPositionOnScreen + this.height - Game1.tileSize / 2 - 16 * Game1.pixelZoom, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), (float)Game1.pixelZoom, false);
            this.acceptQuestButton = new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width / 2 - Game1.tileSize * 2, this.yPositionOnScreen + this.height - Game1.tileSize * 2, Game1.tileSize * 4, Game1.tileSize), "");
            this.mailTitle = mailTitle;
            Game1.temporaryContent = Game1.content.CreateTemporary();
            this.letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
            int offset = -200;
            foreach (Item i in items)
            {
                offset += 120;
                Item obj = i;
                this.itemsToGrab.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + offset + this.width / 2 - 12 * Game1.pixelZoom, this.yPositionOnScreen + this.height - Game1.tileSize / 2 - 24 * Game1.pixelZoom, 24 * Game1.pixelZoom, 24 * Game1.pixelZoom), obj));
            }

            Random r = new Random((int)(Game1.uniqueIDForThisGame / 2UL) - Game1.year);
            mail = mail.Replace("%secretsanta", Utility.getRandomTownNPC(r, Utility.getFarmerNumberFromFarmer(Game1.player)).name);
            this.mailMessage = SpriteText.getStringBrokenIntoSectionsOfHeight(mail, this.width - Game1.tileSize, this.height - Game1.tileSize * 2);
        }

        
        public CustomLetterMenu(string mail, string mailTitle, Item obj)
          : base((int)Utility.getTopLeftPositionForCenteringOnScreen(320 * Game1.pixelZoom, 180 * Game1.pixelZoom, 0, 0).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(320 * Game1.pixelZoom, 180 * Game1.pixelZoom, 0, 0).Y, 320 * Game1.pixelZoom, 180 * Game1.pixelZoom, true)
        {
            mail = mail.Replace("@", Game1.player.Name);
            this.isMail = true;
            Game1.playSound("shwip");
            this.backButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize / 2, this.yPositionOnScreen + this.height - Game1.tileSize / 2 - 16 * Game1.pixelZoom, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), (float)Game1.pixelZoom, false);
            this.forwardButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - Game1.tileSize / 2 - 12 * Game1.pixelZoom, this.yPositionOnScreen + this.height - Game1.tileSize / 2 - 16 * Game1.pixelZoom, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), (float)Game1.pixelZoom, false);
            this.acceptQuestButton = new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width / 2 - Game1.tileSize * 2, this.yPositionOnScreen + this.height - Game1.tileSize * 2, Game1.tileSize * 4, Game1.tileSize), "");
            this.mailTitle = mailTitle;
            Game1.temporaryContent = Game1.content.CreateTemporary();
            this.letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
            
             this.itemsToGrab.Add(new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width / 2 - 12 * Game1.pixelZoom, this.yPositionOnScreen + this.height - Game1.tileSize / 2 - 24 * Game1.pixelZoom, 24 * Game1.pixelZoom, 24 * Game1.pixelZoom), obj));


            Random r = new Random((int)(Game1.uniqueIDForThisGame / 2UL) - Game1.year);
            mail = mail.Replace("%secretsanta", Utility.getRandomTownNPC(r, Utility.getFarmerNumberFromFarmer(Game1.player)).name);
            this.mailMessage = SpriteText.getStringBrokenIntoSectionsOfHeight(mail, this.width - Game1.tileSize, this.height - Game1.tileSize * 2);
        }
        

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(320 * Game1.pixelZoom, 180 * Game1.pixelZoom, 0, 0).X;
            this.yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(320 * Game1.pixelZoom, 180 * Game1.pixelZoom, 0, 0).Y;
            this.backButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + Game1.tileSize / 2, this.yPositionOnScreen + this.height - Game1.tileSize / 2 - 16 * Game1.pixelZoom, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), (float)Game1.pixelZoom, false);
            this.forwardButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - Game1.tileSize / 2 - 12 * Game1.pixelZoom, this.yPositionOnScreen + this.height - Game1.tileSize / 2 - 16 * Game1.pixelZoom, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), (float)Game1.pixelZoom, false);
            this.acceptQuestButton = new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width / 2 - Game1.tileSize * 2, this.yPositionOnScreen + this.height - Game1.tileSize * 2, Game1.tileSize * 4, Game1.tileSize), "");
            foreach (ClickableComponent clickableComponent in this.itemsToGrab)
                clickableComponent.bounds = new Rectangle(this.xPositionOnScreen + this.width / 2 - 12 * Game1.pixelZoom, this.yPositionOnScreen + this.height - Game1.tileSize / 2 - 24 * Game1.pixelZoom, 24 * Game1.pixelZoom, 24 * Game1.pixelZoom);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if ((double)this.scale < 1.0)
                return;
            base.receiveLeftClick(x, y, playSound);
            if (Game1.activeClickableMenu == null && Game1.currentMinigame == null)
            {
                this.unload();
            }
            else
            {
                foreach (ClickableComponent clickableComponent in this.itemsToGrab)
                {
                    if (clickableComponent.containsPoint(x, y) && clickableComponent.item != null)
                    {
                        Game1.playSound("coin");
                        Game1.player.addItemByMenuIfNecessary(clickableComponent.item, (ItemGrabMenu.behaviorOnItemSelect)null);
                        clickableComponent.item = (Item)null;
                        return;
                    }
                }
                if (this.backButton.containsPoint(x, y) && this.page > 0)
                {
                    this.page = this.page - 1;
                    Game1.playSound("shwip");
                }
                else if (this.forwardButton.containsPoint(x, y) && this.page < this.mailMessage.Count - 1)
                {
                    this.page = this.page + 1;
                    Game1.playSound("shwip");
                }
                else if (this.questID != -1 && this.acceptQuestButton.containsPoint(x, y))
                {
                    Game1.player.addQuest(this.questID);
                    this.questID = -1;
                    Game1.playSound("newArtifact");
                }
                else if (this.isWithinBounds(x, y))
                {
                    if (this.page < this.mailMessage.Count - 1)
                    {
                        this.page = this.page + 1;
                        Game1.playSound("shwip");
                    }
                    else if (this.page == this.mailMessage.Count - 1 && this.mailMessage.Count > 1)
                    {
                        this.page = 0;
                        Game1.playSound("shwip");
                    }
                    if (this.mailMessage.Count != 1 || this.isMail)
                        return;
                    this.exitThisMenuNoSound();
                    Game1.playSound("shwip");
                }
                else
                {
                    if (this.itemsLeftToGrab())
                        return;
                    this.exitThisMenuNoSound();
                    Game1.playSound("shwip");
                }
            }
        }

        public bool itemsLeftToGrab()
        {
            if (this.itemsToGrab == null)
                return false;
            foreach (ClickableComponent clickableComponent in this.itemsToGrab)
            {
                if (clickableComponent.item != null)
                    return true;
            }
            return false;
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            foreach (ClickableComponent clickableComponent in this.itemsToGrab)
                clickableComponent.scale = !clickableComponent.containsPoint(x, y) ? Math.Max(1f, clickableComponent.scale - 0.03f) : Math.Min(clickableComponent.scale + 0.03f, 1.1f);
            this.backButton.tryHover(x, y, 0.6f);
            this.forwardButton.tryHover(x, y, 0.6f);
            if (this.questID == -1)
                return;
            float scale = this.acceptQuestButton.scale;
            this.acceptQuestButton.scale = this.acceptQuestButton.bounds.Contains(x, y) ? 1.5f : 1f;
            if ((double)this.acceptQuestButton.scale <= (double)scale)
                return;
            Game1.playSound("Cowboy_gunshot");
        }

        public override void update(GameTime time)
        {
            base.update(time);
            TimeSpan timeSpan;
            if ((double)this.scale < 1.0)
            {
                double scale = (double)this.scale;
                timeSpan = time.ElapsedGameTime;
                double num = (double)timeSpan.Milliseconds * (3.0 / 1000.0);
                this.scale = (float)(scale + num);
                if ((double)this.scale >= 1.0)
                    this.scale = 1f;
            }
            if (this.page >= this.mailMessage.Count - 1 || this.forwardButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
                return;
            ClickableTextureComponent forwardButton = this.forwardButton;
            double num1 = 4.0;
            timeSpan = time.TotalGameTime;
            double num2 = Math.Sin((double)timeSpan.Milliseconds / (64.0 * Math.PI)) / 1.5;
            double num3 = num1 + num2;
            forwardButton.scale = (float)num3;
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            b.Draw(this.letterTexture, new Vector2((float)(this.xPositionOnScreen + this.width / 2), (float)(this.yPositionOnScreen + this.height / 2)), new Rectangle?(new Rectangle(0, 0, 320, 180)), Color.White, 0.0f, new Vector2(160f, 90f), (float)Game1.pixelZoom * this.scale, SpriteEffects.None, 0.86f);
            if ((double)this.scale == 1.0)
            {
                SpriteText.drawString(b, this.mailMessage[this.page], this.xPositionOnScreen + Game1.tileSize / 2, this.yPositionOnScreen + Game1.tileSize / 2, 999999, this.width - Game1.tileSize, 999999, 0.75f, 0.865f, false, -1, "", -1);
                foreach (ClickableComponent clickableComponent in this.itemsToGrab)
                {
                    b.Draw(this.letterTexture, clickableComponent.bounds, new Rectangle?(new Rectangle(0, 180, 24, 24)), Color.White);
                    if (clickableComponent.item != null)
                        clickableComponent.item.drawInMenu(b, new Vector2((float)(clickableComponent.bounds.X + 4 * Game1.pixelZoom), (float)(clickableComponent.bounds.Y + 4 * Game1.pixelZoom)), clickableComponent.scale);
                }
                if (this.learnedRecipe != null && this.learnedRecipe.Length > 0)
                {
                    string s = Game1.content.LoadString("Strings\\UI:LetterViewer_LearnedRecipe", (object)this.cookingOrCrafting);
                    SpriteText.drawStringHorizontallyCenteredAt(b, s, this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen + this.height - Game1.tileSize / 2 - SpriteText.getHeightOfString(s, 999999) * 2, 999999, this.width - Game1.tileSize, 9999, 0.65f, 0.865f, false, -1);
                    SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:LetterViewer_LearnedRecipeName", (object)this.learnedRecipe), this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen + this.height - Game1.tileSize / 2 - SpriteText.getHeightOfString("t", 999999), 999999, this.width - Game1.tileSize, 9999, 0.9f, 0.865f, 0 != 0, -1);
                }
                base.draw(b);
                if (this.page < this.mailMessage.Count - 1)
                    this.forwardButton.draw(b);
                if (this.page > 0)
                    this.backButton.draw(b);
                if (this.questID != -1)
                {
                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.acceptQuestButton.bounds.X, this.acceptQuestButton.bounds.Y, this.acceptQuestButton.bounds.Width, this.acceptQuestButton.bounds.Height, (double)this.acceptQuestButton.scale > 1.0 ? Color.LightPink : Color.White, (float)Game1.pixelZoom * this.acceptQuestButton.scale, true);
                    Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AcceptQuest"), Game1.dialogueFont, new Vector2((float)(this.acceptQuestButton.bounds.X + Game1.pixelZoom * 3), (float)(this.acceptQuestButton.bounds.Y + Game1.pixelZoom * 3)), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
                }
            }
            if (Game1.options.hardwareCursor)
                return;
            b.Draw(Game1.mouseCursors, new Vector2((float)Game1.getMouseX(), (float)Game1.getMouseY()), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
        }

        public void unload()
        {
            Game1.temporaryContent.Unload();
            Game1.temporaryContent = (LocalizedContentManager)null;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            this.receiveLeftClick(x, y, playSound);
        }
    }
}
