using Microsoft.Xna.Framework.Graphics;

namespace CustomShirts
{
    public class HatBlueprint
    {
        public string id { get; set; } = "none";
        public string fullid { get; set; } = "none";
        public string texture { get; set; } = null;
        public int tileindex { get; set; } = 0;
        public float scale { get; set; } = 1;
        public int price { get; set; } = 100;
        public string name { get; set; } = "Hat";
        public string description { get; set; } = "A Hat";
        public int baseid { get; set; } = 1;
        internal Texture2D texture2d = null;

        public HatBlueprint()
        {

        }
    }
}
