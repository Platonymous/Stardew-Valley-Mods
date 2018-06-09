using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using System.IO;
using PyTK;
using PyTK.Extensions;
using PyTK.Types;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI.Events;
using System.Reflection;
using PyTK.CustomElementHandler;
using StardewValley.Objects;
using SObject = StardewValley.Object;
using Microsoft.Xna.Framework;
using System;
using StardewValley.Tools;
using Microsoft.Xna.Framework.Input;

namespace CustomFarmingRedux
{
    public class CustomFarmingReduxMod : Mod
    {
        public static IModHelper _helper;
        public static IMonitor _monitor;
        public static List<CustomMachineBlueprint> machines = new List<CustomMachineBlueprint>();
        public static Config _config;
        public static string folder = "Machines";
        public static string legacyFolder = "MachinesCF1";
        internal static Dictionary<string, int> craftingrecipes = new Dictionary<string, int>();


        public override void Entry(IModHelper helper)
        {
            _helper = Helper;
            _monitor = Monitor;
            _config = Helper.ReadConfig<Config>();

            loadPacks();
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            SaveEvents.AfterLoad += (s, e) =>
            {
                foreach (var c in craftingrecipes)
                    if (Game1.player.craftingRecipes.ContainsKey(c.Key))
                        Game1.player.craftingRecipes[c.Key] = c.Value;
                    else
                        Game1.player.craftingRecipes.Add(c.Key, c.Value);
            };

            harmonyFix();
            SaveHandler.addPreprocessor(legacyFix);
            SaveHandler.addReplacementPreprocessor(fixLegacyObject);
            helper.ConsoleCommands.Add("replace_custom_farming", "Triggers Custom Farming Replacement", replaceCustomFarming);

            if(_config.water)
            {
                new CustomObjectData("Platonymous.Water", "Water/1/2/Cooking -7/Water/Plain drinking water./drink/0 0 0 0 0 0 0 0 0 0 0/0", Game1.objectSpriteSheet.getTile(247).setSaturation(0), Color.Aqua, type: typeof(WaterItem));
                ButtonClick.ActionButton.onClick((pos) => clickedOnWateringCan(pos), (p) => convertWater());
            }
        }

        private bool clickedOnWateringCan(Point pos)
        {
            if (Game1.activeClickableMenu is GameMenu g && g.currentTab == 0 && Game1.player.Items.ToList().Exists(i => i is WateringCan))
            {
                List<IClickableMenu> pages = _helper.Reflection.GetField<List<IClickableMenu>>(g, "pages").GetValue();
                if (pages.Find(p => p is InventoryPage) is InventoryPage ip && ip.inventory.inventory.Exists(c => ip.inventory.actualInventory[int.Parse(c.name)] is WateringCan && c.containsPoint(pos.X, pos.Y)))
                    return true;
            }
            return false;
        }

        private void convertWater()
        {
            Item item = Game1.player.Items.ToList().Find(i => i is WateringCan);

            if(item is WateringCan wc)
            {
                List<IClickableMenu> pages = _helper.Reflection.GetField<List<IClickableMenu>>(Game1.activeClickableMenu, "pages").GetValue();
                Item heldItem = _helper.Reflection.GetField<Item>(pages.Find(p => p is InventoryPage), "heldItem").GetValue();
                
          
                if (heldItem is WaterItem s && wc.WaterLeft < wc.waterCanMax)
                {
                    int w = Math.Min(s.Stack, wc.waterCanMax - wc.WaterLeft);
                    s.Stack -= w;
                    if(s.Stack <= 0)
                        _helper.Reflection.GetField<Item>(pages.Find(p => p is InventoryPage), "heldItem").SetValue(null);
                    
                    wc.WaterLeft += w;
                    Game1.playSound("slosh");
                    return;
                }
                else if (wc.WaterLeft > 0 && heldItem == null)
                {
                    int a = Math.Min(10, wc.WaterLeft);
                    wc.WaterLeft -= a;
                    Game1.player.addItemByMenuIfNecessary(new SObject(CustomObjectData.getIndexForId("Platonymous.Water"), a));
                    Game1.playSound("glug");
                }
            }
        }

        public override object GetApi()
        {
            return new CustomFarmingReduxAPI();
        }

        private void replaceCustomFarming(string action, string[] param)
        {
            if (param[0] == "itemMenu" && Game1.activeClickableMenu is ItemGrabMenu itemMenu)
            {
                List<Item> remove = new List<Item>();
                List<Item> additions = new List<Item>();

                foreach (Item item in itemMenu.ItemsToGrabMenu.actualInventory)
                    if (item is Chest chest && machines.Exists(m => m.fullid == chest.name || m.legacy == chest.name))
                    {
                        Item cf = new CustomMachine(machines.Find(m => m.fullid == chest.name || m.legacy == chest.name));
                        additions.Add(cf);
                        remove.Add(item);
                    }

                foreach (Item addition in additions)
                    itemMenu.ItemsToGrabMenu.actualInventory.Add(addition);

                foreach (Item j in remove)
                    itemMenu.ItemsToGrabMenu.actualInventory.Remove(j);
            }

            if (param[0] == "shop" && Game1.activeClickableMenu is ShopMenu shop)
            {
                Dictionary<Item, int[]> items = Helper.Reflection.GetField<Dictionary<Item, int[]>>(shop, "itemPriceAndStock").GetValue();
                List<Item> selling = Helper.Reflection.GetField<List<Item>>(shop, "forSale").GetValue();
                List<Item> remove = new List<Item>();
                List<Item> additions = new List<Item>();

                foreach (Item i in selling)
                    if (i is Chest chest && machines.Exists(m => m.fullid == chest.name || m.legacy == chest.name))
                    {
                        Item cf = new CustomMachine(machines.Find(m => m.fullid == chest.name || m.legacy == chest.name));
                        items.Add(cf, new int[] { chest.preservedParentSheetIndex.Value, int.MaxValue });
                        additions.Add(cf);
                        remove.Add(i);
                    }

                foreach (Item addition in additions)
                    selling.Add(addition);

                foreach (Item j in remove)
                {
                    items.Remove(j);
                    selling.Remove(j);
                }
            }
        }

        private static string legacyFix(string dataString)
        {
            if (dataString.Contains("simpleMachine"))
            {

                string[] data = SaveHandler.splitElemets(dataString);
                Dictionary<string, string> additionalSaveData = new Dictionary<string, string>();

                for (int i = 3; i < data.Length; i++)
                {
                    string[] entry = data[i].Split(SaveHandler.valueSeperator);
                    additionalSaveData.Add(entry[0], entry[1]);
                }

                string id = new DirectoryInfo(additionalSaveData["modfolder"]).Name + "." + additionalSaveData["filename"];
                CustomMachineBlueprint machine = machines.Find(m => m.legacy == id);

                if (machine == null)
                    return dataString;

                dataString = SaveHandler.newPrefix + SaveHandler.seperator + "Object" + SaveHandler.seperator + "CustomFarmingRedux.CustomMachine, CustomFarmingRedux" + SaveHandler.seperator + "id" + SaveHandler.valueSeperator + machine.fullid;
                _monitor.Log("Legacy machine converted: " + dataString, LogLevel.Trace);
                return dataString;
            }
           

            return dataString;
        }

        private static object fixLegacyObject(object replacement)
        {
            if (replacement is Chest c)
            {
                if (c.name.Contains("simpleMachine"))
                {
                    c.items.Clear();
                    return c;
                }

                if (c.name.Contains("customNamedObject"))
                {
                    SObject item = (SObject) c.items[0];
                    SObject obj = new SObject(Vector2.Zero, item.ParentSheetIndex, item.Stack);
                    obj.name = item.name;
                    obj.Quality = item.Quality;
                    return obj;
                }
            }

            return replacement;
        }

        private void harmonyFix()
        {
            typeof(SObjectBAI).PatchType(typeof(SObject), Helper);
            typeof(SObjectBAI).PatchType(typeof(ColoredObject), Helper);
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (Game1.activeClickableMenu is GameMenu activeMenu && Helper.Reflection.GetField<List<IClickableMenu>>(activeMenu, "pages").GetValue().Find(p => p is CraftingPage) is CraftingPage craftingPage)
            {
                foreach (CustomMachineBlueprint blueprint in machines.Where(m => m.crafting != null))
                    for (int i = 0; i < craftingPage.pagesOfCraftingRecipes.Count; i++)
                    {
                        if (craftingPage.pagesOfCraftingRecipes[i].Find(k => k.Value.name == blueprint.fullid) is KeyValuePair<ClickableTextureComponent, CraftingRecipe> kv && kv.Value != null && kv.Key != null)
                        {
                            kv.Key.texture = blueprint.getTexture();
                            kv.Key.sourceRect = Game1.getSourceRectForStandardTileSheet(kv.Key.texture, blueprint.tileindex, blueprint.tilewidth, blueprint.tileheight);
                            kv.Value.DisplayName = blueprint.name;
                            Helper.Reflection.GetField<string>(kv.Value, "description").SetValue(blueprint.description);
                        }
                    }
            }
        }

        private void loadPacks()
        {
            List<CustomFarmingPack> packs = new List<CustomFarmingPack>();

            List<CustomFarmingPack> newPacks = new List<CustomFarmingPack>();
            string machineDir = Path.Combine(Helper.DirectoryPath, folder);
            if (Directory.Exists(machineDir) && new DirectoryInfo(machineDir).GetDirectories().Length > 0)
                PyUtils.loadContentPacks(out newPacks, machineDir, SearchOption.AllDirectories, Monitor);
            machines = new List<CustomMachineBlueprint>();
            Dictionary<string, string> toCrafting = new Dictionary<string, string>();

            List<CustomFarmingPack> legacyPacks = new List<CustomFarmingPack>();
            string legacyDir = Path.Combine(Helper.DirectoryPath, legacyFolder);
            if(Directory.Exists(legacyDir) && new DirectoryInfo(legacyDir).GetDirectories().Length > 0)
                PyUtils.loadContentPacks(out legacyPacks, legacyDir, SearchOption.AllDirectories, Monitor);

            legacyPacks.useAll(l => l.baseFolder = legacyFolder);

            newPacks.AddRange(legacyPacks);

            foreach (CustomFarmingPack lPack in newPacks)
            {
                if (!lPack.legacy)
                {
                    packs.AddOrReplace(lPack);
                    continue;
                }
                    
                string lid = lPack.folderName + "." + lPack.fileName;

                bool exists = false;
                packs.useAll(p => exists = exists || p.machines.Exists(m => m.legacy == lid));

                if (exists)
                {
                    Monitor.Log("Skipped legacy machine " + lid + " because a new version was found.", LogLevel.Trace);
                    continue;
                }
                    
                CustomFarmingPack next = new CustomFarmingPack();
                next.legacy = true;
                next.author = lPack.author;
                next.fileName = lPack.fileName;
                next.folderName = lPack.folderName;
                next.name = lPack.name;
                next.machines = new List<CustomMachineBlueprint>();
                CustomMachineBlueprint legacyMachine = new CustomMachineBlueprint();
                legacyMachine.id = 0;
                legacyMachine.pack = next;
                legacyMachine.category = lPack.CategoryName;
                legacyMachine.description = lPack.Description;
                legacyMachine.name = lPack.Name;
                legacyMachine.frames = lPack.WorkAnimationFrames;
                legacyMachine.pulsate = lPack.WorkAnimationFrames <= 0;
                legacyMachine.readyindex = lPack.ReadyTileIndex;
                legacyMachine.tileindex = lPack.TileIndex;
                legacyMachine.texture = lPack.Tilesheet;
                legacyMachine.fps = 6;
                legacyMachine.showitem = lPack.displayItem;
                legacyMachine.itempos = new int[] { lPack.displayItemX, lPack.displayItemY };
                legacyMachine.itemzoom = lPack.displayItemZoom;
                legacyMachine.crafting = lPack.Crafting;

                if (lPack.StarterMaterial > 0)
                {
                    IngredientBlueprint starter = new IngredientBlueprint();
                    starter.index = lPack.StarterMaterial;
                    starter.stack = lPack.StarterMaterialStack;
                    legacyMachine.starter = starter;
                }

                if (lPack.Produce != null && lPack.Produce.ProduceID <= 0)
                    legacyMachine.asdisplay = true;
                
                if (lPack.Produce != null && lPack.Produce.ProduceID > 0)
                {
                    legacyMachine.production = new List<RecipeBlueprint>();
                    legacyMachine.legacy = lid;
                    Monitor.Log("Legacy:" + legacyMachine.legacy);
                    RecipeBlueprint baseProduce = new RecipeBlueprint();
                    baseProduce.name = lPack.Produce.Name;
                    baseProduce.index = lPack.Produce.ProduceID;
                    baseProduce.colored = lPack.Produce.useColor;
                    baseProduce.prefix = lPack.Produce.usePrefix;
                    baseProduce.suffix = lPack.Produce.useSuffic;
                    baseProduce.texture = lPack.Produce.Tilesheet;
                    baseProduce.tileindex = lPack.TileIndex;
                    baseProduce.time = lPack.Produce.ProductionTime;
                    baseProduce.stack = lPack.Produce.Stack;
                    baseProduce.description = lPack.Produce.Description;
                    baseProduce.quality = lPack.Produce.Quality;
                    List<int> materials = lPack.Materials.ToList();
                    baseProduce.materials = new List<IngredientBlueprint> { new IngredientBlueprint() };
                    baseProduce.materials[0].index = materials[0];
                    baseProduce.materials[0].exactquality = lPack.Produce.MaterialQuality;
                    baseProduce.materials[0].stack = lPack.RequiredStack;

                    if (lPack.SpecialProduce != null)
                    {
                        foreach (LegacySpecialProduce pnext in lPack.SpecialProduce)
                        {
                            RecipeBlueprint nextProduce = new RecipeBlueprint();
                            nextProduce.name = pnext.Name != null ? pnext.Name : baseProduce.name;
                            nextProduce.index = pnext.ProduceID != -1 ? pnext.ProduceID : baseProduce.index;
                            nextProduce.colored = pnext.uc ? pnext._useColor : baseProduce.colored;
                            nextProduce.prefix = pnext.up ? pnext._usePrefix : baseProduce.prefix;
                            nextProduce.suffix = pnext.us ? pnext._useSuffix : baseProduce.suffix;
                            nextProduce.texture = pnext.Tilesheet != null ? pnext.Tilesheet : baseProduce.texture;
                            nextProduce.tileindex = pnext.TileIndex != -1 ? pnext.TileIndex : baseProduce.tileindex;
                            nextProduce.time = pnext.ProductionTime != -1 ? pnext.ProductionTime : baseProduce.time;
                            nextProduce.stack = pnext.Stack != -1 ? pnext.Stack : baseProduce.stack;
                            nextProduce.description = pnext.Description != null ? pnext.Description : baseProduce._description;
                            nextProduce.quality = pnext.Quality != -9 ? pnext.Quality : baseProduce.quality;
                            nextProduce.materials = new List<IngredientBlueprint>() { new IngredientBlueprint() };
                            nextProduce.materials[0].index = pnext.Material;
                            nextProduce.materials[0].exactquality = pnext.MaterialQuality;
                            nextProduce.materials[0].stack = lPack.RequiredStack;
                            materials.Remove(pnext.Material);
                            legacyMachine.production.Add(nextProduce);
                        }
                    }

                    baseProduce.materials[0].index = materials.Count > 0 ? materials[0] : baseProduce.materials[0].index;
                    materials.Remove(baseProduce.materials[0].index);
                    baseProduce.include = materials.Count > 0 ? materials.ToArray() : null;
                    legacyMachine.production.Add(baseProduce);
                }

                next.machines.Add(legacyMachine);
                packs.Add(next);
            }
            
            foreach (CustomFarmingPack pack in packs)
                foreach (CustomMachineBlueprint blueprint in pack.machines)
                {
                    blueprint.pack = pack;
                    machines.AddOrReplace(blueprint);

                    if (blueprint.production != null)
                        foreach (RecipeBlueprint recipe in blueprint.production)
                            recipe.mBlueprint = blueprint;
                    else if (blueprint.asdisplay)
                    {
                        blueprint.pulsate = false;
                        blueprint.production = new List<RecipeBlueprint>();
                        blueprint.production.Add(new RecipeBlueprint());
                        blueprint.production[0].index = 0;
                        blueprint.production[0].time = 0;
                    }

                    CustomObjectData data = new CustomObjectData(blueprint.fullid, $"{blueprint.name}/{blueprint.price}/-300/Crafting -9/{blueprint.description}/true/true/0/{blueprint.name}", blueprint.getTexture(), Color.White, blueprint.tileindex, true, typeof(CustomMachine), (blueprint.crafting == null || blueprint.crafting == "") ? null : new CraftingData(blueprint.fullid, blueprint.crafting));

                    if (blueprint.forsale && (blueprint.condition == null || PyUtils.CheckEventConditions(blueprint.condition)))
                        new InventoryItem(new CustomMachine(blueprint), blueprint.price).addToNPCShop(blueprint.shop);
                }
            
            Monitor.Log(packs.Count + " Content Packs with " + machines.Count + " machines found.", LogLevel.Trace);
        }
    }
}
