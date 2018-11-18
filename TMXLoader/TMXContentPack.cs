using System.Collections.Generic;

namespace TMXLoader
{
    internal class TMXContentPack
    {
        public List<MapEdit> addMaps { get; set; } = new List<MapEdit>();
        public List<MapEdit> replaceMaps { get; set; } = new List<MapEdit>();
        public List<MapEdit> mergeMaps { get; set; } = new List<MapEdit>();
        public List<MapEdit> onlyWarps { get; set; } = new List<MapEdit>();
        public List<SpouseRoom> spouseRooms { get; set; } = new List<SpouseRoom>();
        public List<NPCPlacement> festivalSpots { get; set; } = new List<NPCPlacement>();
        public List<string> scripts { get; set; } = new List<string>();
    }
}
