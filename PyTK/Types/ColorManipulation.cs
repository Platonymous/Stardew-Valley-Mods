using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyTK.Types
{
    public class ColorManipulation
    {
        public float saturation;
        public float light;
        public List<Color> palette;

        public ColorManipulation(List<Color> palette, float saturation = 100, float light = 100)
        {
            this.saturation = saturation;
            this.light = light;
            this.palette = palette;
        }

        public ColorManipulation(float saturation = 100, float light = 100)
        {
            this.saturation = saturation;
            this.light = light;
            palette = new List<Color>();
        }
    }
}
