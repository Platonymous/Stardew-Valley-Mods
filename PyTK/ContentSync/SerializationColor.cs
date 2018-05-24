using Microsoft.Xna.Framework;
namespace PyTK.ContentSync
{
    public class SerializationColor
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public SerializationColor()
        {

        }

        public SerializationColor(Color color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }

        public Color getColor()
        {
            return new Color(R,G,B,A);
        }
    }
}
