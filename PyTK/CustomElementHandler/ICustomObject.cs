using System.Collections.Generic;

namespace PyTK.CustomElementHandler
{
    public interface ICustomObject : ISaveElement
    {
        ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement);
    }
}
