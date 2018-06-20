using Microsoft.Xna.Framework.Graphics;

namespace CustomShirts
{
    public class Shirt
    {
        public string id { get; set; } = "none";
        public string fullid { get; set; } = "none";
        public string texture { get; set; } = null;
        public int tileindex { get; set; } = 0;
        public float scale { get; set; } = 1;
        public int baseid { get; set; } = -9999;
        internal Texture2D texture2d = null;

        public Shirt()
        {

        }
    }
}
