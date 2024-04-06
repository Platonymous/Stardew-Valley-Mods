using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artista.Artpieces
{

    public class Painting : Artpiece
    {
        public Texture2D Border { get; set; }

        public Painting(int width, int height, int scale)
            :base(width,height,1,scale, ArtType.Painting, Material.Canvas, Color.AntiqueWhite, false)
        {
            if(scale == 1)
                SetName($"Canvas {width} x {height}");
            else
                SetName($"Canvas {width} x {height} (X{scale})");
        }

        public void SetBorder(Texture2D texture)
        {
            Border = texture;
        }

        protected override Texture2D FinishTextureForMenu()
        {
            if (Border == null)
               SetDefaultBorder();

            return base.FinishTextureForMenu();
        }

        protected void SetDefaultBorder()
        {
            Border = ArtistaMod.Singleton.Helper.ModContent.Load<Texture2D>(Path.Combine("Assets", $"{TileWidth}x{TileHeight}.png"));
        }

        public Painting(SavedArtpiece art)
            : base(art) { }

    }
}
