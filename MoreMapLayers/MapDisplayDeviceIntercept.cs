using Microsoft.Xna.Framework.Graphics;

using System.Collections.Generic;

using xTile.Dimensions;
using xTile.Display;
using xTile.Tiles;

namespace MoreMapLayers
{
    class MapDisplayDeviceIntercept : IDisplayDevice
    {
        public XnaDisplayDevice device;
        private string lastTileLayerID;
        private Dictionary<TileSheet, Texture2D> textures;

        public MapDisplayDeviceIntercept()
        {

        }

        public MapDisplayDeviceIntercept(XnaDisplayDevice xna)
        {
            device = xna;
            textures = MoreMapLayers.helper.Reflection.GetPrivateValue<Dictionary<TileSheet, Texture2D>>(device, "m_tileSheetTextures");
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
            
            DrawMapEvents.OnLoadTileSheet(this, new LoadTilesheetEventArgs(tileSheet,device,textures));
        }

        public void SetClippingRegion(Rectangle clippingRegion)
        {

            device.SetClippingRegion(clippingRegion);
        }
    }
}
