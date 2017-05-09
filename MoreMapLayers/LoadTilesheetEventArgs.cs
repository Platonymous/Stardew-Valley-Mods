using System;
using System.Collections.Generic;
using xTile.Tiles;
using xTile.Display;
using Microsoft.Xna.Framework.Graphics;

namespace MoreMapLayers
{
    public class LoadTilesheetEventArgs : EventArgs
    {
        private TileSheet tilesheet;
        private XnaDisplayDevice device;
        private Dictionary<TileSheet, Texture2D> textures;

        public LoadTilesheetEventArgs(TileSheet tilesheet, XnaDisplayDevice device, Dictionary<TileSheet,Texture2D> textures)
        {
            this.tilesheet = tilesheet;
            this.device = device;
            this.textures = textures;
        }

        public TileSheet LastTileSheet
        {
            get
            {
                return tilesheet;
            }
        }

        public XnaDisplayDevice MapDisplayDevice
        {
            get
            {
                return device;
            }
        }

        public Dictionary<TileSheet,Texture2D> Textures
        {
            get
            {
                return textures;
            }
        }

       
    }
}