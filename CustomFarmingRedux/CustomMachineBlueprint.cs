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
        public int frames { get; set; } = 1;
        public int fps { get; set; } = 6;
        public bool showitem { get; set; } = false;
        public int[] itempos { get; set; }
        public bool custom { get; set; } = false;
        public int index { get; set; } = -1;
        public string folder;
        public string file;
        public IngredientBlueprint starter { get; set; }
    }
}
