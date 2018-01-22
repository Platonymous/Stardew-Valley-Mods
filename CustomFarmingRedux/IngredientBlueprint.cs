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
        public string name { get; set; } = "";
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
