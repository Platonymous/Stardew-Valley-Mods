using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int readyindex { get; set; } = 0;
        public int frames { get; set; } = 0;
        public int fps { get; set; } = 6;
        public bool showitem { get; set; } = false;
        public int[] itempos { get; set; }
        public int index { get; set; } = -1;
        public int tilewidth { get; set; } = 16;
        public int tileheight { get; set; } = 32;
        public bool water { get; set; } = false;
        public CustomFarmingPack pack;
        public string folder => pack.folderName;
        public string file => pack.fileName;
        public IngredientBlueprint starter { get; set; }
    }
}
