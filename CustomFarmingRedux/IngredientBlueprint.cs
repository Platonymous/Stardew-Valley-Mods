using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SObject = StardewValley.Object;
using PyTK.Extensions;
using Microsoft.Xna.Framework;

namespace CustomFarmingRedux
{
    public class IngredientBlueprint
    {
        public int _index = -1;
        public int quality { get; set; } = 0;
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

        public SObject get()
        {
            return new SObject(Vector2.Zero, index, stack);
        }
    }
}
