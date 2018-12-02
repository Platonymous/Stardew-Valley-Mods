using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace NoSoilDecayRedux
{
    public class SaveTiles
    {
        public string location { get; set; } = "Farm";
        public List<Vector2> tiles = new List<Vector2>();

        public SaveTiles()
        {

        }

        public SaveTiles(string location, List<Vector2> tiles)
        {
            this.location = location;
            this.tiles = tiles;
        }
    }
}
