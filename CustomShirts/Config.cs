using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace CustomShirts
{
    class Config
    {
        public string ShirtId { get; set; } = "none";
        public Keys SwitchKey { get; set; } = Keys.J;
        public List<SavedShirt> SavedShirts { get; set; } = new List<SavedShirt>();

        public Config()
        {
        }
    }
}
