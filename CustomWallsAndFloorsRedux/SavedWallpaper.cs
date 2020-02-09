using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomWallsAndFloorsRedux
{
    public class SavedWallpaper
    {
        public string SetId { get; set; }

        public int CustomIndex { get; set; }

        public string Location { get; set; }

        public int TileX { get; set; } = -1;

        public int TileY { get; set; } = -1;

        public int X { get; set; }

        public int Y { get; set; }

        public bool IsFloors { get; set; } = false;

        public string Layer { get; set; }

        internal bool ShouldBeSend { get; set; } = false;

        public SavedWallpaper()
        {

        }

        public SavedWallpaper(string id, int index, string location, int x, int y, bool isFloors, bool shouldBeSend = false)
        {
            SetId = id;
            CustomIndex = index;
            Location = location;
            X = x;
            Y = y;
            IsFloors = isFloors;
            ShouldBeSend = shouldBeSend;
        }

    }
}
