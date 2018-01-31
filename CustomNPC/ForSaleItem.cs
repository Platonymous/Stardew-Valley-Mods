using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNPC
{
    public class ForSaleItem
    {
        public int index { get; set; } = -1;
        public string name { get; set; } = "none";
        public string condition { get; set; } = "none";
        public int price { get; set; } = 100;
        public string type { get; set; } = "Object";

    }
}
