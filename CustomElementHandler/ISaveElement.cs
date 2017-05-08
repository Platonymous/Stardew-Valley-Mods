using StardewValley;
using System.Collections.Generic;

namespace CustomElementHandler
{
    public interface ISaveElement
    {

        object getReplacement();

        Dictionary<string, string> getAdditionalSaveData();

        void rebuild(Dictionary<string, string> additionalSaveData, object replacement);
        
    }
}
