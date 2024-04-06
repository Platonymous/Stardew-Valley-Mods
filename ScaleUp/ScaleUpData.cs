using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
#if DEBUG || RELEASE
using StardewValley.GameData.FloorsAndPaths;
#endif
using System;
using System.Net.NetworkInformation;

namespace Portraiture2
{
    public class ScaleUpData
    {
        public string Asset { get; set; }

        private int _Width = -1;
        private int _Height = -1;

        internal int Width
        {
            get
            {
                if (_Height == -1 || _Width == -1)
                {
                    var tex = ScaleUpMod.Singleton.Helper.GameContent.Load<Texture2D>(Asset);
                    _Height = tex.Height;
                    _Width = tex.Width;

                }
                return _Width;
            }
            set
            {
                _Width = value;
            }
        }

        internal int Height
        {
            get
            {
                if (_Height == -1 || _Width == -1)
                {
                    var tex = ScaleUpMod.Singleton.Helper.GameContent.Load<Texture2D>(Asset);
                    _Height = tex.Height;
                    _Width = tex.Width;

                }
                return _Height;
            }
            set
            {
                _Height = value;
            }
        }


        private int _orgWidth = -1;
        private int _orgHeight = -1;
        internal int OrgHeight
        {
            get
            {
                if (_orgHeight == -1 || _orgWidth == -1)
                {
                    _orgHeight = (int)((Height - PaddingHeight) / Scale);
                    _orgWidth = (int)((Width - PaddingWidth) / Scale);
                }

                return _orgHeight;
            }
            set
            {
                _orgHeight = value;
            }
        }

        internal int OrgWidth { 
            get
            {
                if (_orgHeight == -1 || _orgWidth == -1)
                {
                    _orgHeight = (int)((Height - PaddingHeight) / Scale);
                    _orgWidth = (int)((Width - PaddingWidth) / Scale);
                }

                return _orgWidth;
            }
            set
            {
                _orgWidth = value;
            }
        }

        public int PaddingWidth { get; set; } = 0;

        public int PaddingHeight { get; set; } = 0;

        public float Scale { get; set; } = 1;

        public bool Padded => PaddingWidth + PaddingHeight > 0;

        public Rectangle? GetScaledSource(Rectangle? source, int originalWidth, int originalHeight,out int padx, out int pady, bool force = false)
        {
            padx = 0; pady = 0;
            if (source.HasValue)
            {
                int tilesX = OrgWidth / originalWidth;
                int tilesY = OrgHeight / originalHeight;
                int x = source.Value.X / originalWidth;
                int y = source.Value.Y / originalHeight;
                padx = (int)((PaddingWidth / (float)tilesX));
                pady = (int)((PaddingHeight / (float)tilesY));
                var tileWidth = originalWidth * Scale + padx;
                var tileHeight = originalHeight * Scale + pady;

                return GetSourceRectForStandardTileSheet(Width, y * tilesX + x,(int)tileWidth,(int)tileHeight);
            }
            else if (force)
                return new Rectangle(0, 0, Width, Height);

            return source;
        }

        public static Rectangle GetSourceRectForStandardTileSheet(int textureWidth, int tilePosition, int width = -1, int height = -1)
        {

            return new Rectangle(tilePosition * width % textureWidth, tilePosition * width / textureWidth * height, width, height);
        }

    }
}
