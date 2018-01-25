using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using PyTK;
using PyTK.Extensions;
using PyTK.Types;

namespace NoSoilDecayRedux
{
    public class NoSoilDecayReduxMod : Mod
    {
        private Dictionary<GameLocation, List<Vector2>> hoeDirtChache;

        public override void Entry(IModHelper helper)
        {
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
        }

        private void SaveEvents_AfterLoad(object sender, System.EventArgs e)
        {
            hoeDirtChache = new Dictionary<GameLocation, List<Vector2>>();
            foreach (var location in PyUtils.getAllLocationsAndBuidlings())
            {
                hoeDirtChache[location] = location.terrainFeatures
                    .Where(t => t.Value is HoeDirt)
                    .Select(t => t.Key).ToList();
            }

            new TerrainSelector<HoeDirt>().whenAddedToLocation((gl, list) => list.ForEach(v => hoeDirtChache[gl].AddOrReplace(v)));
            new TileLocationSelector((l, v) => hoeDirtChache[l].Contains(v)).whenRemovedFromLocation(restoreTiles);

            /* Legacy Fix */
            "Town".toLocation().objects.Remove(new Vector2(2, 0));
        }

        private void restoreTiles(GameLocation l, List<Vector2> list)
        {
            if (l != Game1.currentLocation)
            {
                foreach (Vector2 v in list)
                {
                    l.terrainFeatures[v] = Game1.isRaining ? new HoeDirt(1) : new HoeDirt(0);
                    if (l.objects.ContainsKey(v) && l.objects[v] is SObject o && 
                        (o.name.Equals("Weeds") || o.name.Equals("Stone") || o.name.Equals("Twig")))
                        l.objects.Remove(v);
                }
            }
            else
            {
                foreach (Vector2 v in list)
                    hoeDirtChache[l].Remove(v);
            }
        }

    }
    
}
