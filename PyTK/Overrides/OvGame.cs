using HarmonyLib;
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

            internal static bool Prefix(Texture2D tileSheet, int tilePosition, int width, int height, ref Rectangle __result, ref bool __state)
            {
                if (tileSheet == null)
                {
                    __state = true;
                    return __state;
                }

                string id = tileSheet.Width + "." + tileSheet.Height + "." + width + "." + height;
                __state = true;

                if (rectangleCache.ContainsKey(id) && rectangleCache[id].ContainsKey(tilePosition))
                {
                    __result = rectangleCache[id][tilePosition];
                    __state = false;
                }

                return __state;
            }

                internal static void Postfix(Texture2D tileSheet, int tilePosition, int width, int height, ref Rectangle __result, ref bool __state)
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
        
        }
}
