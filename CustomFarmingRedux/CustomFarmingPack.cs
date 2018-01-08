using PyTK.Types;

namespace CustomFarmingRedux
{
    public class CustomFarmingPack : IContentPack
    {
        public string folderName { get; set; }
        public string fileName { get; set; }
        public string author { get; set; } = "none";
        public string version { get; set; } = "1.0.0";
        public string name { get; set; } = "Custom Farming Pack";
        public CustomMachineBlueprint[] machines { get; set; }
    }
}
