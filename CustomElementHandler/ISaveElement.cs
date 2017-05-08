using System.Collections.Generic;

namespace CustomElementHandler
{
    public interface ISaveElement
    {

        dynamic getReplacement();

        Dictionary<string, string> getAdditionalSaveData();

        void rebuild(Dictionary<string, string> additionalSaveData, object replacement);
        
    }
}
