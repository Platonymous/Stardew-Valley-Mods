using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Reflection;

namespace PyTK.Overrides
{

        internal class OvGame
        {
            internal static IModHelper Helper { get; } = PyTKMod._helper;
            internal static IMonitor Monitor { get; } = PyTKMod._monitor;
            internal static bool allready = false;
            internal static Dictionary<string, Dictionary<int, Rectangle>> rectangleCache = new Dictionary<string, Dictionary<int, Rectangle>>();


            [HarmonyPatch]
            internal class AllReaday
            {
                internal static MethodInfo TargetMethod()
                {
                    return AccessTools.Method(PyUtils.getTypeSDV("Menus.ReadyCheckDialog"), "update");
                }

                internal static void Postfix(ReadyCheckDialog __instance, GameTime time)
                {
                    if (allready)
                        __instance.confirm();
                    allready = false;
                }
            }

            [HarmonyPatch]
            internal class GameGetRectangleFix1
            {
                internal static MethodInfo TargetMethod()
                {
                    return AccessTools.Method(PyUtils.getTypeSDV("Game1"), "getSourceRectForStandardTileSheet");
                }

                internal static bool Prefix(ref Texture2D tileSheet, ref int tilePosition, ref int width, ref int height, ref Rectangle __result, ref bool __state)
                {
                    string id = tileSheet.Width + "." + tileSheet.Height + "." + width + "." + height;
                    __state = true;

                    if (rectangleCache.ContainsKey(id) && rectangleCache[id].ContainsKey(tilePosition))
                    {
                        __result = rectangleCache[id][tilePosition];
                        __state = false;
                    }

                    return __state;
                }

                internal static void Postfix(ref Texture2D tileSheet, ref int tilePosition, ref int width, ref int height, ref Rectangle __result, ref bool __state)
                {
                    if (!__state)
                    {
                        string id = tileSheet.Width + "." + tileSheet.Height + "." + width + "." + height;

                        if (!rectangleCache.ContainsKey(id))
                            rectangleCache.Add(id, new Dictionary<int, Rectangle>());

                        rectangleCache[id].AddOrReplace(tilePosition, __result);
                    }
                }
            }

            [HarmonyPatch]
            internal class GameGetRectangleFix2
            {
                internal static MethodInfo TargetMethod()
                {
                    return AccessTools.Method(PyUtils.getTypeSDV("Game1"), "getSquareSourceRectForNonStandardTileSheet");
                }

                internal static bool Prefix(ref Game1 __instance, ref Texture2D tileSheet, ref int tileWidth, ref int tileHeight, ref int tilePosition, ref Rectangle __result)
                {
                    __result = Game1.getSourceRectForStandardTileSheet(tileSheet, tilePosition, tileWidth, tileHeight);
                    return false;
                }
            }

            [HarmonyPatch]
            internal class GameGetRectangleFix3
            {
                internal static MethodInfo TargetMethod()
                {
                    return AccessTools.Method(PyUtils.getTypeSDV("Game1"), "getArbitrarySourceRect");
                }

                internal static bool Prefix(ref Game1 __instance, ref Texture2D tileSheet, ref int tileWidth, ref int tileHeight, ref int tilePosition, ref Rectangle __result)
                {
                    if (tileSheet == null)
                        return true;

                    __result = Game1.getSourceRectForStandardTileSheet(tileSheet, tilePosition, tileWidth, tileHeight);
                    return false;
                }
            }

        }
}
