using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomFarmingRedux
{
    public class LegacyProduce
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool usePrefix { get; set; } = false;
        public bool useSuffic { get; set; } = false;
        public bool useColor { get; set; } = false;
        public int ProduceID { get; set; } = 0;
        public string Tilesheet { get; set; }
        public int TileIndex { get; set; } = 0;
        public int Stack { get; set; } = 1;
        public int ProductionTime { get; set; } = 120;
        public int Quality { get; set; } = 0;
        public int MaterialQuality { get; set; } = -1;
    }
}
