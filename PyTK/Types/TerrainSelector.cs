using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using PyTK.Extensions;
using Microsoft.Xna.Framework;

namespace PyTK.Types
{
    public class TerrainSelector<T> where T : TerrainFeature
    {
        public Func<TerrainFeature, bool> predicate = o => o is T;

        public TerrainSelector(Func<T, bool> predicate = null)
        {
            if (predicate != null)
                this.predicate = o => (o is T) ? predicate.Invoke((T) o) : false;
        }

        public List<Vector2> keysIn(GameLocation location = null)
        {
            if (location == null)
                location = Game1.currentLocation;
            List<Vector2> list = location.terrainFeatures.toList(t => predicate(t.Value) ? t.Key : new Vector2(-1,-1));
            list.RemoveAll(p => p.X < 0);
            return list;
        }

        public List<TerrainFeature> valuesIn(GameLocation location = null)
        {
            if (location == null)
                location = Game1.currentLocation;

            List<TerrainFeature> list = location.terrainFeatures.toList(t => predicate(t.Value) ? t.Value : null);
            return list;
        }
    }
}
