using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomWallsAndFloors
{
    public class AnimatedTile
    {
        public int Index { get; set; } = 0;
        public int Frames { get; set; } = 3;
        public int Length { get; set; } = 1000;
        public bool Floor { get; set; } = false;
     }
}
