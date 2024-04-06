using System.Collections.Generic;

namespace InstantLoad
{
    public class Config { 

        public bool EnableInstantLoad { get; set; } = true;

        public bool LoadHost { get; set; } = false;

        public bool EnableDebugCommands { get; set; } = true;

        public List<DebugTrigger> DebugCommands { get; set; } = new List<DebugTrigger>();

    }
}
