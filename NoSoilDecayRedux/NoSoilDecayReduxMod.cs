using StardewValley;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using PyTK;
using PyTK.Extensions;
using PyTK.Types;
using System;

namespace NoSoilDecayRedux
{
    public class NoSoilDecayReduxMod : Mod
    {
        private Dictionary<GameLocation, List<Vector2>> hoeDirtChache = new Dictionary<GameLocation, List<Vector2>>();

        public override void Entry(IModHelper helper)
        {
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
        }

        private void SaveEvents_AfterLoad(object sender, System.EventArgs e)
        {
            hoeDirtChache = PyUtils.getAllLocationsAndBuidlings().toDictionary(l => new DictionaryEntry<GameLocation, List<Vector2>>(l, l.terrainFeatures.toList(t => t.Value is HoeDirt ? t.Key : Vector2.Zero)));
            hoeDirtChache.useAll(k => k.Value.RemoveAll(v => v == Vector2.Zero));

            new TerrainSelector<HoeDirt>().whenAddedToLocation((gl, list) => list.useAll(v => hoeDirtChache[gl].AddOrReplace(v)));
            new TileLocationSelector((l, v) => hoeDirtChache[l].Contains(v)).whenRemovedFromLocation(restoreTiles);

            /* Legacy Fix */
            "Town".toLocation().objects.Remove(new Vector2(2, 0));
        }

        private void restoreTiles(GameLocation l, List<Vector2> list)
        {
            if (l != Game1.currentLocation)
                foreach (Vector2 v in list)
                {
                    l.terrainFeatures.AddOrReplace(v, Game1.isRaining ? new HoeDirt(1) : new HoeDirt(0));
                    if (l.objects.ContainsKey(v) && l.objects[v] is SObject o && (o.name.Equals("Weeds") || o.name.Equals("Stone") || o.name.Equals("Twig")))
                        l.objects.Remove(v);
                }
            else foreach (Vector2 v in list)
                    hoeDirtChache[l].Remove(v);
        }

    }
    
}
