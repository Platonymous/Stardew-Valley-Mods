using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;
using System;
using System.IO;
using Visualize;

namespace Portraiture
{
    public class PortraitureMod : Mod
    {
        private float displayAlpha;
        public static IModHelper helper;
        private static Mod instance;
        internal static PConfig config;
        internal static Texture2D activeTexure;
        private static IVisualizeHandler vHandler = new PortaitureVHandler();
        
        public override void Entry(IModHelper help)
        {
            helper = help;
            instance = this;
            config = Helper.ReadConfig<PConfig>();
            string customContentFolder = Path.Combine(helper.DirectoryPath, "Portraits");
            displayAlpha = 0;
            VisualizeMod.addHandler(vHandler);
            
            if (!Directory.Exists(customContentFolder))
                Directory.CreateDirectory(customContentFolder);

            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;
        }

        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            GameEvents.FourthUpdateTick -= GameEvents_FourthUpdateTick;
            MenuEvents.MenuClosed -= MenuEvents_MenuClosed;
            MenuEvents.MenuChanged -= MenuEvents_MenuChanged;
            ControlEvents.KeyPressed -= ControlEvents_KeyPressed;
        }

        public static void log (string text)
        {
            instance.Monitor.Log(text);
        }

        private void SaveEvents_AfterLoad(object sender, System.EventArgs e)
        {
            TextureLoader.loadTextures();
            GameEvents.FourthUpdateTick += GameEvents_FourthUpdateTick;
            MenuEvents.MenuClosed += MenuEvents_MenuClosed;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
        }

        private void GameEvents_FourthUpdateTick(object sender, System.EventArgs e)
        {
            displayAlpha = Math.Max(displayAlpha - 0.05f, 0);
        }

        private void drawFolderName(SpriteBatch b, int x, int y)
        {
            if (displayAlpha <= 0)
                return;

            string activeFolderText = TextureLoader.getFolderName();

            activeFolderText = activeFolderText.Replace('_', ' ');

            if (activeTexure is Texture2D texture && texture.Width > 128)
                activeFolderText = "= " + activeFolderText;

            int textlength = (int)Game1.smallFont.MeasureString(activeFolderText).X;
            int textheight = (int)Game1.smallFont.MeasureString(activeFolderText).Y;
            int padding = Game1.pixelZoom * 12;
            int displayBoxWidth = (int)textlength + padding;
            int displayBoxHeight = (int)textheight + padding / 2;

            Vector2 boxPos = new Vector2(x, y);
            Vector2 displayBoxPos = new Vector2(boxPos.X, boxPos.Y - (displayBoxHeight + padding));

            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), (int)displayBoxPos.X, (int)displayBoxPos.Y, displayBoxWidth, displayBoxHeight, Color.White * displayAlpha, 1f, false);
            if (displayAlpha >= 0.8)
                Utility.drawTextWithShadow(b, activeFolderText, Game1.smallFont, new Vector2(displayBoxPos.X + ((displayBoxWidth - textlength) / 2), Game1.pixelZoom + displayBoxPos.Y + ((displayBoxHeight - textheight) / 2)), Game1.textColor);
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            
            if (e.KeyPressed == config.changeKey && Game1.activeClickableMenu is DialogueBox d && d.isPortraitBox() && Game1.currentSpeaker is NPC cs)
            {
                if (d.width < 107 * Game1.pixelZoom * 3 / 2 || Helper.Reflection.GetField<bool>(d, "transitioning").GetValue() || Helper.Reflection.GetField<bool>(d, "isQuestion").GetValue())
                    return;

                TextureLoader.nextFolder();
                displayAlpha = 2;
            }

        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (Game1.activeClickableMenu is ShopMenu s && s.portraitPerson is NPC c && c.Portrait is Texture2D t && Game1.options.showMerchantPortraits)
                GraphicsEvents.OnPostRenderGuiEvent += GraphicsEvents_OnPostRenderGuiEvent;

            if (Game1.activeClickableMenu is DialogueBox d && d.isPortraitBox() && Game1.currentSpeaker is NPC ch && ch.Portrait is Texture2D ct)
                GraphicsEvents.OnPostRenderGuiEvent += GraphicsEvents_OnPostRenderGuiEvent;

        }

        private void MenuEvents_MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            displayAlpha = 0;
            GraphicsEvents.OnPostRenderGuiEvent -= GraphicsEvents_OnPostRenderGuiEvent;
        }

        private void GraphicsEvents_OnPostRenderGuiEvent(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is DialogueBox d && d.isPortraitBox() && Game1.currentSpeaker is NPC cs)
            {
                int x = Helper.Reflection.GetField<int>(d, "x").GetValue();
                int y = Helper.Reflection.GetField<int>(d, "y").GetValue();
                drawFolderName(Game1.spriteBatch, x, y);                
            }
        }


    }
}
