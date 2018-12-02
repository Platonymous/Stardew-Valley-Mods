using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using PyTK.Extensions;
using Microsoft.Xna.Framework;
using System.Linq;

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
            List<Vector2> list = (location.terrainFeatures.FieldDict).Keys.ToList();
            list.RemoveAll(k => !predicate(location.terrainFeatures.FieldDict[k]));
            return list;
        }

        public List<TerrainFeature> valuesIn(GameLocation location = null)
        {
            if (location == null)
                location = Game1.currentLocation;

            List<TerrainFeature> list = location.terrainFeatures.FieldDict.toList(t => predicate(t.Value.Value) ? t.Value.Value : null);
            return list;
        }
    }
}
