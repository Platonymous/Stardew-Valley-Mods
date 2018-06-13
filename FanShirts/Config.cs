using Microsoft.Xna.Framework.Input;

namespace FanShirts
{
    class Config
    {
        public string JerseyID { get; set; } = "Germany";
        public Keys SwitchKey { get; set; } = Keys.J;
        public bool SyncInMultiplayer { get; set; } = false;

        public Config()
        {
        }
    }
}
