using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;
using PyTK.Extensions;
using PyTK.Types;

namespace GhostTown
{
    class Ghostify : IAssetEditor
    {
        public IModHelper helper;
        private ColorManipulation spriteGhostifyer;
        private ColorManipulation portraitGhostifyer;
        private ColorManipulation mapsGhostifyer;

        public Ghostify(IModHelper helper)
        {
            this.helper = helper;
            float ts = 0.6f;
            float tp = 0.9f;
            List<Color> colors = new List<Color>() { Color.Black * 0, Color.Gray * tp, Color.LightCyan * tp, Color.LightBlue * tp, Color.LightGray * tp, Color.LightSkyBlue * tp, Color.LightSlateGray * tp, Color.MidnightBlue * tp, Color.DarkSlateGray * tp, Color.DimGray * tp, new Color(1,1,1), Color.DarkGray * tp, Color.AliceBlue * tp, Color.Aqua * tp, Color.DarkBlue * tp, Color.WhiteSmoke * tp, Color.Blue * tp, Color.CadetBlue * tp, Color.SlateBlue * tp, Color.DarkSlateBlue * tp };
            List<Color> colorsTransparent = new List<Color>() { Color.Black * 0, Color.Gray * ts, Color.LightCyan * ts, Color.LightBlue * ts, Color.LightGray * ts, Color.LightSkyBlue * ts, Color.LightSlateGray * ts, Color.MidnightBlue * ts, Color.DarkSlateGray * ts, Color.DimGray * ts, new Color(10, 10, 10) * ts, Color.DarkGray * ts, Color.AliceBlue * ts, Color.Aqua * ts, Color.DarkBlue * ts, Color.WhiteSmoke * ts, Color.Blue * ts, Color.CadetBlue * ts, Color.SlateBlue * ts, Color.DarkSlateBlue * ts };
            portraitGhostifyer = new ColorManipulation(colors);
            spriteGhostifyer = new ColorManipulation(colorsTransparent);
            mapsGhostifyer = GhostTownMod.config.desaturate ? new ColorManipulation(40, 100) : new ColorManipulation();
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (!GhostTownMod.config.animals && asset.AssetName.Contains("Animals"))
                return false;

            if (!GhostTownMod.config.critters && asset.AssetName.Contains("critters"))
                return false;

            if (!GhostTownMod.config.houses && asset.AssetName.Contains("_town"))
                return false;

            if (!GhostTownMod.config.people && (asset.AssetName.Contains("Portraits") || asset.AssetName.Contains("Characters")))
                return false;

            return asset.DataType.Equals(typeof(Texture2D)) && !asset.AssetName.Contains("Farmer");
        }

        public void Edit<T>(IAssetData asset)
        {
            asset.AsImage().ReplaceWith(asset.AsImage().Data.changeColor(asset.AssetName.Contains("Portraits") ? portraitGhostifyer : (asset.AssetName.Contains("_town") || asset.AssetName.Contains("Animals") || asset.AssetName.Contains("critters") || (asset.AssetName.Contains("Characters") && !asset.AssetName.Contains("Monsters"))) ? spriteGhostifyer : mapsGhostifyer));
        }

    }
}
