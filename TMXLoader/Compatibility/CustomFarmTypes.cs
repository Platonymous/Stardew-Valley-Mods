using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using xTile;

namespace TMXLoader.Compatibility
{
    class CustomFarmTypes
    {
        internal static void fixGreenhouseWarp()
        {
            if (Game1.getFarm() is Farm f && f.map is Map m && m.Properties.ContainsKey("Greenhouse"))
            {
                string[] position = m.Properties["Greenhouse"].ToString().Split(',');
                Point greenHousePosition = new Point(int.Parse(position[0]), int.Parse(position[1]));

                if (Game1.getLocationFromName("Greenhouse") is GameLocation greenhouse)
                {
                    Warp warp = new List<Warp>(greenhouse.warps).Find(w => w.TargetName == "Farm" && w.TargetX == 28 && w.TargetY == 16);
                    if (warp is Warp)
                    {
                        warp.TargetX = greenHousePosition.X;
                        warp.TargetY = greenHousePosition.Y + 1;
                    }
                }

            }
        }
    }

    

}