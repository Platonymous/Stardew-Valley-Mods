using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CropExtensions
{
    public class CropDetails
    {
        public string[] Seasons { get; set; } = new string[] { "default", "default" };
        public int[] Days { get; set; } = new int[] { 0, 0 };
    }
}
