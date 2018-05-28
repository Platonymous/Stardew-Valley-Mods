using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.IO;

namespace PyTK.ContentSync
{
    public class SerializationTexture2D
    {
        public string Data { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public SerializationTexture2D()
        {

        }

        public SerializationTexture2D(Texture2D texture)
        {
            Width = texture.Width;
            Height = texture.Height;
            serialize(texture);
        }

        public Texture2D getTexture()
        {
            byte[] buffer = PyNet.DecompressBytes(Data);
            MemoryStream stream = new MemoryStream(buffer);
            BinaryReader reader = new BinaryReader(stream);
            Color[] colors = new Color[Width * Height];

            for (int i = 0; i < colors.Length; i++)
            {
                var r = reader.ReadByte();
                var g = reader.ReadByte();
                var b = reader.ReadByte();
                var a = reader.ReadByte();
                colors[i] = new Color(r, g, b, a);
            }

            Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, Width, Height);
            texture.SetData(colors);
            return texture;
        }

        public void serialize(Texture2D texture)
        {
            Color[] data = new Color[Width * Height];
            byte[] buffer = new byte[data.Length * 4];
            texture.GetData(data);

            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            for (int i = 0; i < data.Length; i++)
            {
                writer.Write(data[i].R);
                writer.Write(data[i].G);
                writer.Write(data[i].B);
                writer.Write(data[i].A);
            }

            Data = PyNet.CompressBytes(stream.ToArray());
        }
    }
}
