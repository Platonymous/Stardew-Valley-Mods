using StardewValley;
using System;

namespace PyTK.Types
{
    public class ItemSelector<T> where T : Item
    {
        public Func<T, bool> predicate;

        public ItemSelector(Func<T, bool> predicate)
        {
            this.predicate = predicate;
        }
    }
}
