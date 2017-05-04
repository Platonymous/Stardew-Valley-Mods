using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreMapLayers
{
    public static class DrawMapEvents
    {

        public static event EventHandler<DrawLayerEventArgs> DrawMapLayer;


        internal static void OnDrawMapLayer(object sender, DrawLayerEventArgs e)
        {
            DrawMapEvents.DrawMapLayer?.Invoke(sender, e);
        }

    }
}
