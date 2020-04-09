using StardewModdingAPI;
using System.Collections.Generic;

namespace TMXLoader
{
    internal class TMXContentPack
    {
        public List<string> hasMods { get; set; } = new List<string>();

        public List<string> hasNotMods { get; set; } = new List<string>();
        public List<string> alsoLoad { get; set; } = new List<string>();
        public List<MapEdit> addMaps { get; set; } = new List<MapEdit>();
        public List<MapEdit> replaceMaps { get; set; } = new List<MapEdit>();
        public List<MapEdit> mergeMaps { get; set; } = new List<MapEdit>();

        public List<BuildableEdit> buildables { get; set; } = new List<BuildableEdit>();
        public List<MapEdit> onlyWarps { get; set; } = new List<MapEdit>();
        public List<TileShop> shops { get; set; } = new List<TileShop>();
        public List<SpouseRoom> spouseRooms { get; set; } = new List<SpouseRoom>();
        public List<NPCPlacement> festivalSpots { get; set; } = new List<NPCPlacement>();
        public List<NPCPlacement> placeNPCs { get; set; } = new List<NPCPlacement>();
        public List<string> scripts { get; set; } = new List<string>();
        public bool loadLate { get; set; } = false;

        internal IContentPack parent = null;
    }
}
