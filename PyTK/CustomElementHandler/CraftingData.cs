using PyTK.Extensions;
using StardewValley;

namespace PyTK.CustomElementHandler
{
    public class CraftingData
    {
        public string name { get; set; }
        public string displayName { get; set; }
        public string recipe { get; set; }
        public bool field { get; set; }
        public bool learned { get; set; }
        private int _index = -1;
        public int index
        {
            get => _index;
           
            set
            {
                _index = value;
                if (_index != -1)
                {
                    
                    CraftingRecipe.craftingRecipes.AddOrReplace(name, data);

                    if (!Game1.player.craftingRecipes.ContainsKey(name) && delivery == "null")
                        Game1.player.craftingRecipes.Add(name, 0);
                }
            }
        }
        public bool bigCraftable { get; set; }
        public string delivery { get; set; }
        public string data
        {
            get
            {
                string fieldString = field ? "Field" : "Home";
                return $"{recipe}/{fieldString}/{index.ToString()}/{bigCraftable.ToString().ToLower()}/{delivery}/{displayName}";
            }
        }

        public CraftingData(string name, string recipe = "388 2", string displayName = "", int index = -1, bool bigCraftable = true, bool field = false, string delivery = "null")
        {
            this.name = name;
            this.displayName = displayName;

            if (displayName == "")
                this.displayName = name;

            this.recipe = recipe;
            this.field = field;
            this.delivery = delivery;
            this.index = index;
            this.bigCraftable = bigCraftable;
        }

    }
}
