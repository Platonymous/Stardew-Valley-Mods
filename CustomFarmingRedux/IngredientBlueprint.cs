using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PyTK.Extensions;

namespace CustomFarmingRedux
{
    public class IngredientBlueprint
    {
        public int _index = -1;
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
        public string item {
            set
            {
                int.TryParse(value, out _index);
                if (_index == -1)
                    Game1.objectInformation.getIndexByName(value);
            }
        }
        public int quality { get; set; } = 0;
        public int stack { get; set; } = 1;
    }
}
