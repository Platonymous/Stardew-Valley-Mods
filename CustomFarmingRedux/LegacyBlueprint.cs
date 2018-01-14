using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PyTK.Types;

namespace CustomFarmingRedux
{
    public class LegacyBlueprint : IContentPack
    {
        public string folderName { get; set; }
        public string fileName { get; set; }
        public string author { get; set; } = "none";
        public string version { get; set; } = "1.0.0";
        public string name { get; set; } = "Custom Farming Pack";

        public string Type { get; set; } = "CustomFarming.simpleMachine";
        public string Crafting { get; set; } = "388 30";
        public int AnimationSpeed { get; set; } = 6;
        public string Name { get => name; set => name = value; }
        public string Description { get; set; } = "Custom Machine";
        public string Tilesheet { get; set; } = "";
        public int TileIndex { get; set; } = 0;
        public int ReadyTileIndex { get; set; } = 0;
        public int WorkAnimationFrames { get; set; } = 0;
        public string CategoryName { get; set; } = "Crafting";
        public int[] Materials { get; set; } = new int[0];
        public int MaterialQuality { get; set; } = 0;
        public int RequieredStack { get; set; } = 1;
        public int RequiredStack { get => RequieredStack; set => RequieredStack = value; }
        public int StarterMaterial { get; set; } = -1;
        public int StarterMaterialStack { get; set; } = 1;
        public LegacyProduce Produce { get; set; }
        public List<LegacySpecialProduce> SpecialProduce = new List<LegacySpecialProduce>();
        public bool displayItem { get; set; } = false;
        public int displayItemX { get; set; } = 0;
        public int displayIemY { get; set; } = 0;
        public float displayItemZoom { get; set; } = 1;



    }
}
