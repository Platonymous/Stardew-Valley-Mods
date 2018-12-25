using Harmony;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using PyTK.Extensions;

namespace PyTK.Overrides
{
    internal class OvLocations
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;
        internal static Dictionary<int, Rectangle> rectangleCache = new Dictionary<int, Rectangle>();

        [HarmonyPatch]
        internal class GLBugFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(PyUtils.getTypeSDV("GameLocation"), "Equals",new[] { PyUtils.getTypeSDV("GameLocation") });
            }

            internal static bool Prefix(GameLocation __instance, GameLocation other, ref bool __result)
            {
                __result = object.Equals((object)__instance.Name, (object)other.Name) && object.Equals((object)__instance.uniqueName.Value, (object)other.uniqueName.Value ) && object.Equals((object)__instance.isStructure.Value, (object)other.isStructure.Value);
                return false;
            }
        }

        [HarmonyPatch]
        internal class BLoadBugFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(PyUtils.getTypeSDV("Buildings.Building"), "load");
            }

            internal static bool Prefix(Building __instance)
            {
                return !(__instance is Mill m && (SaveHandler.getDataString(__instance).StartsWith(SaveHandler.newPrefix)) && m.indoors.Value is GameLocation gl && (SaveHandler.getDataString(gl).StartsWith(SaveHandler.newPrefix)));
            }
        }


        [HarmonyPatch]
        internal class TouchActionFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(PyUtils.getTypeSDV("GameLocation"), "performTouchAction");
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
                return AccessTools.Method(PyUtils.getTypeSDV("GameLocation"), "isActionableTile");
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
                return AccessTools.Method(PyUtils.getTypeSDV("Game1"), "tryToCheckAt");
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
                return AccessTools.Method(PyUtils.getTypeSDV("GameLocation"), "tryToAddCritters");
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
                return AccessTools.Method(PyUtils.getTypeSDV("Game1"), "getLocationRequest");
            }

            internal static void Prefix(string locationName, bool isStructure = false)
            {
                if (locationName == null || isStructure)
                    return;

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

                    string locationMap = Path.Combine("Maps", locationName);

                    if (locationName.Contains(":"))
                    {
                        locationMap = Path.Combine("Maps", locationName.Split(':')[0]);
                        locationName = locationMap + "_" + locationName.Split(':')[1];
                    }

                    if (locationName.Contains("FarmHouse"))
                        Game1.locations.Add(new FarmHouse(locationMap, locationName));
                    else if (locationName.Contains("Farm"))
                        Game1.locations.Add(new Farm(locationMap, locationName));
                    else
                        Game1.locations.Add(new GameLocation(locationMap, locationName));
                }
            }
        }

        [HarmonyPatch]
        internal class GetRectangleFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(PyUtils.getTypeSDV("GameLocation"), "getSourceRectForObject", new[] { typeof(int) });
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
                if(!__state)
                    rectangleCache.AddOrReplace(tileIndex, __result);
            }
        }

    }
}
