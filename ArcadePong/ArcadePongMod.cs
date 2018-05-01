using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework.Input;
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
        internal static IModHelper pongHelper;
        internal static IMonitor monitor;
        internal static Mod pong;
        internal static List<EventHandler<EventArgsKeyPressed>> keyEvents = new List<EventHandler<EventArgsKeyPressed>>();
        internal CustomObjectData pdata; 


        public override void Entry(IModHelper helper)
        {
            monitor = Monitor;
            HarmonyInstance.Create("Platonymous.ArcadePong").PatchAll(Assembly.GetExecutingAssembly());
            SaveEvents.AfterLoad += (o, e) => setup();
            SaveEvents.AfterReturnToTitle += (s, o) =>
            {
                foreach (EventHandler<EventArgsKeyPressed> a in keyEvents)
                    ControlEvents.KeyPressed -= a;
            };
            pdata = new CustomObjectData("Pong", "Pong/0/-300/Crafting -9/Play 'Pong by Cat' at home!/true/true/0/Pong", Game1.bigCraftableSpriteSheet.getTile(159, 16, 32).setSaturation(0).setLight(130), Color.Yellow, bigCraftable: true, type: typeof(PongMachine));
           
        }

        private void setup()
        {
            new InventoryItem(pdata.getObject(), 0, 1).addToFurnitureCatalogue();
            PongMinigame.game = Helper.Reflection.GetField<object>(pong, "game").GetValue();
            pongHelper = (IModHelper)typeof(Mod).GetProperty("Helper", BindingFlags.Public | BindingFlags.Instance).GetValue(pong);
            keyEvents.AddOrReplace(Keys.Space.onPressed(() =>
            {
                if (Game1.currentMinigame is PongMinigame p)
                    Helper.Reflection.GetMethod(PongMinigame.game, "Start").Invoke();
            }));

            keyEvents.AddOrReplace(Keys.Escape.onPressed(() =>
            {
                if (Game1.currentMinigame is PongMinigame p)
                    if (Helper.Reflection.GetMethod(PongMinigame.game, "HasStarted").Invoke<bool>())
                        PongMinigame.game.GetType().GetMethod("Reset", new Type[] { }).Invoke(PongMinigame.game, null);
                    else
                    {
                        PongMinigame.quit = true;
                        Game1.options.zoomLevel = PongMachine.zoom;
                    }
            }));
            keyEvents.AddOrReplace(Keys.P.onPressed(() =>
            {
                if (Game1.currentMinigame is PongMinigame p)
                    Helper.Reflection.GetMethod(PongMinigame.game, "TogglePaused").Invoke();
            }));
        }
    }

    [HarmonyPatch]
    internal class StopPong1
    {
        internal static MethodInfo TargetMethod()
        {
                return AccessTools.Method(Type.GetType("Pong.ModEntry, Pong"), "ButtonPressed");
        }

        internal static void Prefix(Mod __instance, ref EventArgsInput e)
        {
            ArcadePongMod.pong = __instance;
    
            e = new EventArgsInput(SButton.A, e.Cursor, false, false, new HashSet<SButton>());
        }
    }

    [HarmonyPatch]
    internal class StopPong2
    {
        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(Type.GetType("Pong.ModEntry, Pong"), "OnPostRender");
        }

        internal static bool Prefix(Mod __instance)
        {
            ArcadePongMod.pong = __instance;
            return false;
        }
    }

}
