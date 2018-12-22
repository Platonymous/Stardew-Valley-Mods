using System.Collections.Generic;
using StardewModdingAPI;

namespace CustomFarmingRedux
{
    public class CustomFarmingPack
    {
        public string folderName { get; set; }
        public string fileName { get; set; }
        public string baseFolder { get; set; } = "ContentPack";
        internal IContentPack contentPack { get; set; } = null;
        public string useid { get; set; } = "";
        public string author { get; set; } = "none";
        public string version { get; set; } = "1.0.0";
        public string name { get; set; } = "Custom Farming Pack";
        public List<CustomMachineBlueprint> machines { get; set; } = new List<CustomMachineBlueprint>();
    }
}
