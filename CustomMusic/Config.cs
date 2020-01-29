using System.Collections.Generic;

namespace CustomMusic
{
    public class Config
    {
        public bool Convert { get; set; } = false;
        public bool Debug { get; set; } = false;
        public Dictionary<string, string> Presets { get; set; } = new Dictionary<string, string>();
    }
}
