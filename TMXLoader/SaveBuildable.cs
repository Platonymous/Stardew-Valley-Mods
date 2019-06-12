using Microsoft.Xna.Framework;
using PyTK.Types;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile;

namespace TMXLoader
{
    public class SaveBuildable
    {
        public string Id { get; set; }

        public string Location { get; set; }
        public int[] Position { get; set; }
        public string UniqueId { get; set; }

        public Dictionary<string, string> Colors { get; set; } = new Dictionary<string, string>();

        public SaveLocation Indoors { get; set; }

        internal TMXAssetEditor _editor;

        public SaveBuildable()
        {

        }

        public SaveBuildable(string id, string location, Point position, string uniqueId, Dictionary<string,string> colors, TMXAssetEditor editor)
        {
            Position = new int[2]{position.X, position.Y };
            Id = id;
            UniqueId = uniqueId;
            Location = location;
            Colors = colors;
            _editor = editor;
            editor.saveBuildable = this;
        }
    }
}
