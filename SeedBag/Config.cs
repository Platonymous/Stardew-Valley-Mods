using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portraiture2
{
    public class Config
    {
        public string Shop { get; set; } = "Pierre";
        public int Price { get; set; } = 30000;

        public SButton AndroidKey { get; set; } = SButton.B;
    }
}
