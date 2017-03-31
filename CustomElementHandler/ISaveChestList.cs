using StardewValley.Objects;
using System.Collections.Generic;

namespace CustomElementHandler
{
    public interface ISaveChestList
    {
        List<Chest> storage { get; set; }
    }
}
