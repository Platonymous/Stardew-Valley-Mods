using System.Collections.Generic;

namespace InstantLoad
{
    public class DebugTrigger
    {
        public string Target { get; set; } = "Console";

        public string Event { get; set; } = "Load";

        public string Command { get; set; }

        public List<string> Args { get; set; } = new List<string>();
    }
}
