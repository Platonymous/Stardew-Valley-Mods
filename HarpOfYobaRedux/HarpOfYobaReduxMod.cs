using System;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using PyTK.CustomElementHandler;
using StardewValley.Menus;


namespace HarpOfYobaRedux
{
    public class HarpOfYobaReduxMod : Mod
    {
        private DataLoader data;

        public override void Entry(IModHelper helper)
        {

            SaveHandler.BeforeRebuilding += SaveHandler_BeforeRebuilding;
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;
            
        }

     
        

        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            ControlEvents.KeyPressed -= ControlEvents_KeyPressed;
            MenuEvents.MenuChanged -= MenuEvents_MenuChanged;
            SaveHandler.BeforeRebuilding -= SaveHandler_BeforeRebuilding2;
            TimeEvents.TimeOfDayChanged -= TimeEvents_TimeOfDayChanged;
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            SaveHandler.BeforeRebuilding += SaveHandler_BeforeRebuilding2;
            SaveHandler_BeforeRebuilding(sender, e);
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            TimeEvents.TimeOfDayChanged += TimeEvents_TimeOfDayChanged;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
        }

        private void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
            if(e.NewInt == 700 || e.NewInt == 1200 || e.NewInt == 18)
            {
                Delivery.checkMail();
            }
            
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if(e.NewMenu is LetterViewerMenu && !Delivery.showsLetter)
            {
                Delivery.showLetter();
            }
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            string key = e.KeyPressed.ToString().ToLower();

            if (key == "h")
            {
             
            }
        }

        private void SaveHandler_BeforeRebuilding(object sender, EventArgs e)
        {
            if (data == null)
            {
                data = new DataLoader(Helper);
                DataLoader.load();
            }

            SaveHandler.BeforeRebuilding -= SaveHandler_BeforeRebuilding;


        }

        private void SaveHandler_BeforeRebuilding2(object sender, EventArgs e)
        {
            Instrument.beforeRebuilding();
            SheetMusic.beforeRebuilding();

        }
    }
}
