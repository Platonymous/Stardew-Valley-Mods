using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace PyTK.Types
{
    public interface ICustomContentAPI
    {
        Item getCustomObject(string id);
        void addContentPack(string folderName, string fileName, IModHelper helper = null, Dictionary<string,string> options = null);
    }
}
