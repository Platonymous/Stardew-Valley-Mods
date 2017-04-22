using System;

namespace InteractiveMapLayer
{
    public class DrawLayerEventArgs : EventArgs
    {
        private string priorLayer;
        private string newLayer;

        public DrawLayerEventArgs(string priorLayer, string newLayer)
        {
            this.priorLayer = priorLayer;
            this.newLayer = newLayer;
        }

        public string PriorLayerID
        {
            get
            {
                return priorLayer;
            }
        }

        public string NewLayerID
        {
            get
            {
                return newLayer;
            }
        }
    }
}