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
            MenuEvents.MenuChanged -= MenuEvents_MenuChanged;
            SaveHandler.BeforeRebuilding -= SaveHandler_BeforeRebuilding2;
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            SaveHandler.BeforeRebuilding += SaveHandler_BeforeRebuilding2;
            SaveHandler_BeforeRebuilding(sender, e);

            TimeEvents.AfterDayStarted += (s,ev) => Delivery.checkMail();
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (e.NewMenu is LetterViewerMenu lvm)
            {
                string mailTitle = Helper.Reflection.GetField<string>(lvm, "mailTitle").GetValue();
                if (mailTitle.StartsWith("hoy_"))
                    lvm.itemsToGrab[0].item = DataLoader.getLetter(mailTitle).item;
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
