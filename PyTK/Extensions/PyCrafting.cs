using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace PyTK.Extensions
{
    public static class PyCrafting
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        public static void consumeIngredients(this CraftingRecipe current, List<List<Item>> items)
        {
            Dictionary<int, int> recipeList = Helper.Reflection.GetField<Dictionary<int, int>>(current, "recipeList").GetValue();
            Dictionary<int, int> ingredients = recipeList.clone();

            foreach (int i in recipeList.Keys)
                for (int list = 0; list < items.Count; list++)
                    if (ingredients.Count <= 0)
                        return;
                    else if (ingredients.ContainsKey(i))
                        if (items[list].Find(p => p.ParentSheetIndex == i) is Item j)
                        {
                            j.Stack -= ingredients[i];
                            ingredients[i] = (j.Stack >= 0) ? 0 : Math.Abs(j.Stack);
                            if (ingredients[i] == 0)
                                ingredients.Remove(i);
                            if (j.Stack < 1)
                                items[list].Remove(j);
                        }
        }

        public static bool hasIngredients(this CraftingRecipe current, List<List<Item>> items)
        {
            Dictionary<int, int> recipeList = Helper.Reflection.GetField<Dictionary<int, int>>(current, "recipeList").GetValue();
            Dictionary<int, int> ingredients = recipeList.clone();

            foreach (int i in recipeList.Keys)
                for (int list = 0; list < items.Count; list++)
                    if (ingredients.Count <= 0)
                        return true;
                    else if (ingredients.ContainsKey(i))
                        if (items[list].Find(p => p.ParentSheetIndex == i) is Item j)
                        {
                            ingredients[i] = (j.Stack - ingredients[i] >= 0) ? 0 : Math.Abs(j.Stack - ingredients[i]);
                            if (ingredients[i] == 0)
                                ingredients.Remove(i);
                        }
            if (ingredients.Count <= 0)
                return true;
            else
                return false;
        }

        public static void consumeIngredients(this CraftingRecipe current, List<Item> items)
        {
            current.consumeIngredients(new List<List<Item>>() { items });
        }

        public static bool hasIngredients(this CraftingRecipe current, List<Item> items)
        {
            return current.hasIngredients(new List<List<Item>>() { items });
        }
    }
}
