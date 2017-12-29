using Microsoft.Xna.Framework;
using StardewValley;
using System;

namespace PyTK.Types
{
    public class TileLocationSelector
    {
        public Func<GameLocation, Vector2, bool> predicate = (l, v) => false;
        public GameLocation location;

        public TileLocationSelector(Func<GameLocation, Vector2,bool> predicate = null, GameLocation location = null)
        {
            this.location = location;

            if (predicate != null && location != null)
                this.predicate = (l, v) => l == location ? predicate.Invoke(l, v) : false;
            else if (location != null)
                this.predicate = (l, v) => l == location;
            else if(predicate != null)
                this.predicate = predicate;
        }
    }
}
