using Microsoft.Xna.Framework.Graphics;
using PyTK.ContentSync;

namespace CustomWallsAndFloors
{
    public class ContentRequest
    {
        public SerializationTexture2D Texture { get; set; } = null;
        public string Id { get; set; } = "na";
        public bool IsFloor { get; set; } = false;

        public ContentRequest(string id, bool isFloor, Texture2D texture = null)
        {
            if(texture != null)
                Texture = new SerializationTexture2D(texture);

            Id = id;
            IsFloor = isFloor;
        }
    }
}
