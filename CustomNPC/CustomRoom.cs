using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomNPC
{
    public class CustomRoom
    {
        public string name { get; set; } = "none";
        public string map { get; set; } = "none";
        public bool isOutdoor { get; set; } = false;
        public string conditions = "none";
    }
}
