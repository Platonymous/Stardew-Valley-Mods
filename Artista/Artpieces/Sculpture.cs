using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Artista.Artpieces
{
    public class Sculpture : Artpiece
    {
        public Sculpture(int width, int height, Material material)
            : base(width, height, width, 1, ArtType.Sculpture, material, Color.Transparent, true)
        { 
        }

        private Vector3 GetPoint(int x, int y)
        {
            if (Rotation == 0)
            {
                int x1 = x;
                int y1 = y;

                for(int z1 = 0; z1 < Width; z1++)
                    if (Canvas[GetIndex(x1, y1, z1)] != Color.Transparent)
                            return new Vector3(x1, y1, z1);
            }
            else if (Rotation == 1)
            {
                int y1 = y;
                int z1 = x;

                for (int x1 = Width; x1 >= 0; x1--)
                    if (Canvas[GetIndex(x1, y1, z1)] != Color.Transparent)
                        return new Vector3(x1, y1, z1);
            }
            else if (Rotation == 2)
            {
                int x1 = Width - x;
                int y1 = y;

                for (int z1 = Width; z1 >= 0; z1--)
                    if (Canvas[GetIndex(x1, y1, z1)] != Color.Transparent)
                        return new Vector3(x1, y1, z1);
            }
            else if (Rotation == 3)
            {
                int y1 = y;
                int z1 = Width - x;

                for (int x1 = 0; x1 < Width; x1++)
                    if (Canvas[GetIndex(x1,y1,z1)] != Color.Transparent)
                        return new Vector3(x1, y1, z1);
            }

            return new Vector3(-1,-1,-1);
        }

        private int GetIndex(int x, int y, int z)
        {
            return (int)(Width * Height * z + y * Width + x);
        }

        private int GetIndex(Vector3 point)
        {
            return GetIndex((int)point.X, (int)point.Y, (int)point.Z);
        }

        public override void Paint(int x, int y, Color color, bool update = true)
        {
            var point = GetPoint(x, y);

            if (point.X == -1)
                return;

            Canvas[GetIndex(point)] = color;

            if (update)
                Update();
        }

        protected override Texture2D DrawTexture()
        {
            return base.DrawTexture();
        }
    }
}
