using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;


namespace CustomFarmingRedux
{
    public class CustomMachineBlueprint
    {
        public int id { get; set; }
        public string fullid
        {
            get
            {
                return $"{folder}.{file}.{id}";
            }
        }
        public string name { get; set; }
        public string description { get; set; }
        public string category { get; set; } = "Crafting";
        public List<RecipeBlueprint> production { get; set; }
        public Texture2D _texture { get; set; }
        public string texture { get; set; }
        public int tileindex { get; set; } = 0;
        public int readyindex { get; set; } = -1;
        public int frames { get; set; } = 0;
        public int fps { get; set; } = 6;
        public bool showitem { get; set; } = false;
        public int[] itempos { get; set; } = new int[] { 0, 0 };
        public float itemzoom { get; set; } = 1;
        public int index { get; set; } = -1;
        public int tilewidth { get; set; } = 16;
        public int tileheight { get; set; } = 32;
        public bool water { get; set; } = false;
        public bool pulsate { get; set; } = true;
        public CustomFarmingPack pack;
        public string folder => pack.folderName;
        public string file => pack.fileName;
        public IngredientBlueprint starter { get; set; }
        public bool forsale { get; set; } = false;
        public string shop { get; set; } = "Robin";
        public int price { get; set; } = 100;
        public string condition { get; set; }
        public string crafting { get; set; }
    }
}
