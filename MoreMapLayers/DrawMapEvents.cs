using System;
using xTile.Tiles;

namespace MoreMapLayers
{
    public static class DrawMapEvents
    {

        public static event EventHandler<DrawLayerEventArgs> DrawMapLayer;
        public static event EventHandler<LoadTilesheetEventArgs> LoadTileSheet;
        public static event EventHandler<LoadTilesheetEventArgs> BeforeLoadTileSheet;


        internal static void OnDrawMapLayer(object sender, DrawLayerEventArgs e)
        {
            DrawMapEvents.DrawMapLayer?.Invoke(sender, e);
        }

        internal static void OnLoadTileSheet(object sender, LoadTilesheetEventArgs e)
        {
            DrawMapEvents.LoadTileSheet?.Invoke(sender, e);
        }

        internal static void OnBeforeLoadTileSheet(object sender, LoadTilesheetEventArgs e)
        {
            DrawMapEvents.BeforeLoadTileSheet?.Invoke(sender, e);
        }

    }
}
