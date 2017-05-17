using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;

namespace Portraiture
{
    public class PortraitureMod : Mod
    {
        private ShopMenu activeShop;
        private DialogueBox activeDialogueBox;
        private float displayAlpha;
        public static IModHelper helper;
        internal static PConfig config;

        public override void Entry(IModHelper help)
        {
            helper = help;
            config = Helper.ReadConfig<PConfig>();
            string customContentFolder = Path.Combine(helper.DirectoryPath, "Portraits");
            displayAlpha = 0;

            if (!Directory.Exists(customContentFolder))
            {
                Directory.CreateDirectory(customContentFolder);
            }

            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
        }

        private void SaveEvents_AfterLoad(object sender, System.EventArgs e)
        {
            TextureLoader.loadTextures();
            GameEvents.FourthUpdateTick += GameEvents_FourthUpdateTick;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            MenuEvents.MenuClosed += MenuEvents_MenuClosed;
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
        }

        private void GameEvents_FourthUpdateTick(object sender, System.EventArgs e)
        {
            displayAlpha -= 0.05f;
        }

        private void drawFolderName(SpriteBatch b, int textureSize, int x, int y)
        {
            string activeFolderText = TextureLoader.getFolderName();

            activeFolderText = activeFolderText.Replace('_', ' ');

            if (textureSize > 64)
            {
                activeFolderText = "= " + activeFolderText;
            }

            int textlength = (int)Game1.smallFont.MeasureString(activeFolderText).X;
            int textheight = (int)Game1.smallFont.MeasureString(activeFolderText).Y;
            int padding = Game1.pixelZoom * 12;
            int displayBoxWidth = (int)textlength + padding;
            int displayBoxHeight = (int)textheight + padding / 2;

            Vector2 boxPos = new Vector2(x, y);
            Vector2 displayBoxPos = new Vector2(boxPos.X, boxPos.Y - (displayBoxHeight + padding));

            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), (int)displayBoxPos.X, (int)displayBoxPos.Y, displayBoxWidth, displayBoxHeight, Color.White * displayAlpha, 1f, false);
            if (displayAlpha >= 0.8)
            {
                Utility.drawTextWithShadow(b, activeFolderText, Game1.smallFont, new Vector2(displayBoxPos.X + ((displayBoxWidth - textlength) / 2), Game1.pixelZoom + displayBoxPos.Y + ((displayBoxHeight - textheight) / 2)), Game1.textColor);
            }

        }

        private void drawPortraiture(SpriteBatch b, DialogueBox d, NPC cs)
        {
            if (d.width < 107 * Game1.pixelZoom * 3 / 2 || Helper.Reflection.GetPrivateValue<bool>(d, "transitioning") || Helper.Reflection.GetPrivateValue<bool>(d, "isQuestion"))
            {
                return;
            }
            
            Texture2D texture = TextureLoader.getPortrait(cs.name);
            
            Dialogue characterDialogue = Helper.Reflection.GetPrivateValue<Dialogue>(d, "characterDialogue");
            int textureSize = Math.Max(texture.Width / 2, 64);

            int x = Helper.Reflection.GetPrivateValue<int>(d, "x");
            int y = Helper.Reflection.GetPrivateValue<int>(d, "y");

            int num1 = x + d.width - 112 * Game1.pixelZoom + Game1.pixelZoom;
            int num2 = x + d.width - num1;
            int num3 = num1 + Game1.pixelZoom * 19;
            int num4 = y + d.height / 2 - 74 * Game1.pixelZoom / 2 - 18 * Game1.pixelZoom / 2;

            int num5 = Helper.Reflection.GetPrivateMethod(d, "shouldPortraitShake").Invoke<bool>(new object[] { characterDialogue }) ? Game1.random.Next(-1, 2) : 0;

            Rectangle rectangle = Game1.getSourceRectForStandardTileSheet(texture, characterDialogue.getPortraitIndex(), textureSize, textureSize);
            if (!texture.Bounds.Contains(rectangle))
            {
                rectangle = new Rectangle(0, 0, textureSize, textureSize);
            }

            b.Draw(texture, new Rectangle(num3 + 4 * Game1.pixelZoom + num5, num4 + 6 * Game1.pixelZoom, 64 * Game1.pixelZoom, 64 * Game1.pixelZoom), new Rectangle?(rectangle), Color.White, 0f,Vector2.Zero,SpriteEffects.None, 0.88f);
            if (!Game1.options.hardwareCursor)
            {
                d.drawMouse(b);
            }
            drawFolderName(b,textureSize,x,y);
        }

        private void drawShopkeeper(SpriteBatch b, NPC c)
        {
            Texture2D texture = TextureLoader.getPortrait(c.name);

            int textureSize = Math.Max(texture.Width / 2, 64);
            b.Draw(texture, new Rectangle((activeShop.xPositionOnScreen - 80 * Game1.pixelZoom + Game1.pixelZoom * 5), (activeShop.yPositionOnScreen + Game1.pixelZoom * 5), 64 * Game1.pixelZoom, 64 * Game1.pixelZoom), new Rectangle?(new Rectangle(0, 0, textureSize, textureSize)), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.92f);
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            
            if (e.KeyPressed == config.changeKey && Game1.activeClickableMenu is DialogueBox d && d.isPortraitBox() && Game1.currentSpeaker is NPC cs)
            {
                if (d.width < 107 * Game1.pixelZoom * 3 / 2 || Helper.Reflection.GetPrivateValue<bool>(d, "transitioning") || Helper.Reflection.GetPrivateValue<bool>(d, "isQuestion"))
                {
                    return;
                }

                TextureLoader.nextFolder();
                displayAlpha = 2;
            }

        }

        private void MenuEvents_MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            displayAlpha = 0;
            GraphicsEvents.OnPostRenderGuiEvent -= GraphicsEvents_OnPostRenderGuiEvent;
        }


        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            

            if(Game1.currentSpeaker is NPC n && n.Portrait is Texture2D nt)
            {
                TextureLoader.loadVanillaPortraits();
            }


            if (Game1.activeClickableMenu is ShopMenu s && s.portraitPerson is NPC c && c.Portrait is Texture2D t && Game1.options.showMerchantPortraits)
            {
                activeShop = s;
                TextureLoader.pushToVanilla(c);
                GraphicsEvents.OnPostRenderGuiEvent += GraphicsEvents_OnPostRenderGuiEvent;
            }

            if (Game1.activeClickableMenu is DialogueBox d && d.isPortraitBox() && Game1.currentSpeaker is NPC ch && ch.Portrait is Texture2D ct)
            {
                activeDialogueBox = d;
                
                TextureLoader.pushToVanilla(ch);
                GraphicsEvents.OnPostRenderGuiEvent += GraphicsEvents_OnPostRenderGuiEvent;
            }

        }

        private void GraphicsEvents_OnPostRenderGuiEvent(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is ShopMenu s && s.portraitPerson is NPC c && c.Portrait is Texture2D && Game1.options.showMerchantPortraits && Game1.viewport.Width > 800)
            {
                drawShopkeeper(Game1.spriteBatch, c);
            }

            if (Game1.activeClickableMenu is DialogueBox d && d.isPortraitBox() && Game1.currentSpeaker is NPC cs)
            {
                drawPortraiture(Game1.spriteBatch, d, cs);
            }
        }

       
    }
}
