using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using PyTK.Types;
using PyTK.Extensions;
using StardewValley.Menus;
using System.Collections.Generic;
using StardewValley;

namespace HarpOfYobaRedux
{
    public class HarpOfYobaReduxMod : Mod
    {
        public static Config config;

        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<Config>();
            new ConsoleCommand("hoy_cheat", "Get all sheets without doing anything.", (c, p) => cheat(p)).register();
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            DataLoader.load(Helper, Helper.ModRegistry.IsLoaded("Platonymous.CustomMusic"));
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

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            Delivery.checkMail();
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
    }
}
