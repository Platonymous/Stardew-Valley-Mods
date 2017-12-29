using StardewValley.TerrainFeatures;
using System;

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
    }
}
