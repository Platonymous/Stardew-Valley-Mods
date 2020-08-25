using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using xTile;
using xTile.Display;

namespace PyTK.APIs
{
    public interface IMapAPI
    {
        void EnableMoreMapLayers(Map map);
        IDisplayDevice GetPyDisplayDevice(ContentManager contentManager, GraphicsDevice graphicsDevice);
        IDisplayDevice GetPyDisplayDevice(ContentManager contentManager, GraphicsDevice graphicsDevice, bool compatibility);
    }
}
