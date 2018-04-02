using Microsoft.Xna.Framework;

namespace Visualize
{
    public class Profile
    {
        public string name { get; set; } = "Visualize Profile";
        public string author { get; set; } = "none";
        public string version { get; set; } = "1.0.0";
        public string id { get; set; } = "auto";
        public float saturation { get; set; } = 100;
        public int[] tint { get; set; } = new int[] { 255, 255, 255, 255 };
        public string palette { get; set; } = "none";
    }
}
