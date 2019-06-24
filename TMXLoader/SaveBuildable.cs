using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace TMXLoader
{
    public class SaveBuildable
    {
        public string Id { get; set; }

        public string Location { get; set; }
        public int[] Position { get; set; }
        public string UniqueId { get; set; }

        public string PlayerName { get; set; } = null;

        public long PlayerId { get; set; } = -1;

        public Dictionary<string, string> Colors { get; set; } = new Dictionary<string, string>();

        public SaveLocation Indoors { get; set; }

        public SaveBuildable()
        {

        }

        public SaveBuildable(string id, string location, Point position, string uniqueId, string playerName, long playerId, Dictionary<string,string> colors)
        {
            Position = new int[2]{position.X, position.Y };
            Id = id;
            UniqueId = uniqueId;
            Location = location;
            Colors = colors;
            PlayerId = playerId;
            PlayerName = playerName;
        }
    }
}
