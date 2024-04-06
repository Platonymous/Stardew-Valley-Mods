using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static StardewValley.Menus.CoopMenu;
using static StardewValley.Menus.LoadGameMenu;

namespace InstantLoad
{
    public class InstantLoadMod : Mod
    {
        public static Config Options { get; set; }

        public static bool FirstLoad { get; set; } = false;

        public static IModHelper ModHelper { get; set; }

        public static IMonitor ModMonitor { get; set; }

        public static bool LoadEmpty { get; set; } = true;
        public static bool HasLoaded { get; set; } = false;

        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;
            ModMonitor = Monitor;
            Options = helper.ReadConfig<Config>();
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            var instance = new Harmony("Platonymous.InstantLoad");
            if (Options.EnableInstantLoad)
            {
                
                instance.Patch(
                    original: AccessTools.Method(typeof(LoadGameMenu), "FindSaveGames"),
                    prefix: new HarmonyMethod(typeof(InstantLoadMod), nameof(PrefixList)));

                instance.Patch(
                    original: AccessTools.Method(typeof(LoadGameMenu.SaveFileSlot), "Draw"),
                    prefix: new HarmonyMethod(typeof(InstantLoadMod), nameof(Block)));
                
                instance.Patch(
                    original: AccessTools.Method(typeof(Game1), "UpdateTitleScreen"),
                    prefix: new HarmonyMethod(typeof(InstantLoadMod), nameof(Block)));
            }
        }

        public static bool Block()
        {
            if (LoadEmpty && !HasLoaded)
                return false;

            return true;
        }


        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            Options.DebugCommands.ForEach(c => CheckForCommands("Start", c));
            if (Options.EnableInstantLoad)
            {
                LoadGameMenu lgm = Options.LoadHost ? new FakeCoopMenu(false) : new FakeLoadGameMenu();
                Game1.activeClickableMenu = lgm;
                if(Options.LoadHost)
                    ModHelper.Events.GameLoop.OneSecondUpdateTicked += TryLoadFirstSaveGame;
            }
        }


        public static bool PrefixList(ref List<Farmer> __result)
        {
            if (!FirstLoad && Options.EnableInstantLoad)
            {
                FirstLoad = true;
                __result = FindSaveGames();
                return false;
            }

            return true;
        }

        public static List<Farmer> FindSaveGames()
        {
            List<Farmer> results = new List<Farmer>();
            string pathToDirectory = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves"));
            DirectoryInfo dirinfo = new DirectoryInfo(pathToDirectory);
            if (Directory.Exists(pathToDirectory))
            {
                foreach (string s in dirinfo.EnumerateDirectories().OrderByDescending(d => d.LastWriteTime).Select(s => s.FullName))
                {
                    string saveName = s.Split(Path.DirectorySeparatorChar).Last();
                    string pathToFile = Path.Combine(pathToDirectory, s, "SaveGameInfo");
                    if (!File.Exists(Path.Combine(pathToDirectory, s, saveName)))
                    {
                        continue;
                    }
                    Farmer f = new FakeFarmer();
                    if (Options.LoadHost)
                    {
                            f = (Farmer)SaveGame.farmerSerializer.Deserialize(File.OpenRead(pathToFile));
                        SaveGame.loadDataToFarmer(f);
                    }
                    
                        f.slotName = saveName;
                        if (!Options.LoadHost || f.slotCanHost)
                        {
                            LoadEmpty = Options.LoadHost ? false : true;
                            return new List<Farmer> { f };
                        }
                }
            }

            HasLoaded = true;
            results.Sort();
            return results;
        }

        public void RunDebugDay(string name, string result)
        {
            Monitor.Log("RunDebugDay: " + name + " " + result, LogLevel.Warn);
        }

        public void RunDebugStart(string name, string result)
        {
            Monitor.Log("RunDebugStart: " + name + " " + result, LogLevel.Warn);
        }

        public void RunDebugLoad(string name, string result)
        {
            Monitor.Log("RunDebugLoad: " + name + " " + result, LogLevel.Debug);
        }

        public static void RunCommand(DebugTrigger c)
        {
            try
            {
                object manager = AccessTools.Field(ModHelper.ConsoleCommands.GetType(), "CommandManager").GetValue(ModHelper.ConsoleCommands);
                AccessTools.Method(manager.GetType(), "Trigger").Invoke(manager, new object[] { c.Command, c.Args.ToArray() });
            }
            catch (Exception e)
            {
                ModMonitor.Log("Could not execute ConsoleCommand " + c.Command + " with arguments " + String.Join(' ', c.Args), LogLevel.Error);
                ModMonitor.Log(e.Message + e.StackTrace, LogLevel.Debug);
            }
        }

        public static void RunModMethod(DebugTrigger c)
        {
            try
            {
                if (ModHelper.ModRegistry.IsLoaded(c.Target)
                    && ModHelper.ModRegistry.Get(c.Target) is IModInfo m
                    && AccessTools.Property(m.GetType(), "Mod") is PropertyInfo p
                    && p.GetValue(m) is Mod mod
                    && AccessTools.Method(mod.GetType(), c.Command) is MethodInfo method)
                    method.Invoke(mod, c.Args.ToArray());

            }
            catch (Exception e)
            {
                ModMonitor.Log("Could not execute Method " + c.Command + " from Mod " + c.Target + " with arguments " + String.Join(' ', c.Args), LogLevel.Error);
                ModMonitor.Log(e.Message + e.StackTrace, LogLevel.Debug);
            }
        }

        public static void CheckForCommands(string type, DebugTrigger c)
        {
            if (Options.EnableDebugCommands && c.Event == type)
            {
                if (c.Target == "Console")
                    RunCommand(c);
                else if (ModHelper.ModRegistry.IsLoaded(c.Target))
                    RunModMethod(c);
            }
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            HasLoaded = true;
            Options.DebugCommands.ForEach(c => CheckForCommands("Day", c));
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            Options.DebugCommands.ForEach(c => CheckForCommands("Load", c));
        }

        public static void TryLoadFirstSaveGame(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if (Game1.activeClickableMenu is CoopMenu lgm && AccessTools.Method(typeof(LoadGameMenu), "checkListPopulation").Invoke(lgm, new object[] { }) is bool b && !b)
            {
                if (Game1.activeClickableMenu is CoopMenu coop)
                {
                    coop.SetTab(Tab.HOST_TAB, false);
                    if (AccessTools.Field(typeof(CoopMenu), "currentTab").GetValue(coop) is Tab tab && tab == Tab.HOST_TAB)
                        StartFirstSave(coop);
                }
                else
                    StartFirstSave(lgm);
            }
        }

        public static void StartFirstSave(LoadGameMenu lgm)
        {
            List<MenuSlot> slots = (List<MenuSlot>)AccessTools.Property(typeof(LoadGameMenu), "MenuSlots").GetValue(lgm);
            if (slots.Count > 0)
            {
                ModHelper.Events.GameLoop.OneSecondUpdateTicked -= TryLoadFirstSaveGame;
                AccessTools.Field(typeof(LoadGameMenu), "timerToLoad").SetValue(lgm, 1500);
                AccessTools.Field(typeof(LoadGameMenu), "selected").SetValue(lgm, lgm is CoopMenu ? 1 : 0);
            }
        }

    }

    public class FakeCoopMenu : CoopMenu
    {
        public FakeCoopMenu(bool tooManyFarms)
            : base(tooManyFarms)
        {

        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
        }
        public override void draw(SpriteBatch b)
        {
        }

        public override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
        {
        }

        public override void drawBackground(SpriteBatch b)
        {
        }


    }

    public class FakeLoadGameMenu : LoadGameMenu
    {
        public FakeLoadGameMenu()
        {
            MenuSlots.AddRange(((IEnumerable<Farmer>)InstantLoadMod.FindSaveGames()).Select((Func<Farmer, MenuSlot>)((Farmer file) => new SaveFileSlot(this, file))));
            AccessTools.Field(typeof(LoadGameMenu), "timerToLoad").SetValue(this, 1500);
            AccessTools.Field(typeof(LoadGameMenu), "selected").SetValue(this, 0);
        }

        public override void draw(SpriteBatch b)
        {
        }

        public override void draw(SpriteBatch b, int red = -1, int green = -1, int blue = -1)
        {
        }

        public override void drawBackground(SpriteBatch b)
        {
        }

        public override void update(GameTime time)
        {
            timerToLoad -= time.ElapsedGameTime.Milliseconds;
            if (timerToLoad <= 0)
            {
                if (MenuSlots.Count > selected)
                {
                    MenuSlots[selected].Activate();
                }
                else
                {
                    Game1.ExitToTitle();
                }
            }
        }
    }

    public class FakeFarmer : Farmer
    {
        public FakeFarmer()
        {

        }
    }

}
