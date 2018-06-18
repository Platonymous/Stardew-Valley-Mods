using Microsoft.Xna.Framework.Graphics;
using PyTK.ContentSync;

namespace CustomShirts
{
    internal class ShirtSync
    {
        public SerializationTexture2D Texture { get; set; }
        public int BaseShirt { get; set; }
        public long FarmerId { get; set; }

        public ShirtSync()
        {

        }

        public ShirtSync(Texture2D texture, int baseid, long id)
        {
            Texture = new SerializationTexture2D(texture);
            BaseShirt = baseid;
            FarmerId = id;
        }

    }
}
