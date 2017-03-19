using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;

namespace Portraiture
{
    public class PortraitureMod : Mod
    {
        public IModHelper helper;
        public int tick = 0;

        public override void Entry(IModHelper helper)
        {
            this.helper = helper;
            ImageHelper.helper = helper;
            ImageHelper.monitor = Monitor;
            PortraitureDialogueBoxNew.Monitor = Monitor;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            GameEvents.EighthUpdateTick += GameEvents_EighthUpdateTick;
            
        }

        private void GameEvents_EighthUpdateTick(object sender, System.EventArgs e)
        {
            tick++;
            PortraitureDialogueBoxNew.totalTick = tick;
        }

        private void SaveEvents_AfterLoad(object sender, System.EventArgs e)
        {
            ImageHelper.loadTextureFolders();
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            string key = e.KeyPressed.ToString();   

            if (key == "P")
            {
                ImageHelper.nextFolder();
            }
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {

            if (Game1.activeClickableMenu is DialogueBox && (!(Game1.activeClickableMenu is PortraitureDialogueBoxNew)))
            {
                DialogueBox oldBox = (DialogueBox) Game1.activeClickableMenu;

                Dialogue dialogue = (Dialogue) this.helper.Reflection.GetPrivateField<Dialogue>(oldBox, "characterDialogue").GetValue();
                
                if (dialogue != null && dialogue.speaker != null && dialogue.speaker.Portrait != null)
                {

                  
                        Game1.activeClickableMenu = new PortraitureDialogueBoxNew(dialogue);
                   
                    
                }
                
               
            }

        }
    }
}
