using StardewValley;
using System;

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
    }
}
