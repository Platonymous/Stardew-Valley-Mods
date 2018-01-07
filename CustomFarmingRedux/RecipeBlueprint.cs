using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PyTK.Extensions;
using Microsoft.Xna.Framework.Graphics;
using SObject = StardewValley.Object;
using Microsoft.Xna.Framework;
using StardewValley.Objects;

namespace CustomFarmingRedux
{
    public class RecipeBlueprint
    {
        public string _name = "";
        public string _description = "";
        public string _category = "";
        public int _tileindex = -1;
        public int _index = -1;
        public string name
        {
            get
            {
                if (_name == "")
                    return Game1.objectInformation[_index];
                else
                    return _name;
            }
            set
            {
                _name = value;
            }
        }
        public string description
        {
            get
            {
                if (_description == "")
                    return new SObject(Vector2.Zero, _index).getDescription();
                else
                    return _description;
            }
            set
            {
                _description = value;
            }
        }
        public string category
        {
            get
            {
                if (_category == "")
                    return new SObject(Vector2.Zero, _index).getCategoryName();
                else
                    return _category;
            }
            set
            {
                _category = value;
            }
        }
        public int index
        {
            get
            {
                if (_index == -1)
                    return Game1.objectInformation.getIndexByName(_name);
                else
                    return _index;
            }
            set
            {
                _index = value;
            }
        }
        public string item
        {
            set
            {
                int.TryParse(value, out _index);
                if (_index == -1)
                    _index = Game1.objectInformation.getIndexByName(value);
            }
        }
        public List<IngredientBlueprint> materials { get; set; }
        public Texture _texture { get; set; } = Game1.objectSpriteSheet;
        public string texture { get; set; }
        public int tileindex
        {
            get
            {
                if (_tileindex == -1)
                    return _index;
                else
                    return _tileindex;
            }
            set
            {
                _tileindex = value;
            }
        }
        public bool prefix { get; set; } = false;
        public bool suffix { get; set; } = false;
        public bool colored { get; set; } = false;
        public int[] color { get; set; } = null;
        public int time { get; set; }
        public int stack { get; set; } = 1;
        public int quality { get; set; } = 0;
        public bool custom { get; set; } = false;

        public void consumeIngredients( List<List<Item>> items)
        {
  
            List<IngredientBlueprint> ingredients = materials.toList(p => p);

            for (int i = 0; i < materials.Count; i++)
                for (int list = 0; list < items.Count; list++)
                    if (ingredients.Count <= 0)
                        return;
                    else if (ingredients.Contains(materials[i]))
                        if (items[list].Find(p => p.parentSheetIndex == materials[i].index && materials[i].quality > 0 && p is SObject o && o.quality >= materials[i].quality) is Item j)
                        {
                            j.Stack -= materials[i].stack;
                            int ii = ingredients.FindIndex(p=> p == materials[i]);
                            ingredients[ii].stack = (j.Stack >= 0) ? 0 : Math.Abs(j.Stack);
                            if (ingredients[ii].stack == 0)
                                ingredients.Remove(ingredients[ii]);
                            if (j.Stack < 1)
                                items[list].Remove(j);
                        }
        }

        public bool hasIngredients(List<List<Item>> items)
        {
            List<IngredientBlueprint> ingredients = materials.toList(p => p);

            for (int i = 0; i < materials.Count; i++)
                for (int list = 0; list < items.Count; list++)
                    if (ingredients.Count <= 0)
                        return true;
                    else if (ingredients.Contains(materials[i]))
                        if (items[list].Find(p => p.parentSheetIndex == materials[i].index && materials[i].quality > 0 && p is SObject o && o.quality >= materials[i].quality) is Item j)
                        {
                            int ii = ingredients.FindIndex(p => p == materials[i]);
                            ingredients[ii].stack = (j.Stack - ingredients[ii].stack >= 0) ? 0 : Math.Abs(j.Stack - ingredients[ii].stack);
                            if (ingredients[ii].stack == 0)
                                ingredients.Remove(ingredients[ii]);
                        }

            if (ingredients.Count <= 0)
                return true;
            else
                return false;
        }

        public void consumeIngredients(List<Item> items)
        {
            consumeIngredients(new List<List<Item>>() { items });
        }

        public bool hasIngredients(List<Item> items)
        {
            return hasIngredients(new List<List<Item>>() { items });
        }

        public SObject createObject(Color color)
        {
            if (colored && color != Color.White)
            {
                ColoredObject c = new ColoredObject(index, stack, color);
                c.quality = quality;
                return c;
            }
            else
                return createObject();
        }

        public SObject createObject()
        {
            SObject s = new SObject(Vector2.Zero, index, stack);
            s.quality = quality;
            return s;
        }
    }
}
