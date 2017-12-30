using System.Collections.Generic;

namespace PyTK.CustomElementHandler
{
    public interface ISaveElement
    {

        object getReplacement();

        Dictionary<string, string> getAdditionalSaveData();

        void rebuild(Dictionary<string, string> additionalSaveData, object replacement);

    }
}
