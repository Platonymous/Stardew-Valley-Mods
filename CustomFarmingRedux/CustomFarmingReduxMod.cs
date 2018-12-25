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
using PyTK.Lua;

namespace CustomFarmingRedux
{
    public class CustomFarmingReduxMod : Mod
    {
        public static IModHelper _helper;
        public static IMonitor _monitor;
        public static List<CustomMachineBlueprint> machines = new List<CustomMachineBlueprint>();
        public static Config _config;
        public static bool hasKisekae = false;
        public static IMod kisekae = null;
        internal static Dictionary<string, int> craftingrecipes = new Dictionary<string, int>();
        internal static Dictionary<string, MachineHandler> machineHandlers = new Dictionary<string, MachineHandler>();


        public override void Entry(IModHelper helper)
        {
            _helper = Helper;
            _monitor = Monitor;
            _config = Helper.ReadConfig<Config>();

            hasKisekae = helper.ModRegistry.IsLoaded("Kabigon.kisekae");

            if (hasKisekae)
            {
                var registry = helper.ModRegistry.GetType().GetField("Registry", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(helper.ModRegistry);
                System.Collections.IList list = (System.Collections.IList) registry.GetType().GetField("Mods", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(registry);
                foreach (var m in list)
                {
                    IManifest mmanifest = (IManifest)m.GetType().GetProperty("Manifest").GetValue(m);
                    if (mmanifest.UniqueID == "Kabigon.kisekae")
                    {
                        kisekae = (IMod)m.GetType().GetProperty("Mod").GetValue(m);
                        break;
                    }
                }
            }

            loadPacks();
            helper.Events.Display.MenuChanged += OnMenuChanged;
            helper.Events.GameLoop.SaveLoaded += (s, e) =>
            {
                foreach (var c in craftingrecipes)
                    if (Game1.player.craftingRecipes.ContainsKey(c.Key))
                        Game1.player.craftingRecipes[c.Key] = c.Value;
                    else
                        Game1.player.craftingRecipes.Add(c.Key, c.Value);
            };

            harmonyFix();
            helper.ConsoleCommands.Add("replace_custom_farming", "Triggers Custom Farming Replacement", replaceCustomFarming);

            if(_config.water)
            {
                new CustomObjectData("Platonymous.Water", "Water/1/2/Cooking -7/Water/Plain drinking water./drink/0 0 0 0 0 0 0 0 0 0 0/0", Game1.objectSpriteSheet.getTile(247).setSaturation(0), Color.Aqua, type: typeof(WaterItem));
                ButtonClick.ActionButton.onClick((pos) => clickedOnWateringCan(pos), (p) => convertWater());
            }

            PyLua.registerType(typeof(CustomMachine),registerAssembly:true);
        }

        private bool clickedOnWateringCan(Point pos)
        {
            if (Game1.activeClickableMenu is GameMenu g && g.currentTab == 0 && Game1.player.Items.ToList().Exists(i => i is WateringCan))
            {
                List<IClickableMenu> pages = _helper.Reflection.GetField<List<IClickableMenu>>(g, "pages").GetValue();
                if (pages.Find(p => p is InventoryPage) is InventoryPage ip && ip.inventory.inventory.Exists(c => int.Parse(c.name) is int i && i > 0 && i < ip.inventory.actualInventory.Count &&  ip.inventory.actualInventory[i] is WateringCan && c.containsPoint(pos.X, pos.Y)))
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

        private void harmonyFix()
        {
            typeof(SObjectBAI).PatchType(typeof(SObject), Helper);
            typeof(SObjectBAI).PatchType(typeof(ColoredObject), Helper);
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is GameMenu activeMenu && Helper.Reflection.GetField<List<IClickableMenu>>(activeMenu, "pages").GetValue().Find(p => p is CraftingPage) is CraftingPage craftingPage)
            {
                foreach (CustomMachineBlueprint blueprint in machines.Where(m => m.crafting != null))
                {
                    foreach (var page in craftingPage.pagesOfCraftingRecipes)
                    {
                        if (page.Find(k => k.Value.name == blueprint.fullid) is KeyValuePair<ClickableTextureComponent, CraftingRecipe> kv && kv.Value != null && kv.Key != null)
                        {
                            kv.Key.texture = blueprint.getTexture();
                            kv.Key.sourceRect = Game1.getSourceRectForStandardTileSheet(kv.Key.texture, blueprint.tileindex, blueprint.tilewidth, blueprint.tileheight);
                            kv.Value.DisplayName = blueprint.name;
                            Helper.Reflection.GetField<string>(kv.Value, "description").SetValue(blueprint.description);
                        }
                    }
                }
            }
        }

        private List<CustomFarmingPack> loadContentPacks()
        {
            List<CustomFarmingPack> packs = new List<CustomFarmingPack>();

            foreach (StardewModdingAPI.IContentPack pack in Helper.ContentPacks.GetOwned())
            {
                List<CustomFarmingPack> cpPacks = loadCP(pack);
                packs.AddRange(cpPacks);
            }

            return packs;
        }

        public List<CustomFarmingPack> loadCP(StardewModdingAPI.IContentPack contentPack, SearchOption option = SearchOption.TopDirectoryOnly,  string filesearch = "*.json")
        {
            List<CustomFarmingPack> packs = new List<CustomFarmingPack>();
            string[] files = Directory.GetFiles(contentPack.DirectoryPath, filesearch, option);
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                DirectoryInfo directoryInfo = fileInfo.Directory;
                string filename = fileInfo.Name;
                if (filename == "manifest.json")
                    continue;

                CustomFarmingPack pack = contentPack.ReadJsonFile<CustomFarmingPack>(filename);

                pack.fileName = filename;
                pack.folderName = directoryInfo.Name;
                pack.author = contentPack.Manifest.Author;
                pack.version = contentPack.Manifest.Version.ToString();
                pack.baseFolder = "ContentPack";
                pack.contentPack = contentPack;
                packs.Add(pack);
            }

            return packs;
        }

        private void loadPacks()
        {
            List<CustomFarmingPack> packs = new List<CustomFarmingPack>();

            machines = new List<CustomMachineBlueprint>();
            Dictionary<string, string> toCrafting = new Dictionary<string, string>();
            packs.AddRange(loadContentPacks());

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

                    if (blueprint.forsale)
                        new InventoryItem(new CustomMachine(blueprint), blueprint.price).addToNPCShop(blueprint.shop, blueprint.condition);
                    Monitor.Log("Added:" + blueprint.fullid);
                }
            
            Monitor.Log(packs.Count + " Content Packs with " + machines.Count + " machines found.", LogLevel.Trace);
        }
    }
}
