using System.Collections.Generic;

namespace PyTK.Types
{
    public class InterceptorData
    {
        public List<string> Mods { get; set; } = new List<string>() { PyTKMod._instance.ModManifest.UniqueID };
    }
}
