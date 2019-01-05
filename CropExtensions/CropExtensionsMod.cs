using StardewModdingAPI;
using Harmony;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace CropExtensions
{
    public class CropExtensionsMod : Mod
    {
        internal static Config config;
        const char seperator1 = '|';
        const char seperator2 = '-';
        public static bool skip = false;
        public static bool active = true;
        public static bool working = false;

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<Config>();
            var instance = HarmonyInstance.Create("Platonymous.CropExtension");
            instance.Patch(typeof(HoeDirt).GetMethod("plant"), null, new HarmonyMethod(this.GetType().GetMethod("plant")));
            instance.Patch(typeof(HoeDirt).GetMethod("canPlantThisSeedHere"), null, new HarmonyMethod(this.GetType().GetMethod("canPlantThisSeedHere")));
            instance.Patch(typeof(Crop).GetMethod("newDay"), new HarmonyMethod(this.GetType().GetMethod("newDay")));
            instance.Patch(typeof(GameLocation).GetMethod("UpdateWhenCurrentLocation"), new HarmonyMethod(this.GetType().GetMethod("UpdateWhenCurrentLocation")));
            instance.Patch(typeof(Game1).GetMethod("UpdateLocations", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance), new HarmonyMethod(this.GetType().GetMethod("UpdateLocations")));
            instance.Patch(typeof(Game1).GetMethod("UpdateCharacters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), new HarmonyMethod(this.GetType().GetMethod("UpdateCharacters")));
            instance.Patch(typeof(GameLocation).GetMethod("updateCharacters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), new HarmonyMethod(this.GetType().GetMethod("updateCharacters")));

            
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(e.Button == SButton.K)
            {
                active = !active;
                Monitor.Log("Active:" + active);
            }
        }

        public static bool UpdateWhenCurrentLocation(GameLocation __instance, GameTime time)
        {
            if (!active || skip)
            {
                skip = false;
                return true;
            }

            skip = true;
            Task.Run(() =>
            {
                try
                {
                    typeof(GameLocation).GetMethod("UpdateWhenCurrentLocation").Invoke(__instance, new[] { time });
                }
                catch
                {

                }
            });

            return false;
        }

        public static bool UpdateLocations(Game1 __instance, GameTime time)
        {
            if (!active || skip)
            {
                skip = false;
                return true;
            }

            skip = true;
            Task.Run(() =>
            {
                try
                {
                    typeof(Game1).GetMethod("UpdateLocations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(__instance, new[] { time });
                }
                catch
                {

                }
            });

            return false;
        }

        public static bool UpdateCharacters(Game1 __instance, GameTime time)
        {
            if (!active || skip)
            {
                skip = false;
                return true;
            }

            skip = true;
            Task.Run(() =>
            {
                try
                {
                    typeof(Game1).GetMethod("UpdateCharacters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(__instance, new[] { time });
                }
                catch
                {

                }
            });

            return false;
        }

        public static bool updateCharacters(GameLocation __instance, GameTime time)
        {
            if (!active || skip)
            {
                skip = false;
                return true;
            }

            skip = true;
            Task.Run(() =>
            {
                try
                {
                    typeof(GameLocation).GetMethod("updateCharacters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(__instance, new[] { time });
                }
                catch
                {

                }
            });

            return false;
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
            if (crop.seasonsToGrowIn.Contains(Game1.currentSeason) && crop.seasonsToGrowIn.ToList() is List<string> seasons && seasons.Exists(s => s.Contains(Game1.currentSeason) && s.Contains(seperator2)))
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

    }
}