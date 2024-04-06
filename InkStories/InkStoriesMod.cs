using StardewModdingAPI;
using System.IO;
using Ink;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Menus;
using System.Linq;
using HarmonyLib;
using System;
using StardewModdingAPI.Utilities;

namespace InkStories
{
    public class InkStoriesMod : Mod
    {

        public const string STOREASSET = @"Data/InkStories/Store";
        public const string STORIESASSET = @"Data/InkStories/Stories";
        public const string INKSAVEDATAID = "Platonymous.InkStories.Save";
        public const string INKFLAG = "INK ";
        public const string INKCALLFLAG = "INKCALL ";
        public static Dictionary<string, InkStory> Stories { get; } = new Dictionary<string, InkStory>();
        public static Dictionary<string, Dictionary<string, string>> Store = new Dictionary<string, Dictionary<string, string>>(); 
        public static Dictionary<string,List<string>> InksForNextDay { get; private set; } = new Dictionary<string,List<string>>();

        public static Dictionary<NPC, Stack<Dialogue>> ExtraDialogues { get; } = new Dictionary<NPC, Stack<Dialogue>>();


        public static IMonitor Mon { get; internal set; }

        public static InkStoriesMod Singleton { get; private set; }

        public override void Entry(IModHelper helper)
        {
            Singleton = this;
            Mon = Monitor;

            RegisterSMAPIEvents();
            AddConsoleCommands();
            InkPatches.Initialize();
        }

        private void RegisterSMAPIEvents()
        {
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.Saving += GameLoop_Saving;
            Helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.BaseName.StartsWith(PathUtilities.NormalizeAssetName(STOREASSET))){
                e.LoadFrom(() =>
                {
                    string id = Path.GetFileName(e.NameWithoutLocale.BaseName);

                    if (Store.TryGetValue(id, out Dictionary<string, string> store))
                        return store;

                    Store.Add(id, new Dictionary<string, string>());
                    return Store[id];
                }, StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void AddConsoleCommands()
        {
            Helper.ConsoleCommands.Add("ink", "Adds a dialogue from ink stories to an npc: ink npcname id>path (path is optional)", (s, p) =>
            {
                if (p.Length > 1)
                {
                    Monitor.Log("Trying to add InkStory " + p[1] + " to NPC " + p[0] + "...", LogLevel.Info);
                    if (Game1.getCharacterFromName(p[0]) is NPC npc && InkUtils.TryParseInkPath(p[1], out string id, out string path))
                    {
                        InkUtils.AddInkToNPC(id, path, npc);
                        Monitor.Log("...Success!", LogLevel.Info);
                    }
                    else
                        Monitor.Log("...Failed!", LogLevel.Error);

                }
                else
                    Monitor.Log("Missing parameters!", LogLevel.Error);
            }
                );

            Helper.ConsoleCommands.Add("inkreset", "Resets everything, including fixed values, usefull during testing.", (s, p) =>
            {
                Stories.Values.ToList().ForEach(story => story.Reset(true));
                InksForNextDay.Clear();
                Monitor.Log("All cleared!", LogLevel.Info);

            });
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            Stories.Values.ToList().ForEach(s => s.Reset(true));
            InksForNextDay.Clear();
        }

        private void GameLoop_Saving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            var save = new InkStorySave();

            foreach(var story in Stories.Values)
            {
                if (story.GetSaveData() is InkStorySaveData data)
                    save.Data.Add(data);
            }

            save.InksForNextDay = InksForNextDay;

            Helper.Data.WriteSaveData(INKSAVEDATAID, save);
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            try
            {
                if (Helper.Data.ReadSaveData<InkStorySave>(INKSAVEDATAID) is InkStorySave save)
                {
                    save.Data.ForEach(s =>
                    {
                        if (Stories.TryGetValue(s.Id, out InkStory story))    
                            story.LoadSaveData(s);
                    });

                    InksForNextDay = save.InksForNextDay ?? new Dictionary<string, List<string>>();
                }
            }
            catch
            {

            }
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (InksForNextDay.Count > 0)
                InksForNextDay.Keys.ToList().ForEach(k =>
                {
                    if (Game1.getCharacterFromName(k) is NPC npc)
                    {
                        InksForNextDay[k].ForEach(t =>
                        {
                            if (InkUtils.TryParseInkPath(t, out string id, out string path))
                                InkUtils.AddInkToNPC(id, path, npc);
                        });
                    }
                });

            InksForNextDay.Clear();
        }

        private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            foreach (var story in Stories.Values)
            {
                if (story.Loaded)
                {
                    bool shouldReset = true;

                    if(story.Instance.HasFunction("DayEnding"))
                        shouldReset = (bool)story.Instance.EvaluateFunction("DayEnding", Game1.dayOfMonth, Game1.currentSeason, Game1.year);

                    if (shouldReset)
                        story.Reset();
                }
            }

            foreach(var npc in ExtraDialogues.Keys)
                ExtraDialogues[npc].Clear();
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            if (Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher") is IContentPatcherAPI api)
            {
                api.RegisterToken(ModManifest, "Story", new InkStoriesToken());
                api.RegisterToken(ModManifest, "Store", new InkStoriesStoreToken());
            }

            LoadContentPacks();
        }

        private void LoadContentPacks()
        {
            foreach (var cp in Helper.ContentPacks.GetOwned())
            {
                ContentPack pack = cp.ReadJsonFile<ContentPack>("content.json");
                foreach (var story in pack.Stories)
                {
                    try
                    {
                        string source = File.ReadAllText(InkUtils.PlatformPath(Helper.DirectoryPath, story.FromFile));
                        bool isJson = story.FromFile.EndsWith(".json");
                        Stories.Add(story.Id, new InkStory(story.Id, source, isJson ? DataType.JSON : DataType.TEXT));
                    }
                    catch (Exception ex)
                    {
                        Mon.Log("Could not load " + story.Id, LogLevel.Error);
                        Mon.Log(ex.Message + ex.StackTrace, LogLevel.Error);
                    }
                }
            }
        }
    }
}
