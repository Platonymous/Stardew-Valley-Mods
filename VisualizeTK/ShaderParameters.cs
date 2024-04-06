using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualizeTK
{
    public class ShaderParameters
    {
        public float SatR { get; set; } = -1.0f;
        public float SatG { get; set; } = -1.0f;
        public float SatB { get; set; } = -1.0f;

        public string Preset { get; set; } = "Custom";

        public Color[] Colors { get; set; } = new Color[0];

        public float SwitchSpeed { get; set; } = -1f;
    }
}
