using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using xTile.Dimensions;

namespace MoreMapLayers
{
    public class MoreMapLayers : Mod
    {

        public override void Entry(IModHelper helper)
        {
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;
        }

        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            DrawMapEvents.DrawMapLayer -= DrawMapEvents_DrawMapLayer;
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            Game1.mapDisplayDevice = new MapDisplayDeviceIntercept((xTile.Display.XnaDisplayDevice)Game1.mapDisplayDevice);
            DrawMapEvents.DrawMapLayer += DrawMapEvents_DrawMapLayer;
        }

        private void DrawMapEvents_DrawMapLayer(object sender, DrawLayerEventArgs e)
        {
            if (e.PriorLayerID == "Back" && e.NewLayerID == "Buildings")
            {
                int i = 0;
                while (Game1.currentLocation.Map.Layers[i].Id != "Buildings")
                {
                    if (Game1.currentLocation.Map.Layers[i].Id != "Back")
                    {
                        Game1.currentLocation.Map.Layers[i].Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, Game1.pixelZoom);
                    }

                    i++;
                }
            }
        }
    }
    
}
