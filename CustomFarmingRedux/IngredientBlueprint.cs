using StardewValley;
using PyTK.Extensions;

namespace CustomFarmingRedux
{
    public class IngredientBlueprint
    {
        public int _index = -1;
        public int quality { get; set; } = 0;
        public int exactquality { get; set; } = -1;
        public int stack { get; set; } = 1;
        public int index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
            }
        }
        public string name
        {
            get
            {
                return Game1.objectInformation[_index].Split('/')[4];
            }
        }

        public string item {
            set
            {
                int.TryParse(value, out _index);
                if (_index <= 0)
                    Game1.objectInformation.getIndexByName(value);
            }
        }
        
        public IngredientBlueprint clone()
        {
            IngredientBlueprint clone = new IngredientBlueprint();
            clone.index = index;
            clone.stack = stack;
            clone.quality = quality;
            return clone;
        }
    }
}
