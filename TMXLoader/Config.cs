using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMXLoader
{
    public class Config
    {
        public Microsoft.Xna.Framework.Input.Keys openBuildMenu = Microsoft.Xna.Framework.Input.Keys.F2;

        public bool clearBuildingSpace = true;
        public bool converter { get; set; } = false;
    }
}
