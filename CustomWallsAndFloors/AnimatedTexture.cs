using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Types;
using StardewValley;
using System.Collections.Generic;
using System.IO;

namespace CustomWallsAndFloors
{
    public class AnimatedTexture : Texture2D
    {
        public List<AnimatedTile> AnimatedTiles { get; set; } = new List<AnimatedTile>();

        public AnimatedTexture(GraphicsDevice graphicsDevice, int width, int height)
            : base(graphicsDevice, width, height)
        {

        }

        public AnimatedTexture(Texture2D tex, List<AnimatedTile> tiles)
            : base(tex.GraphicsDevice, tex.Width, tex.Height)
        {
            Color[] data = new Color[tex.Width * tex.Height];
            tex.GetData(data);
            SetData(data);

            AnimatedTiles = tiles;
        }

        public AnimatedTexture(GraphicsDevice graphicsDevice, int width, int height, List<AnimatedTile> tiles)
            : base(graphicsDevice, width, height)
        {
            AnimatedTiles = tiles;
        }

        public static AnimatedTexture FromTexture(Texture2D texture, List<AnimatedTile> tiles)
        {
            AnimatedTexture result = new AnimatedTexture(texture, tiles);
            return result;
        }
    }
}
