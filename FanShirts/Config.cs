using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace FanShirts
{
    class Config
    {
        public string JerseyID { get; set; } = "Platonymous.WorldCup2018.Germany";
        public Keys SwitchKey { get; set; } = Keys.J;
        public List<SavedJersey> SavedJerseys { get; set; } = new List<SavedJersey>();

        public Config()
        {
        }
    }
}
