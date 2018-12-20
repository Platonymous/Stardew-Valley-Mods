using System.Collections.Generic;

namespace PyTK.CustomElementHandler
{
    public class CODSyncMessage
    {
        public List<CODSync> Syncs = new List<CODSync>();

        public CODSyncMessage()
        {

        }

        public CODSyncMessage(List<CODSync> syncs)
        {
            Syncs = syncs;
        }
    }
}
