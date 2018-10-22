using Microsoft.Xna.Framework.Input;

namespace GhostTown
{
    class Config
    {
        public bool desaturate { get; set; }
        public bool people { get; set; }
        public bool houses { get; set; }
        public bool animals { get; set; }
        public bool critters { get; set; }

        public Config()
        {
            desaturate = true;
            people = true;
            houses = true;
            animals = true;
            critters = true;
        }
    }
}
