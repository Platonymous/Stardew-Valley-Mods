using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using PlatoTK;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace Arcade2048
{
    class Machine2048
    {
        public Machine2048()
        {

        }
        
        public static void start(IModHelper helper)
        {
            Game1.currentMinigame = new Game2048(helper.GetPlatoHelper());
        }
        public static StardewValley.Object GetNew(StardewValley.Object alt)
        {
            if (Game1.bigCraftablesInformation.Values.Any(v => v.Contains("2048 Arcade Machine")))
            {
                var obj = new StardewValley.Object(Vector2.Zero, Game1.bigCraftablesInformation.FirstOrDefault(b => b.Value.Contains("2048 Arcade Machine")).Key, false);
                return obj;
            }

            return alt;
        }

    }
}
