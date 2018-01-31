using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNPC
{
    public class AdditionalWarp
    {
        public string mapEntry { get; set; } = "none";
        public int[] entry { get; set; } = new int[] { 0, 0 };
        public string mapExit { get; set; } = "none";
        public int[] exit { get; set; } = new int[] { 0, 0 };
        public bool flip { get; set; } = false;
    }
}
