using Microsoft.Xna.Framework.Input;

namespace Visualize
{
    public class Config
    {
        public string activeProfile { get; set; } = "Platonymous.Original";
        public Keys next { get; set; } = Keys.PageDown;
        public Keys previous { get; set; } = Keys.PageUp;
    }
}
