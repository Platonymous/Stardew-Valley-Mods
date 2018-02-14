using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.IO;

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

            string path = Path.Combine(baseFolder, folderName, fileName);
            CustomFarmingPack pack = helper.ReadJsonFile<CustomFarmingPack>(path);

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
                        blueprint.production[0].time = (STime.CURRENT + STime.YEAR * 1000).timestamp;
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
