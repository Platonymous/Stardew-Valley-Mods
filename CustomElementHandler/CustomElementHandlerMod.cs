using System;

using StardewModdingAPI;
using StardewModdingAPI.Events;


namespace CustomElementHandler
{
    public class CustomElementHandlerMod : Mod
    {

        public bool pytk
        {
            get
            {
                return Helper.ModRegistry.IsLoaded("Platonymous.Toolkit");
            }
        }

        public override void Entry(IModHelper helper)
        {
            if(!pytk)
                SaveEvents.AfterLoad += SaveEvents_AfterLoad;

            SaveHandler.Monitor = Monitor;
            Helper.ConsoleCommands.Add("ceh", "[ceh cleanup] removes all custom element leftovers", cleanup);        
        }

        private void cleanup(string command, string[] args)
        {
            if (args[0] == "cleanup")
                if (pytk)
                    Helper.ConsoleCommands.Trigger("pytk_cleanup", new string[0]);
                else
                    SaveHandler.placeElements(true);
        }
        
        private void setUpEventHandlers()
        {
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            SaveEvents.AfterSave += SaveEvents_AfterSave;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;
        }

        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            SaveEvents.AfterSave -= SaveEvents_AfterSave;
            SaveEvents.BeforeSave -= SaveEvents_BeforeSave;
            SaveEvents.AfterReturnToTitle -= SaveEvents_AfterReturnToTitle;
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
          SaveHandler.removeElements();

        }

        private void SaveEvents_AfterSave(object sender, EventArgs e)
        {
            SaveHandler.placeElements();
    
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            setUpEventHandlers();
            SaveHandler.placeElements();
        }

    }
}
