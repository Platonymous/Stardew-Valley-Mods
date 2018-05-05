using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using PyTK.CustomElementHandler;
using PyTK.Types;
using PyTK.Extensions;
using StardewValley.Menus;
using System.Collections.Generic;
using StardewValley;

namespace HarpOfYobaRedux
{
    public class HarpOfYobaReduxMod : Mod
    {
        private DataLoader data;
        public static Config config;
        public static IMonitor monitor;
        public static IModHelper helper;

        public override void Entry(IModHelper helper)
        {
            monitor = Monitor;
            HarpOfYobaReduxMod.helper = Helper;
            config = Helper.ReadConfig<Config>();
            new ConsoleCommand("hoy_cheat", "Get all sheets without doing anything.", (c, p) => cheat(p)).register();
            SaveHandler.BeforeRebuilding += SaveHandler_BeforeRebuilding;
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;
        }

        private void cheat(string[] p)
        {
            if (p == null || p.Length < 1)
            {
                List<string> list = SheetMusic.allSheets.toList(d => d.Key);
                list.Add("harp");
                Monitor.Log(String.Join(" - ", list),LogLevel.Info);
            }

            List<Item> items = new List<Item>();

            foreach (string s in p)
            {
                Monitor.Log(s);
                if (s == "harp")
                    items.AddOrReplace(new Instrument("harp"));
                else if (SheetMusic.allSheets.ContainsKey(s))
                    items.AddOrReplace(new SheetMusic(s));
                else if (s == "allsheets")
                    foreach(string sheet in SheetMusic.allSheets.Keys)
                        items.AddOrReplace(new SheetMusic(sheet));
            }
            if(items.Count > 0)
                Game1.activeClickableMenu = new ItemGrabMenu(items);
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
