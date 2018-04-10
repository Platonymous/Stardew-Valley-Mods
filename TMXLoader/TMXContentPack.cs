using PyTK.Types;
using System.Collections.Generic;

namespace TMXLoader
{
    internal class TMXContentPack : IContentPack
    {
        public string name { get; set; } = "TMXContentPack";
        public string version { get; set; } = "1.0.0";
        public string author { get; set; } = "none";
        public string folderName { get; set; } = "";
        public string fileName { get; set; } = "";
        public List<MapEdit> addMaps { get; set; } = new List<MapEdit>();
        public List<MapEdit> replaceMaps { get; set; } = new List<MapEdit>();
        public List<MapEdit> mergeMaps { get; set; } = new List<MapEdit>();
        public List<MapEdit> onlyWarps { get; set; } = new List<MapEdit>();
        public List<string> scripts { get; set; } = new List<string>();
    }
}
