using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RingOfFire
{
    class Flame
    {
        public Texture2D texture;
        public Vector2 position;
        public float scale;
        public float alpha;
        public float rotation;
        public float angle;
        public int direction;

        public Flame(Texture2D texture, Vector2 position, float scale, float rotation, float alpha, float angle, int direction)
        {
            this.texture = texture;
            this.position = position;
            this.scale = scale;
            this.rotation = rotation;
            this.alpha = alpha;
            this.angle = angle;
            this.direction = direction;
        }

    }
}
