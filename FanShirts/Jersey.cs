using Microsoft.Xna.Framework.Graphics;
using PyTK.Types;

namespace FanShirts
{
    public class Jersey
    {
        public string id { get; set; }
        public string fullid { get; set; }
        public string texture { get; set; }
        public float scale { get; set; }
        public int baseid { get; set; }
        internal Texture2D texture2d;

        public Jersey()
        {

        }
    }
}
