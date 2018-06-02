using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmHub
{
    public class FHConfig
    {
        public string FarmHub { get; set; } = "https://farmhub-51957.firebaseio.com/";
        public string UniqueId { get; set; } = Guid.NewGuid().ToString();
        public string Passwort { get; set; } = "open";
        public string[] RequiredMods { get; set; } = new []{"Platonymous.FarmHub", "Platonymous.Toolkit"};

        public FHConfig()
        {

        }
    }
}
