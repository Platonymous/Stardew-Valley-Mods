using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using System.Collections.Generic;
using StardewValley;
using System.Linq;
using SpaceShared.APIs;
using static StardewValley.Minigames.BoatJourney;

namespace HarpOfYobaRedux
{
    public class HarpOfYobaReduxMod : Mod
    {
        public static Config config;
        public static IModHelper modHelper;

        public override void Entry(IModHelper helper)
        {
            modHelper = helper;
            config = Helper.ReadConfig<Config>();
            Helper.ConsoleCommands.Add("hoy_cheat", "Get all sheets without doing anything.", (c, p) => cheat(p));
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                Instrument.allAdditionalSaveData = Helper.Data.ReadSaveData<SaveData>("hoy.savedata")?.Data ?? new Dictionary<string, string>();
            }
            catch
            {
                Instrument.allAdditionalSaveData = new Dictionary<string, string>();
            }
        }

        /// <summary>The method called after a new day starts.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            foreach (var item in SheetMusic.allSheets)
            {
                item.Value.playedToday = false;
            }
        }

        private void cheat(string[] p)
        {
            if (p == null || p.Length < 1)
            {
                List<string> list = SheetMusic.allSheets.Select(d => d.Key).ToList();
                list.Add("harp");
                list.Add("all");
                Monitor.Log(String.Join(" - ", list), LogLevel.Info);
            }

            List<Item> items = new List<Item>();

            foreach (string s in p)
            {
                Monitor.Log(s);
                if (s == "harp")
                {
                    items.RemoveAll(i => i is Instrument inst && inst.instrumentID == "harp");
                    items.Add(new Instrument("harp"));
                }
                else if (SheetMusic.allSheets.ContainsKey(s))
                {
                    items.RemoveAll(i => i is SheetMusic inst && inst.sheetMusicID == s);
                    items.Add(new SheetMusic(s));
                }
                else if (s == "allsheets")
                {
                    foreach (string sheet in SheetMusic.allSheets.Keys)
                    {
                        items.RemoveAll(i => i is SheetMusic inst && inst.sheetMusicID == sheet);
                        items.Add(new SheetMusic(sheet));
                    }
                }
                else if (s == "all")
                {
                    items.Clear();
                    items.Add(new Instrument("harp"));
                    foreach (string sheet in SheetMusic.allSheets.Keys)
                        items.Add(new SheetMusic(sheet));
                }
            }
            if (items.Count > 0)
                Game1.activeClickableMenu = new ItemGrabMenu(items);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var spaceCore = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            spaceCore.RegisterSerializerType(typeof(Instrument));
            spaceCore.RegisterSerializerType(typeof(SheetMusic));
            DataLoader.load(Helper, Helper.ModRegistry.IsLoaded("Platonymous.CustomMusic"));
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            var save = new SaveData() { Data = Instrument.allAdditionalSaveData };
            if (Context.IsMainPlayer)
                Helper.Data.WriteSaveData("hoy.savedata", save);

            Delivery.checkMail();
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is LetterViewerMenu lvm)
            {
                string mailTitle = Helper.Reflection.GetField<string>(lvm, "mailTitle").GetValue();
                if (mailTitle is string mt && mt.StartsWith("hoy_"))
                    lvm.itemsToGrab[0].item = DataLoader.getLetter(mt).item;
            }
        }
    }
}
