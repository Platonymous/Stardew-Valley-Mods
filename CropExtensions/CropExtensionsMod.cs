using StardewModdingAPI;
using HarmonyLib;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CropExtensions
{
    public interface IGMCMAPI
    {
        void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);

        void RegisterLabel(IManifest mod, string labelName, string labelDesc);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet);

        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max);

        void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices);

        void RegisterComplexOption(IManifest mod, string optionName, string optionDesc,
                                   Func<Vector2, object, object> widgetUpdate,
                                   Func<SpriteBatch, Vector2, object, object> widgetDraw,
                                   Action<object> onSave);
    }

    public class CropExtensionsMod : Mod
    {
        public static Config config;
        const char seperator1 = '|';
        const char seperator2 = '-';
        public static List<string> fourseasons = new List<string>() { "spring", "summer", "fall", "winter" };




        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<Config>();
            var instance = new Harmony("Platonymous.CropExtension");
            instance.Patch(typeof(HoeDirt).GetMethod("plant"), null, new HarmonyMethod(this.GetType().GetMethod("plant")));
            instance.Patch(typeof(HoeDirt).GetMethod("canPlantThisSeedHere"), null, new HarmonyMethod(this.GetType().GetMethod("canPlantThisSeedHere")));
            instance.Patch(typeof(Crop).GetMethod("newDay"), new HarmonyMethod(this.GetType().GetMethod("newDay")));

            helper.Events.GameLoop.GameLaunched += (s, e) => addMenu();
        }

        public static void plant(ref HoeDirt __instance, ref bool __result, int index, int tileX, int tileY, Farmer who, bool isFertilizer, GameLocation location)
        {
            if (__result == false || __instance.crop == null || !config.DetailedCropSeasons)
                return;

            if (!canGrow(__instance.crop))
            {
                __instance.crop = null;
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13924"));
                __result = false;
            }

        }

        public static void canPlantThisSeedHere(ref HoeDirt __instance, ref bool __result, int objectIndex, int tileX, int tileY, bool isFertilizer = false)
        {
            if (__result == false || isFertilizer || __instance.crop == null || !config.DetailedCropSeasons)
                return;

            if (!canGrow(__instance.crop))
                __result = false;
        }

        public static void newDay(ref Crop __instance, int state, int fertilizer, int xTile, int yTile, GameLocation environment)
        {
            if (!config.DetailedCropSeasons)
                return;

            if(!canGrow(__instance))
                __instance.dead.Value = true;
        }

        public static bool canGrow(Crop crop)
        {
            string name = Game1.objectInformation[crop.indexOfHarvest.Value].Split('/')[0];
            var seasonList = crop.seasonsToGrowIn.ToList();

            if (config.Presets.ContainsKey(name))
            {
                string startSeason = seasonList[0];
                string endSeason = seasonList.Last().Split('-')[0];
                int startDay = 1;
                int endDay = 28;

                if (seasonList.Exists(s => s.Contains(seperator2)))
                {
                    string[] details1 = seasonList.Find(s => s.Contains(startSeason) && s.Contains(seperator2)).Split(seperator1).ToList().Find(s => s.StartsWith(startSeason)).Split(seperator2);
                    string[] details2 = seasonList.Find(s => s.Contains(endSeason) && s.Contains(seperator2)).Split(seperator1).ToList().Find(s => s.StartsWith(endSeason)).Split(seperator2);
                    if (details1.Length > 1)
                        int.TryParse(details1[1], out startDay);
                    if (details2.Length > 2)
                        int.TryParse(details2[2], out endDay);
                }

                var data = config.Presets[name];
                startSeason = data.Seasons[0] == "default" ? startSeason : data.Seasons[0];
                endSeason = data.Seasons[1] == "default" ? endSeason : data.Seasons[1];
                startDay = data.Days[0] == 0 ? startDay : data.Days[0];
                endDay = data.Days[1] == 0 ? endDay : data.Days[1];

                seasonList = new List<string>();
                seasonList.Add(startSeason);

                string detailString = startSeason + seperator2 + startDay + seperator2;

                if (startSeason == endSeason)
                    detailString += endDay;
                else
                {
                    detailString += "28";
                    int i = fourseasons.IndexOf(startSeason);
                    while (!seasonList.Contains(endSeason))
                    {
                        i++;
                        if (i >= fourseasons.Count())
                            i = 0;
                        seasonList.Add(fourseasons[i]);
                        detailString += seperator1 + fourseasons[i];
                    }

                    detailString += seperator2 + "1" + endDay;
                }

                seasonList.Add(detailString);
            }


            if (seasonList.Contains(Game1.currentSeason) && seasonList is List<string> seasons && seasons.Exists(s => s.Contains(Game1.currentSeason) && s.Contains(seperator2)))
            {
                string[] details = seasons.Find(s => s.Contains(Game1.currentSeason) && s.Contains(seperator2)).Split(seperator1).ToList().Find(s => s.StartsWith(Game1.currentSeason)).Split(seperator2);
                int start = 0;
                if (details.Length > 1)
                    int.TryParse(details[1], out start);
                int end = 28;
                if (details.Length > 2)
                    int.TryParse(details[2], out end);

                if (Game1.dayOfMonth < start || Game1.dayOfMonth > end)
                    return false;
            }

            return true;

        }

        public void addMenu()
        {
            if (!Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
                return;

            string[] cSeasons = new string[] { "default", "spring", "summer", "fall", "winter" };

            string[] days = new string[29];

            for (int i = 0; i < 29; i++)
                days[i] = i.ToString();

            days[0] = "default";

            var api = Helper.ModRegistry.GetApi<IGMCMAPI>("spacechase0.GenericModConfigMenu");

            api.RegisterModConfig(ModManifest, () =>
             {
                 foreach (var crop in config.Presets.Keys)
                     config.Presets[crop] = new CropDetails();
             }, () =>
             {
                 Helper.WriteConfig<Config>(config);
             });


            foreach (var crop in Helper.Content.Load<Dictionary<int, string>>("Data/Crops", ContentSource.GameContent)){
                string[] cropInfo = Game1.objectInformation[int.Parse(crop.Value.Split('/')[3])].Split('/');
                string label = cropInfo[0];
                string description = cropInfo[5];

                if (!config.Presets.ContainsKey(label))
                    config.Presets.Add(label, new CropDetails());

                api.RegisterLabel(ModManifest, label, description);
                api.RegisterChoiceOption(ModManifest, "Start season", "", () => config.Presets[label].Seasons[0], (value) => config.Presets[label].Seasons[0] = value, cSeasons);
                api.RegisterClampedOption(ModManifest, "Start day", "", () => config.Presets[label].Days[0], (value) => config.Presets[label].Days[0] = (int)value,0,28);
                api.RegisterChoiceOption(ModManifest, "End season", "", () => config.Presets[label].Seasons[1], (value) => config.Presets[label].Seasons[1] = value, cSeasons);
                api.RegisterClampedOption(ModManifest, "End day", "", () => config.Presets[label].Days[1], (value) => config.Presets[label].Days[1] = (int)value, 0, 28);
            }

            Helper.WriteConfig<Config>(config);
        }

        

    }
}