using Netcode;
using System.Collections.Generic;

namespace PyTK.CustomElementHandler
{
    public interface ISyncableElement : ISaveElement
    {
        Dictionary<string, string> getSyncData();
        void sync(Dictionary<string, string> syncData);
        PySync syncObject { get; set; }
    }
}
