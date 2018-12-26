using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMXLoader
{
    public class TileShopItem
    {
        public int index { get; set; } = -1;
        public string type { get; set; } = "Object";
        public string name { get; set; } = "none";
        public int stock { get; set; } = int.MaxValue;
        public int price { get; set; } = -1;
    }
}
