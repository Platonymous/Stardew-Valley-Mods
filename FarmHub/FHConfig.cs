using System;

namespace FarmHub
{
    public class FHConfig
    {
        public string FarmHub { get; set; } = "https://farmhub-51957.firebaseio.com/";
        public string UniqueId { get; set; } = Guid.NewGuid().ToString();
        public string Password { get; set; } = "open";
        public string[] RequiredMods { get; set; } = new []{"Platonymous.FarmHub", "Platonymous.Toolkit"};
        public bool UseIP { get; set; } = false;

        public FHConfig()
        {

        }
    }
}
