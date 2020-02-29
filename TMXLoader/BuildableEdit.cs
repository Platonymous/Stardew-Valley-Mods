using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace TMXLoader
{
    public class BuildableEdit : MapEdit
    {
        public string id { get; set; }
        public string indoorsFile { get; set; }
        public List<TileShopItem> buildItems { get; set; } = new List<TileShopItem>();
        public string iconFile { get; set; }
        public int[] exitTile { get; set; } = new int[] { 0, 0 };
        public string set { get; set; } = "Others";
        public int price { get; set; } = 10000;

        internal Texture2D _icon = null;
        internal string _mapName = null;
        internal string _location = null;

        public List<string> tags { get; set; } = new List<string>();

        public Dictionary<string, BuildableTranslation> translations { get; set; } = new Dictionary<string, BuildableTranslation>();

        public BuildableEdit()
        {

        }

        public BuildableEdit(string id,string indoorsMap,string iconFile,int price,Texture2D icon, string mapName, string location, IContentPack pack)
        {
            this.id = id;
            this.indoorsFile = indoorsMap;
            this.iconFile = iconFile;
            this.price = price;
            this._icon = icon;
            this._mapName = mapName;
            this._location = location;
        }
        public BuildableEdit Clone()
        {
            BuildableEdit b = new BuildableEdit(id, indoorsFile, iconFile, price, _icon, _mapName, _location, _pack);
            b.conditions = conditions;
            b.file = file;
            b.info = info;
            b.name = name;
            b.position = position;
            b.removeEmpty = removeEmpty;
            b.removeWarps = removeWarps;
            b.retainWarps = retainWarps;
            b.sourceArea = sourceArea;
            b.type = type;
            b._map = _map;
            b.tags = tags;
            return b;
        }
    }
}
