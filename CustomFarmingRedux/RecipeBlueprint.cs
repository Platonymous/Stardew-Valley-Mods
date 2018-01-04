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
        public int time { get; set; }
        public int stack { get; set; } = 1;
    }
}
