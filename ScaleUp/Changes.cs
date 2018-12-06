using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScaleUp
{
    public class Changes
    {
        public string Action { get; set; } = "";
        public string Target { get; set; } = "";
        public bool ScaleUp { get; set; } = false;
        public int OriginalWidth { get; set; } = -1;
    }
}
