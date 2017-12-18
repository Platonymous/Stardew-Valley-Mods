using Microsoft.Xna.Framework;

namespace Visualize
{
    public class Profile
    {
        public string name { get; set; } = "Visualize Profile";
        public string author { get; set; } = "none";
        public string version { get; set; } = "1.0.0";
        public string id { get; set; } = "auto";
        public Color tint { get; set; } = Color.White;
        public float saturation { get; set; } = 100;
        public float red { get; set; } = 100;
        public float green { get; set; } = 100;
        public float blue { get; set; } = 100;
        public float light { get; set; } = 100;
        public string palette { get; set; } = "none";
        public bool noAmbientLight { get; set; } = false;
        public bool noTransparancy { get; set; } = false;
        public bool noLightsources { get; set; } = false;
        public bool noShadow { get; set; } = false;
        public bool noColorTransparancy { get; set; } = false;
    }
}
