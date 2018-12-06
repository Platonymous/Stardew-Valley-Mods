using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Types;
using PyTK.Extensions;
using StardewModdingAPI;
using System;
using System.Collections.Generic;

namespace ScaleUp
{
    class Scaler : IAssetEditor
    {
        public static Dictionary<string, int> Assets = new Dictionary<string, int>();

        public IModHelper helper;

        public Scaler(IModHelper helper)
        {
            this.helper = helper;
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return Assets.Exists(a => asset.AssetNameEquals(a.Key));
        }

        public void Edit<T>(IAssetData asset)
        {
            Texture2D image = asset.AsImage().Data;
            int width = image.Width;
            int oWith = Assets.Find(a => asset.AssetNameEquals(a.Key)).Value;

            if (oWith < width)
            {
                float scale = (float)(Convert.ToDouble(width) / Convert.ToDouble(oWith));
                int height =(int)(image.Height / scale);
                var scaled = ScaledTexture2D.FromTexture(image.getArea(new Rectangle(0, 0, oWith, height)), image, scale);
                asset.AsImage().ReplaceWith(scaled);
            }
        }

    }
}
