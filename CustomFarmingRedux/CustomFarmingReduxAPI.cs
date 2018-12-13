using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomFarmingRedux
{
    public class CustomFarmingReduxAPI : ICustomContentAPI
    {
        internal IModHelper Helper = CustomFarmingReduxMod._helper;
        internal IMonitor Monitor = CustomFarmingReduxMod._monitor;

        public Item getCustomObject(string id)
        {
            CustomMachine machine = new CustomMachine(CustomFarmingReduxMod.machines.Find(m => m.fullid == id || m.legacy == id));

            if (machine != null) {
                Monitor.Log("API: Requested machine " + id + " not found.",LogLevel.Error);
                return null;
            } 

            return machine;
        }

        private RecipeBlueprint findRecipe(CustomMachineBlueprint blueprint, List<Item> items)
        {
            RecipeBlueprint result = null;
            if (blueprint.production != null)
                foreach (RecipeBlueprint r in blueprint.production)
                    if (r.hasIngredients(items))
                        result = r;

            return result;
        }

        private Item maxed(Item obj)
        {
            Item o = obj.getOne();
            o.Stack = int.MaxValue;
            return o;
        }

        /// <summary>Returns the name, Texture and required stack of all machines that can use the specified item</summary>
        /// <param name="item">The item to be used as material.</param>
        public List<Tuple<string, Texture2D, int, int>> getMachinesForItem(StardewValley.Object item)
        {
            List<Tuple<string,Texture2D, int, int>> result = new List<Tuple<string, Texture2D, int, int>>();

            foreach (CustomMachineBlueprint blueprint in CustomFarmingReduxMod.machines)
            {
                var find = findRecipe(blueprint, new List<Item> { maxed(item) });
                if (find is RecipeBlueprint recipe && recipe.materials != null && recipe.materials.Count > 0) {
                    var material = recipe.materials.Find(i => i.index == -999 || i.index == item.ParentSheetIndex || item.Category == i.index);
                    if(material is IngredientBlueprint ing)
                        result.Add(new Tuple<string, Texture2D, int, int>(blueprint.name, blueprint.getTexture(), blueprint.tileindex, ing.stack));
                }
            }

            return result;
        }

        /// <summary>Returns the respective machine and draw specs of a custom object dummy item</summary>
        /// <param name="dummy">The dummy item that would be replaced by the custom item</param>
        public Tuple<Item,Texture2D, Rectangle, Color> getRealItemAndTexture(StardewValley.Object dummy)
        {
            var result = CustomObjectData.collection.Find(c => c.Value.sdvId == dummy.ParentSheetIndex && dummy.bigCraftable.Value == c.Value.bigCraftable);
            if (result is KeyValuePair<string,CustomObjectData> kvp && kvp.Value is CustomObjectData cod)
                return new Tuple<Item,Texture2D, Rectangle, Color>(cod.getObject(),cod.texture, cod.sourceRectangle,cod.color);

            return null;
        }

        /// <summary>Get whether a given item is a custom object or machine from Custom Farming Redux.</summary>
        /// <param name="item">The item instance.</param>
        public bool isCustom(Item item)
        {
            return item is CustomMachine || item is CustomObject;
        }

        /// <summary>Get the spritesheet texture for a custom object or machine (if applicable).</summary>
        /// <param name="item">The item instance.</param>
        public Texture2D getSpritesheet(Item item)
        {
            switch (item)
            {
                case CustomMachine machine:
                    return machine.texture;

                case CustomObject obj:
                    return obj.texture;

                default:
                    return null;
            }
        }

        /// <summary>Get the spritesheet source area for a custom object or machine (if applicable).</summary>
        /// <param name="item">The item instance.</param>
        public Rectangle? getSpriteSourceArea(Item item)
        {
            switch (item)
            {
                case CustomMachine machine:
                    return machine.sourceRectangle;

                case CustomObject obj:
                    return obj.sourceRectangle;

                default:
                    return null;
            }
        }

        public void addContentPack(string folderName, string fileName, IModHelper helper = null, Dictionary<string, string> options = null)
        {
            if (helper == null)
                helper = Helper;

            string baseFolder = CustomFarmingReduxMod.folder;

            if (options != null && options.ContainsKey("baseFolder"))
                baseFolder = options["baseFolder"];

            CustomFarmingPack pack = this.Helper.ContentPacks.CreateFake(Path.Combine(baseFolder, folderName)).ReadJsonFile<CustomFarmingPack>(fileName);

            pack.baseFolder = baseFolder;

            Dictionary<string, string> toCrafting = new Dictionary<string, string>();
            pack.folderName = folderName;
            pack.fileName = fileName;

            if (pack is CustomFarmingPack)
                foreach (CustomMachineBlueprint blueprint in pack.machines)
                {
                    blueprint.pack = pack;
                    blueprint.texture2d = blueprint.getTexture(helper);

                    CustomFarmingReduxMod.machines.AddOrReplace(blueprint);

                    if (blueprint.production != null)
                        foreach (RecipeBlueprint recipe in blueprint.production)
                        {
                            if (recipe.texture != null && recipe.texture != "")
                                recipe.texture2d = recipe.getTexture(helper);

                            recipe.mBlueprint = blueprint;
                        }
                    else if (blueprint.asdisplay)
                    {
                        blueprint.pulsate = false;
                        blueprint.production = new List<RecipeBlueprint>();
                        blueprint.production.Add(new RecipeBlueprint());
                        blueprint.production[0].index = 0;
                        blueprint.production[0].time = 0;
                    }

                    if (blueprint.crafting != null)
                    {
                        toCrafting.AddOrReplace(blueprint.fullid, $"{blueprint.crafting}/Home/130/true/null/{blueprint.fullid}");
                        CustomFarmingReduxMod.craftingrecipes.AddOrReplace(blueprint.fullid, 0);
                    }

                    if (blueprint.forsale && (blueprint.condition == null || PyTK.PyUtils.CheckEventConditions(blueprint.condition)))
                        new InventoryItem(new CustomMachine(blueprint), blueprint.price).addToNPCShop(blueprint.shop);
                }

            if (toCrafting.Count > 0)
                toCrafting.injectInto($"Data/CraftingRecipes");
        }

    }
}
