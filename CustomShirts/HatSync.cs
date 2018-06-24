using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.ContentSync;

namespace CustomShirts
{
    public class HatSync
    {
        public SerializationTexture2D Texture { get; set; }
        public string SyncId { get; set; }

        public HatSync()
        {

        }

        public HatSync(Texture2D texture, long id, string hatId)
        {
            Texture = new SerializationTexture2D(texture);
            SyncId = hatId + "." + id;
        }
    }
}
