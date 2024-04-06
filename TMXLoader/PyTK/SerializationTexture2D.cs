using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.IO;

namespace TMXLoader
{
    public class SerializationTexture2D
    {
        public string Data { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public bool IsScaled { get; set; }
        public float Scale { get; set; }
        public string ScaledData { get; set; }
        public int ScaledWidth { get; set; }
        public int ScaledHeight { get; set; }
        public int[] ForcedSourceRectangle { get; set; }

        public SerializationTexture2D()
        {

        }

        public SerializationTexture2D(Texture2D texture)
        {
            Width = texture.Width;
            Height = texture.Height;

            IsScaled = false;

            if (texture is ScaledTexture2D st)
            {
                IsScaled = true;
                Scale = st.Scale;
                ScaledWidth = st.STexture.Width;
                ScaledHeight = st.STexture.Height;
                if (st.ForcedSourceRectangle.HasValue)
                    ForcedSourceRectangle = new int[4] { st.ForcedSourceRectangle.Value.X, st.ForcedSourceRectangle.Value.Y, st.ForcedSourceRectangle.Value.Width, st.ForcedSourceRectangle.Value.Height };
                else
                    ForcedSourceRectangle = new int[4] { -1, -1, -1, -1 };
            }

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

            Texture2D texture = null;

            if (IsScaled)
            {
                byte[] sbuffer = PyNet.DecompressBytes(ScaledData);
                MemoryStream sstream = new MemoryStream(sbuffer);
                BinaryReader sreader = new BinaryReader(sstream);
                Color[] scolors = new Color[ScaledWidth * ScaledHeight];

                for (int i = 0; i < scolors.Length; i++)
                {
                    var sr = sreader.ReadByte();
                    var sg = sreader.ReadByte();
                    var sb = sreader.ReadByte();
                    var sa = sreader.ReadByte();
                    scolors[i] = new Color(sr, sg, sb, sa);
                }

                Texture2D stexture = new Texture2D(Game1.graphics.GraphicsDevice, ScaledWidth, ScaledHeight);
                stexture.SetData(scolors);

                texture = new ScaledTexture2D(Game1.graphics.GraphicsDevice, Width, Height, stexture, Scale, (ForcedSourceRectangle.Length > 0 && ForcedSourceRectangle[0] != -1) ? new Rectangle?(new Rectangle(ForcedSourceRectangle[0], ForcedSourceRectangle[1], ForcedSourceRectangle[2], ForcedSourceRectangle[3])) : null);
            }else
                texture = new Texture2D(Game1.graphics.GraphicsDevice, Width, Height);

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

            if (texture is ScaledTexture2D stexture)
            {
                Color[] sdata = new Color[ScaledWidth * ScaledHeight];
                byte[] sbuffer = new byte[sdata.Length * 4];
                stexture.STexture.GetData(sdata);

                MemoryStream sstream = new MemoryStream();
                BinaryWriter swriter = new BinaryWriter(sstream);

                for (int i = 0; i < sdata.Length; i++)
                {
                    swriter.Write(sdata[i].R);
                    swriter.Write(sdata[i].G);
                    swriter.Write(sdata[i].B);
                    swriter.Write(sdata[i].A);
                }

                ScaledData = PyNet.CompressBytes(sstream.ToArray());
            }
        }
    }
}
