using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.ContentSync;

namespace CustomShirts
{
    public class ShirtSync
    {
        public SerializationTexture2D Texture { get; set; }
        public int BaseShirt { get; set; }
        public long FarmerId { get; set; }
        public int[][] BaseColors { get; set; }

        public ShirtSync()
        {

        }

        public ShirtSync(Texture2D texture, int baseid, long id)
        {
            Texture = new SerializationTexture2D(texture);
            BaseShirt = baseid;
            FarmerId = id;
            BaseColors = baseid > 0 ? getColorsFromBaseShirt(baseid - 1) : null;
        }

        public static int[][] getColorsFromBaseShirt(int id)
        {
            Color[] data = new Color[CustomShirtsMod.vanillaShirts.Bounds.Width * CustomShirtsMod.vanillaShirts.Bounds.Height];
            CustomShirtsMod.vanillaShirts.GetData<Color>(data);
            int index = id * 8 / CustomShirtsMod.vanillaShirts.Bounds.Width * 32 * 128 + id * 8 % CustomShirtsMod.vanillaShirts.Bounds.Width + CustomShirtsMod.vanillaShirts.Width * 4;
            int[] color1 = new int[] { data[index].R, data[index].G, data[index].B };
            int[] color2 = new int[] { data[index - CustomShirtsMod.vanillaShirts.Width].R, data[index - CustomShirtsMod.vanillaShirts.Width].G, data[index - CustomShirtsMod.vanillaShirts.Width].B };
            int[] color3 = new int[] { data[index - CustomShirtsMod.vanillaShirts.Width * 2].R, data[index - CustomShirtsMod.vanillaShirts.Width * 2].G, data[index - CustomShirtsMod.vanillaShirts.Width * 2].B };
            return new int[][] { color1, color2, color3 };
        }

    }
}
