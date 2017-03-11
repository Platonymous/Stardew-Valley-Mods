
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CustomFarming
{
    public interface ICustomFarmingObject
    {
        void build(string modFolder, string filename);

        Texture2D Texture
        {
            get;
        }

        Rectangle SourceRectangle
        {
            get;
        }
    }
}
