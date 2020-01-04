using StardewValley;
using PyTK.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CustomFarmingRedux
{
    public class IngredientBlueprint
    {
        public int _index = -1;
        public int quality { get; set; } = 0;
        public int exactquality { get; set; } = -1;
        public int stack { get; set; } = 1;
        public string name { get; set; } = "";
        public string item { get => name; set => name = value; }

        public List<string> context { get; set; } = new List<string>();
        public int index
        {
            get
            {
                if ((_index == 0 || _index == -1) && name != "")
                    _index = Game1.objectInformation.getIndexByName(name);

                return _index;
            }
            set
            {
                _index = value;
            }
        }

        public IngredientBlueprint()
        {

        }
        
        public IngredientBlueprint clone()
        {
            IngredientBlueprint clone = new IngredientBlueprint();
            clone.index = index;
            clone.name = name;
            clone.exactquality = exactquality;
            clone.stack = stack;
            clone.quality = quality;
            return clone;
        }
    }
}
