using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;


namespace CustomElementHandler
{
    public interface ISaveElement
    {

        dynamic getReplacement();

        Dictionary<string, string> getAdditionalSaveData();

        void rebuild(Dictionary<string, string> additionalSaveData, object replacement);
        
    }
}
