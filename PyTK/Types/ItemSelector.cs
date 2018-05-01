using StardewValley;
using SObject = StardewValley.Object;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using PyTK.Extensions;

namespace PyTK.Types
{
    public class ItemSelector<T> where T : Item
    {
        public Func<Item, bool> predicate = o => o is T;

        public ItemSelector(Func<T, bool> predicate = null)
        {
            if (predicate != null)
                this.predicate = o => (o is T) ? predicate.Invoke((T) o) : false;
        }

        /*
        public List<Vector2> keysIn(GameLocation location = null)
        {
            if (location == null)
                location = Game1.currentLocation;

            List<Vector2> list = Location.objects).toList(t => predicate(t.Value) ? t.Key : new Vector2(-1,-1));
            list.RemoveAll(p => p.X < 0);
            return list;
        }

        public List<SObject> valuesIn(GameLocation location = null)
        {
            if (location == null)
                location = Game1.currentLocation;

            List<SObject> list = location.objects.toList(t => predicate(t.Value) ? t.Value : null);
            return list;
        }
    */
    }

}
