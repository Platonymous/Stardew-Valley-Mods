using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;

namespace Snake
{
    class SnakeMachine
    {
        public SnakeMachine()
        {
        }

        public static void start(IModHelper helper)
        {
            Game1.currentMinigame = new SnakeMinigame(helper);
        }

        public static StardewValley.Object GetNew(StardewValley.Object alt)
        {
            if (Game1.bigCraftablesInformation.Values.Any(v => v.Contains("Snake Arcade Machine"))){
                var obj = new StardewValley.Object(Vector2.Zero, Game1.bigCraftablesInformation.FirstOrDefault(b => b.Value.Contains("Snake Arcade Machine")).Key, false);
                return obj;
            }

            return alt;
        }
    }
}
