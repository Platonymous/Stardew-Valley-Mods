using System.Collections.Generic;
using StardewModdingAPI;

namespace CustomShirts
{
    class Config
    {
        public string ShirtId { get; set; } = "none";
        public SButton SwitchKey { get; set; } = SButton.J;
        public List<SavedShirt> SavedShirts { get; set; } = new List<SavedShirt>();

        public Config()
        {
        }
    }
}
