using Microsoft.Xna.Framework.Input;

namespace TotSQ
{
    class Config
    {
        public Keys debugKey { get; set; }

        public Config()
        {
            debugKey = Keys.J;
        }
    }
}
