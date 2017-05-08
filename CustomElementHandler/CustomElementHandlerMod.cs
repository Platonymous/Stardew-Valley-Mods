using System;

using StardewModdingAPI;
using StardewModdingAPI.Events;


namespace CustomElementHandler
{
    public class CustomElementHandlerMod : Mod
    {

        public override void Entry(IModHelper helper)
        {
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveHandler.Monitor = Monitor;
            
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
            SaveEvents.AfterLoad -= SaveEvents_AfterLoad;
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
