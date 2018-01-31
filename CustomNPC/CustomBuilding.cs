using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNPC
{
    public class CustomBuilding
    {
        public string map { get; set; } = "none";
        public string location { get; set; } = "none";
        public int[] position { get; set; } = new int[] { 0, 0 };
        public bool clear { get; set; } = true;
        public bool inlcudeEmpty { get; set; } = true;
    }
}
