using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace TMXLoader
{
    internal class OvLocations
    {
        internal static IModHelper Helper { get; } = TMXLoaderMod.helper;
        internal static IMonitor Monitor { get; } = TMXLoaderMod.monitor;
        internal static Dictionary<int, Rectangle> rectangleCache = new Dictionary<int, Rectangle>();
        internal static Dictionary<string, Func<string, string, GameLocation, bool>> eventConditions = new Dictionary<string, Func<string,string, GameLocation, bool>>();
        internal static bool skip = false;


        public static void GameLocationConstructor(GameLocation __instance)
        {
            if (__instance.map is xTile.Map map)
            {
                if (map.Properties.ContainsKey("IsGreenHouse") || map.Properties.ContainsKey("IsGreenhouse"))
                    __instance.IsGreenhouse = true;

                if (map.Properties.ContainsKey("IsStructure"))
                    __instance.isStructure.Value = true;

                if (map.Properties.ContainsKey("IsFarm"))
                    __instance.IsFarm = true;
            }
        } 

        [HarmonyPatch]
        internal class GLBugFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(PyUtils.getTypeSDV("GameLocation"), "Equals", new[] { PyUtils.getTypeSDV("GameLocation") });
            }

            internal static bool Prefix(GameLocation __instance, GameLocation other, ref bool __result)
            {
                try
                {
                    if (__instance == null)
                        return other == null;

                    __result =
                        other != null
                        && object.Equals(__instance.Name, other.Name)
                        && object.Equals(__instance.uniqueName.Value, other.uniqueName.Value)
                        && object.Equals(__instance.isStructure.Value, (object)other.isStructure.Value);
                    return false;
                }
                catch
                {
                    return true;
                }
            }
        }
        [HarmonyPatch]
        internal class EventConditionsFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkEventPrecondition), new Type[] {typeof(string), typeof(bool)});
            }

            internal static bool Prefix(GameLocation __instance, ref string precondition,ref string __result)
            {
                string t = "M " + (Game1.player.Money - 1);
                string f = "M " + (Game1.player.Money + 1);

                foreach(var entry in eventConditions)
                {
                    if(precondition.Contains("/!" + entry.Key + " ") || precondition.StartsWith("!" + entry.Key + " ") || precondition.Contains("/" + entry.Key + " ") || precondition.StartsWith(entry.Key + " "))
                    {
                        string[] conditions = precondition.Split('/');
                        for (int i = 0; i < conditions.Length; i++)
                        {
                            if (conditions[i].StartsWith(entry.Key) || conditions[i].StartsWith("!"+entry.Key))
                            {
                                bool comp = true;

                                if (conditions[i].StartsWith("!" + entry.Key))
                                {
                                    conditions[i] = conditions[i].Substring(1);
                                    comp = false;
                                }

                                if (entry.Value.Invoke(entry.Key, conditions[i], __instance) == comp)
                                    conditions[i] = t;
                                else
                                {
                                    conditions[i] = f;
                                    __result = "-1";
                                    return false;
                                }

                            }
                        }

                        precondition = string.Join("/", conditions);
                    }
                }

                return true;
            }
        }


        [HarmonyPatch]
        internal class TouchActionFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTouchAction), new Type[] { typeof(string), typeof(Vector2) });
            }

            internal static void Postfix(GameLocation __instance, string fullActionString, Vector2 playerStandingPosition)
            {
                if (TileAction.getCustomAction(fullActionString) is TileAction customAction)
                    TileAction.invokeCustomTileActions("TouchAction", Game1.currentLocation, playerStandingPosition, "Back");
            }
        }

        [HarmonyPatch]
        internal class ActionableFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isActionableTile));
            }

            internal static void Postfix(GameLocation __instance, ref bool __result, int xTile, int yTile)
            {
                string conditions = __instance.doesTileHaveProperty(xTile, yTile, "Conditions", "Buildings");
                string fallback = __instance.doesTileHaveProperty(xTile, yTile, "Fallback", "Buildings");

                if (__instance.doesTileHaveProperty(xTile, yTile, "Action", "Buildings") is string action)
                    if (TileAction.getCustomAction(action, conditions, fallback) != null)
                        Game1.isInspectionAtCurrentCursorTile = true;
            }
        }

        [HarmonyPatch]
        internal class ActionFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(typeof(Game1), nameof(Game1.tryToCheckAt));
            }

            internal static void Postfix(Vector2 grabTile, ref bool __result)
            {
                GameLocation location = Game1.currentLocation;
                if (!Utility.tileWithinRadiusOfPlayer((int)grabTile.X, (int)grabTile.Y, 1, Game1.player))
                    return;

                if (Game1.currentLocation.doesTileHaveProperty((int)grabTile.X, (int)grabTile.Y, "Action", "Buildings") is string action && TileAction.getCustomAction(action) is TileAction customAction)
                    __result = TileAction.invokeCustomTileActions("Action", Game1.currentLocation, grabTile, "Buildings");
            }

        }

        [HarmonyPatch]
        internal class CrittersFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(typeof(GameLocation), nameof(GameLocation.tryToAddCritters));
            }

            internal static bool Prefix(GameLocation __instance)
            {
                if (__instance.map.Properties.ContainsKey("Outdoors") && __instance.map.Properties["Outdoors"] == "F")
                    return false;
                else
                    return true;
            }
        }

        [HarmonyPatch]
        internal class LocationRequestFix1
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(typeof(Game1), nameof(Game1.getLocationRequest));
            }

            internal static void Prefix(ref string locationName, bool isStructure = false)
            {
                if (!locationName.Contains(":") || locationName == null || isStructure)
                    return;

                string locationMap = Path.Combine("Maps", locationName.Split(':')[0]);
                locationName = locationMap + "_" + locationName.Split(':')[1];

                GameLocation location = null;
                try
                {
                    location = Game1.getLocationFromName(locationName, isStructure);
                }
                catch
                {

                }

                if (location == null)
                {
                    try
                    {
                        if (locationName.Contains("FarmHouse"))
                            Game1.locations.Add(new FarmHouse(locationMap, locationName));
                        else if (locationName.Contains("Farm"))
                            Game1.locations.Add(new Farm(locationMap, locationName));
                        else if (locationName.Contains("FarmCave"))
                            Game1.locations.Add(new FarmCave(locationMap, locationName));
                        else
                            Game1.locations.Add(new GameLocation(locationMap, locationName));
                    }
                    catch
                    {

                    }
                }
            }
        }

        [HarmonyPatch]
        internal class GetRectangleFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(typeof(GameLocation), nameof(GameLocation.getSourceRectForObject), new[] { typeof(int) });
            }

            internal static bool Prefix(GameLocation __instance, int tileIndex, Rectangle __result, ref bool __state)
            {
                Rectangle tileRectangle;
                __state = true;

                if (rectangleCache.TryGetValue(tileIndex,out tileRectangle))
                {
                    __result = tileRectangle;
                    __state = false;
                }
                
                return __state;
            }

            internal static void Postfix(GameLocation __instance, int tileIndex, Rectangle __result, ref bool __state)
            {
                if (!__state)
                {
                    rectangleCache.Remove(tileIndex);
                    rectangleCache.Add(tileIndex, __result);
                }
            }
        }

    }
}
