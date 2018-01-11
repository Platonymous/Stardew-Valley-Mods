using Harmony;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;

namespace PyTK.Overrides
{
    internal class OvLocations
    {
        
        internal static Dictionary<string, Func<List<string>, bool>> actions = new Dictionary<string, Func<List<string>, bool>>();
        
        [HarmonyPatch]
        internal class ActionableFix
        {
            internal static MethodInfo TargetMethod()
            {
                if (Type.GetType("StardewValley.GameLocation, Stardew Valley") != null)
                    return AccessTools.Method(Type.GetType("StardewValley.GameLocation, Stardew Valley"), "isActionableTile");
                else
                    return AccessTools.Method(Type.GetType("StardewValley.GameLocation, StardewValley"), "isActionableTile");
            }

            internal static void Postfix(GameLocation __instance, ref bool __result, int xTile, int yTile)
            {
                if (__instance.doesTileHaveProperty(xTile, yTile, "Action", "Buildings") != null)
                {
                    List<string> prop = __instance.doesTileHaveProperty(xTile, yTile, "Action", "Buildings").Split(' ').ToList();

                    if (actions.ContainsKey(prop[0]))
                        Game1.isInspectionAtCurrentCursorTile = true;
                }
            }
        }

        [HarmonyPatch]
        internal class ActionFix
        {
            internal static MethodInfo TargetMethod()
            {
                if (Type.GetType("StardewValley.Game1, Stardew Valley") != null)
                    return AccessTools.Method(Type.GetType("StardewValley.Game1, Stardew Valley"), "tryToCheckAt");
                else
                    return AccessTools.Method(Type.GetType("StardewValley.Game1, StardewValley"), "tryToCheckAt");
            }

           
            internal static void Postfix(Vector2 grabTile, ref bool __result)
            {
                GameLocation location = Game1.currentLocation;
                if (!Utility.tileWithinRadiusOfPlayer((int)grabTile.X, (int)grabTile.Y, 1, Game1.player))
                    return;
                string action = "";

                PropertyValue propertyValue = null;
                Tile tile = location.map.GetLayer("Buildings").PickTile(new Location((int) grabTile.X * Game1.tileSize, (int) grabTile.Y * Game1.tileSize), Game1.viewport.Size);
                if (tile != null)
                    tile.Properties.TryGetValue("Action", out propertyValue);
                if (propertyValue != null && (location.currentEvent != null || location.isCharacterAtTile(grabTile + new Vector2(0.0f, 1f)) == null))
                    action = propertyValue;

                if (action == "")
                    return;

                List <string> prop = action.Split(' ').ToList();
                if (actions.ContainsKey(prop[0]))
                    __result = actions[prop[0]].Invoke(prop);
            }
            
        } 
    }
}
