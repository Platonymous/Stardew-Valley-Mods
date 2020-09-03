using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomTV
{
    public class TVScreen
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public int[] SourceBounds { get; set; } = new int[4] {0, 0, 42, 28 };
        public int[] Offset { get; set; } = new int[2] { 0, 0 };
        public int Frames { get; set; } = 2;
        public int FrameDuration { get; set; } = 150;
    }
}
