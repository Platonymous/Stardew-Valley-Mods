using Microsoft.Xna.Framework.Graphics;
using xTile.Dimensions;
using xTile.Display;
using xTile.Tiles;
using StardewValley;
using System;

namespace InteractiveMapLayer
{
    class mapDisplayDeviceIntercept : IDisplayDevice
    {
        private XnaDisplayDevice device;
        private string lastTileLayerID;

        public mapDisplayDeviceIntercept()
        {

        }

        public mapDisplayDeviceIntercept(XnaDisplayDevice xna)
        {
            device = xna;
        }

        public void BeginScene(SpriteBatch b)
        {
            lastTileLayerID = "New";
            device.BeginScene(b);
        }

        public void DisposeTileSheet(TileSheet tileSheet)
        {
            
            device.DisposeTileSheet(tileSheet);
        }


        public void DrawTile(Tile tile, Location location, float layerDepth)
        {
            if(lastTileLayerID != tile.Layer.Id)
            {
               DrawMapEvents.OnDrawMapLayer(this, new DrawLayerEventArgs(lastTileLayerID, tile.Layer.Id));
            }
            lastTileLayerID = tile.Layer.Id;
            device.DrawTile(tile, location, layerDepth);
        }

        public void EndScene()
        {
           
            device.EndScene();
        }

        public void LoadTileSheet(TileSheet tileSheet)
        {
            
            device.LoadTileSheet(tileSheet);
        }

        public void SetClippingRegion(Rectangle clippingRegion)
        {

            device.SetClippingRegion(clippingRegion);
        }
    }
}
