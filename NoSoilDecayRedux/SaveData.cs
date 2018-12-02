using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;

namespace NoSoilDecayRedux
{
    public class SaveData
    {

        public List<SaveTiles> data { get; set; } = new List<SaveTiles>();

        public SaveData()
        {

        }

        public SaveData(Dictionary<GameLocation, List<Vector2>> data)
        {
            this.data = new List<SaveTiles>();
            foreach (KeyValuePair<GameLocation, List<Vector2>> l in data)
                this.data.Add(new SaveTiles(l.Key.name, l.Value));
        }

    }
}
