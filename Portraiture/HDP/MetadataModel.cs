using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portraiture.HDP
{
    public class MetadataModel
    {
        //https://github.com/tlitookilakin/HDPortraits/blob/master/HDPortraits/Models/MetadataModel.cs
        public int Size { set; get; } = 64;
        public AnimationModel Animation { get; set; } = null;
        public string Portrait
        {
            get => portraitPath;
            set
            {
                portraitPath = value;
                overrideTexture.Reload();
            }
        }

        public readonly LazyAsset<Texture2D> overrideTexture;
        private string portraitPath = null;
        public readonly LazyAsset<Texture2D> originalTexture;
        internal string originalPath = null;

        public MetadataModel()
        {
            overrideTexture = new(PortraitureMod.helper, () => portraitPath)
            {
                CatchErrors = true
            };
            originalTexture = new(PortraitureMod.helper, () => originalPath);
        }

        public bool TryGetTexture(out Texture2D texture)
        {
            if (portraitPath is null)
            {
                texture = originalTexture.Value;
                return true;
            }
            texture = overrideTexture.Value;
            if (overrideTexture.LastError is not null)
            {
                return false;
            }
            return true;
        }
        public Rectangle GetRegion(int which, int millis = -1)
        {
            var missing = !TryGetTexture(out var tex);
            int size = missing ? 64 : Size;
            return Animation is null ? Game1.getSourceRectForStandardTileSheet(tex, which, size, size) :
                Animation.GetSourceRegion(tex, size, which, millis);
        }
    }
}
