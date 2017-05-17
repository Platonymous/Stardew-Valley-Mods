using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.IO;

namespace Portraiture
{
    public class PortraitureMod : Mod
    {
        public static IModHelper helper;
        public static IMonitor monitor;
        public int tick = 0;

        public override void Entry(IModHelper helper)
        {
            string customContentFolder = Path.Combine(helper.DirectoryPath, "Portraits");

            if (!Directory.Exists(customContentFolder))
            {
                Directory.CreateDirectory(customContentFolder);
            }
            
            PortraitureMod.helper = helper;
            PortraitureMod.monitor = Monitor;

            SaveEvents.AfterLoad += SaveEvents_AfterLoad;

            GameEvents.FourthUpdateTick += GameEvents_FourthUpdateTick;

            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            MenuEvents.MenuClosed += MenuEvents_MenuClosed;
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;

        }

        private void SaveEvents_AfterLoad(object sender, System.EventArgs e)
        {
            TextureLoader.loadTextures();
        }

        private void GameEvents_FourthUpdateTick(object sender, System.EventArgs e)
        {
            PortraitureBox.displayAlpha -= 0.05f;
        }


        private void MenuEvents_MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            PortraitureBox.displayAlpha = 0;
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            string key = e.KeyPressed.ToString();

            if (key == "P" && Game1.activeClickableMenu is PortraitureBox)
            {
                TextureLoader.nextFolder();
                PortraitureBox.displayAlpha = 2;
            }
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
           
            if (Game1.activeClickableMenu is DialogueBox && (!(Game1.activeClickableMenu is PortraitureBox)))
            {
                DialogueBox oldBox = (DialogueBox)Game1.activeClickableMenu;
                NPC speaker = Game1.currentSpeaker;

                if (oldBox.isPortraitBox() == true && speaker != null && speaker.Portrait != null && !Helper.Reflection.GetPrivateValue<bool>(oldBox, "isQuestion"))
                {

                    Dialogue dialogue = speaker.CurrentDialogue.Peek();
                    Game1.activeClickableMenu = new PortraitureBox(dialogue);

                }

            }

        }
    }
}
