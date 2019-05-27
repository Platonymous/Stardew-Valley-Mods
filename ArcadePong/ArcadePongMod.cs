using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using PyTK.Extensions;
using StardewValley;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using System.Reflection;
using System;
using System.Collections.Generic;
using PyTK.Types;

namespace ArcadePong
{
    public class ArcadePongMod : Mod
    {
        internal static IMonitor monitor;
        internal static Mod pong;
        internal static List<EventHandler<ButtonPressedEventArgs>> keyEvents = new List<EventHandler<ButtonPressedEventArgs>>();
        internal CustomObjectData pdata;
        internal static bool runPong = false;


        public override void Entry(IModHelper helper)
        {
            monitor = Monitor;
            HarmonyInstance.Create("Platonymous.ArcadePong").PatchAll(Assembly.GetExecutingAssembly());
            helper.Events.GameLoop.SaveLoaded += (o, e) => setup();
            pdata = new CustomObjectData("Pong", "Pong/0/-300/Crafting -9/Play 'Pong by Cat' at home!/true/true/0/Pong", Game1.bigCraftableSpriteSheet.getTile(159, 16, 32).setSaturation(0).setLight(130), Color.Yellow, bigCraftable: true, type: typeof(PongMachine));
           
        }

        private void setup()
        {
            new InventoryItem(pdata.getObject(), 5000, 1).addToNPCShop("Gus");
        }
    }

    [HarmonyPatch]
    internal class StopPong1
    {
        internal static MethodInfo TargetMethod()
        {
                return AccessTools.Method(Type.GetType("Pong.ModEntry, Pong"), "OnButtonPressed");
        }

        internal static bool Prefix(Mod __instance, ButtonReleasedEventArgs e)
        {
            ArcadePongMod.pong = __instance;
            return ArcadePongMod.runPong;
        }
    }

    [HarmonyPatch]
    internal class StopPong2
    {
        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(Type.GetType("Pong.ModEntry, Pong"), "OnRendered");
        }

        internal static bool Prefix(Mod __instance)
        {
            ArcadePongMod.pong = __instance;
            return false;
        }
    }

    [HarmonyPatch]
    internal class StopPong3
    {
        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(Type.GetType("Pong.ModEntry, Pong"), "OnCursorMoved");
        }

        internal static bool Prefix(Mod __instance)
        {
            ArcadePongMod.pong = __instance;
            return ArcadePongMod.runPong;
        }
    }

    [HarmonyPatch]
    internal class StopPong4
    {
        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(Type.GetType("Pong.ModEntry, Pong"), "SwitchToNewMenu");
        }

        internal static void Postfix()
        {
            if (Game1.quit)
            {
                Game1.quit = false;
                ArcadePongMod.runPong = false; ;
                PongMinigame.quit = true;
                Game1.options.zoomLevel = PongMachine.zoom;
            }
        }
    }

}
