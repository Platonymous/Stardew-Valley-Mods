using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.IO;
using System.Reflection;

namespace Portraiture
{
    public class PortraitureMod : Mod
    {
        private float displayAlpha;
        public static IModHelper helper;
        private static Mod instance;
        internal static PConfig config;

        public override void Entry(IModHelper help)
        {
            helper = help;
            instance = this;
            config = Helper.ReadConfig<PConfig>();
            string customContentFolder = Path.Combine(helper.DirectoryPath, "Portraits");
            displayAlpha = 0;

            if (!Directory.Exists(customContentFolder))
                Directory.CreateDirectory(customContentFolder);

            help.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            help.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            harmonyFix();

            Helper.ConsoleCommands.Add("pmenu", "", (s, p) =>
             {
                 MenuLoader.OpenMenu(Game1.activeClickableMenu);
             });
        }
          private void harmonyFix()
        {
            Harmony instance = new Harmony("Platonymous.Portraiture");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void OnReturnedToTitle(object sender, EventArgs e)
        {
            helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            helper.Events.Display.MenuChanged -= OnMenuChanged;
            helper.Events.Input.ButtonPressed -= OnButtonPressed;
        }

        public static void log (string text)
        {
            instance.Monitor.Log(text);
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            TextureLoader.loadTextures();
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (e.IsMultipleOf(15)) // quarter second
                displayAlpha = Math.Max(displayAlpha - 0.05f, 0);
        }

        private void drawFolderName(SpriteBatch b, int x, int y)
        {
            if (displayAlpha <= 0)
                return;

            string activeFolderText = TextureLoader.getFolderName();

            activeFolderText = activeFolderText.Replace('_', ' ');

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

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if ((e.Button == config.changeKey || e.Button == config.menuKey) && Game1.activeClickableMenu is DialogueBox d && d.isPortraitBox() && Game1.currentSpeaker is NPC cs)
            {
                if (e.Button == config.changeKey)
                {
                    if (d.width < 107 * Game1.pixelZoom * 3 / 2 || Helper.Reflection.GetField<bool>(d, "transitioning").GetValue() || Helper.Reflection.GetField<bool>(d, "isQuestion").GetValue())
                        return;

                    TextureLoader.nextFolder();
                    displayAlpha = 2;
                }
                else
                    MenuLoader.OpenMenu(Game1.activeClickableMenu);
            }

        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            switch (e.NewMenu)
            {
                case null:
                    displayAlpha = 0;
                    Helper.Events.Display.RenderedActiveMenu -= OnRenderedActiveMenu;
                    break;

                case ShopMenu shopMenu when (shopMenu.portraitPerson is NPC npc && npc.Portrait is Texture2D t && Game1.options.showMerchantPortraits):
                    Helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
                    break;

                case DialogueBox box when (box.isPortraitBox() && Game1.currentSpeaker is NPC npc && npc.Portrait is Texture2D):
                    Helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
                    break;
            }
        }

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
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
