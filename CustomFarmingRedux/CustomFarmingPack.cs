using PyTK.Types;
using System.Collections.Generic;

namespace CustomFarmingRedux
{
    public class CustomFarmingPack : IContentPack
    {
        public string folderName { get; set; }
        public string fileName { get; set; }
        public string baseFolder
        {
            get => legacy ? CustomFarmingReduxMod.legacyFolder : CustomFarmingReduxMod.folder;
        }
        public bool legacy { get; set; } = false;
        public string author { get; set; } = "none";
        public string version { get; set; } = "1.0.0";
        public string name { get; set; } = "Custom Farming Pack";
        public List<CustomMachineBlueprint> machines { get; set; }
    }
}
