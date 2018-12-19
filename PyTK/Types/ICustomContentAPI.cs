using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace PyTK.Types
{
    public interface ICustomContentAPI
    {
        Item getCustomObject(string id);
    }
}
