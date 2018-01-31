using System;
using System.IO;
using System.Collections.Generic;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;

using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using System.Linq;

namespace CustomFarming
{
    public class customFarmingMod : Mod
    {
        public string customContentFolder;
        public List<string> customFiles = new List<string>();
        public List<Item> customItems = new List<Item>();
        public string key;
        private static Dictionary<string, ICustomFarmingObject> machinePile = new Dictionary<string, ICustomFarmingObject>();
        private Dictionary<string, simpleMachine> machinesForCrafting = new Dictionary<string, simpleMachine>();

        public override void Entry(IModHelper helper)
        {
            
            customContentFolder = Path.Combine(helper.DirectoryPath, "CustomContent");

            if (!Directory.Exists(customContentFolder))
            {
                Directory.CreateDirectory(customContentFolder);
            }

            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            SaveEvents.BeforeSave += (x,y) => save();
            SaveEvents.AfterSave += (x, y) => load();
            SaveEvents.AfterLoad += (x, y) => load();
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            PlayerEvents.InventoryChanged += PlayerEvents_InventoryChanged;
            GameEvents.FourthUpdateTick += GameEvents_FourthUpdateTick;

            helper.ConsoleCommands.Add("replace_custom_farming", "Triggers Custom Farming Replacement", replaceCustomFarming);

        }

        private void replaceCustomFarming(string action, string[] param)
        {
            if (param[0] == "itemMenu" && Game1.activeClickableMenu is ItemGrabMenu itemMenu)
            {
                List<Item> remove = new List<Item>();
                List<Item> additions = new List<Item>();

                foreach (Item item in itemMenu.ItemsToGrabMenu.actualInventory)
                    if (item is Chest chest && machinePile.Keys.Where(f => f.Equals(item.Name)).Any())
                    {
                        Item cf = (simpleMachine) machinePile[machinePile.Keys.Where(f => f.Equals(item.Name)).FirstOrDefault()];
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
                Dictionary<Item, int[]> items = Helper.Reflection.GetPrivateValue<Dictionary<Item, int[]>>(shop, "itemPriceAndStock");
                List<Item> selling = Helper.Reflection.GetPrivateValue<List<Item>>(shop, "forSale");
                List<Item> remove = new List<Item>();
                List<Item> additions = new List<Item>();

                foreach (Item i in selling)
                    if (i is Chest chest && machinePile.Keys.Where(f => f.Equals(i.Name)).Any())
                    {
                        Item cf = (simpleMachine) machinePile[machinePile.Keys.Where(f => f.Equals(i.Name)).FirstOrDefault()];
                        items.Add(cf, new int[] { chest.preservedParentSheetIndex, int.MaxValue });
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

        private void removeCraftingPage()
        {
            
                
                foreach (string name in machinesForCrafting.Keys)
                {
                    if (Game1.bigCraftablesInformation.ContainsKey(machinesForCrafting[name].craftingid))
                {
                    Game1.bigCraftablesInformation.Remove(machinesForCrafting[name].craftingid);
                }
                    
                    if (CraftingRecipe.craftingRecipes.ContainsKey(name))
                    {
                        CraftingRecipe.craftingRecipes.Remove(name);
                    }

                    if (Game1.player.craftingRecipes.ContainsKey(name))
                    {
                        Game1.player.craftingRecipes.Remove(name);
                    }
                }
            
        }

        private void GameEvents_FourthUpdateTick(object sender, EventArgs e)
        {

           
            if (Game1.activeClickableMenu is GameMenu)
            {
                List<IClickableMenu> gameMenuPages = Helper.Reflection.GetPrivateField<List<IClickableMenu>>(Game1.activeClickableMenu, "pages").GetValue();

                foreach (IClickableMenu menu in gameMenuPages)
                {
                    if (menu is CraftingPage)
                    {
                        
                        Item heldItem = Helper.Reflection.GetPrivateField<Item>(menu, "heldItem").GetValue();

                        if (heldItem != null && heldItem is StardewValley.Object && machinesForCrafting.ContainsKey((heldItem as StardewValley.Object).name))
                        {
                            heldItem = machinesForCrafting[(heldItem as StardewValley.Object).name].getOne();
                            
                            Helper.Reflection.GetPrivateField<Item>(menu, "heldItem").SetValue(heldItem);
                        }


                    }
                }
            }
        }

        private void PlayerEvents_InventoryChanged(object sender, EventArgsInventoryChanged e)
        {
            if (Game1.activeClickableMenu is GameMenu)
            {
                List<Item> inventory = e.Inventory;
                for(int i = 0; i < inventory.Count; i++)
                {
                    if (inventory[i] != null && machinesForCrafting.ContainsKey(inventory[i].Name))
                    {
                        inventory[i] = machinesForCrafting[inventory[i].Name].getOne();
                    }
                }
            }
            


        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (Game1.activeClickableMenu is GameMenu)
            {
               
                GameMenu activeMenu = (GameMenu) Game1.activeClickableMenu;
                List<IClickableMenu> gameMenuPages = Helper.Reflection.GetPrivateField<List<IClickableMenu>>(activeMenu, "pages").GetValue();

               foreach(IClickableMenu menu in gameMenuPages)
                {
                    if (menu is CraftingPage)
                    {
                        List<ClickableTextureComponent> replaceComponents = new List<ClickableTextureComponent>();
                        CraftingPage craftingPage = (CraftingPage)menu;
                        List<Dictionary<ClickableTextureComponent, CraftingRecipe>> pagesOfCraftingRecipes = craftingPage.pagesOfCraftingRecipes;

                        foreach (Dictionary<ClickableTextureComponent, CraftingRecipe> dict in pagesOfCraftingRecipes)
                        {
                            foreach(ClickableTextureComponent ctc in dict.Keys)
                            {

                                if (ctc.sourceRect.X < 0 || ctc.sourceRect.Y < 0)
                                {
                                    ctc.name = dict[ctc].name;
                                    replaceComponents.Add(ctc);

                                }

                            }
                        }

                        for (int c = 0; c < replaceComponents.Count; c++)
                        {
                            if (machinesForCrafting.ContainsKey(replaceComponents[c].name))
                            {
                                replaceComponents[c].texture = machinesForCrafting[replaceComponents[c].name].Texture;
                                replaceComponents[c].sourceRect = machinesForCrafting[replaceComponents[c].name].sourceRectangle;
                                
                            }

                            
                        }
                    }
                }

                Helper.Reflection.GetPrivateField<List<IClickableMenu>>(Game1.activeClickableMenu, "pages").SetValue(gameMenuPages);
        
                
            }
            
        }

        private void save()
        {
            removeCraftingPage();
           
        }

        private void load()
        {            
            customFiles = new List<string>();
            ParseDir(customContentFolder);            
            buildMaschines();
            buildCraftingPage();
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            string key = e.KeyPressed.ToString();

            if (key == "V")
            {

            }

            if (key == "I")
            {


            }

            if (key == "O")
            {

            }



        }
        
        
        public void showMachineList()
        {
            buildMaschines();
            Game1.activeClickableMenu = null;
            Game1.activeClickableMenu = new ItemGrabMenu(this.customItems);
 
        }

        public void buildCraftingPage()
        {
            
            
            int count = -10;
            foreach(Item m in customItems)
            {

                if  (!(m is simpleMachine))
                {
                    continue;
                }

                while (Game1.bigCraftablesInformation.ContainsKey(count))
                {
                    count--;
                }

                string name = m.Name;
                int x = 1;

                while (Game1.player.craftingRecipes.ContainsKey(name))
                {
                    x++;
                    name = m.Name + " " + x;
                }

                (m as simpleMachine).craftingid = count;

                string craftingInformation = $"{name}/50/-300/Crafting -24/{m.getDescription()}/true/true/0/{name}";
                Game1.bigCraftablesInformation.Add(count, craftingInformation);
                CraftingRecipe.craftingRecipes.Add($"{name}", $"{(m as simpleMachine).crafting}/Home/{count.ToString()}/true/{name}");
                Game1.player.craftingRecipes.Add($"{name}", 0);
                if (!machinesForCrafting.ContainsKey(name))
                {
                    (m as simpleMachine).tileindex = (m as simpleMachine).menuTileIndex;
                    (m as simpleMachine).updateSourceRectangle();
                    machinesForCrafting.Add(name, (simpleMachine) m);
                }
                count--;
            }

        }


        public void buildMaschines()
        {
            customItems = new List<Item>();

            foreach (string file in customFiles)
            {
                Monitor.Log("Adding:" + file);

                dynamic loadJson = JObject.Parse(File.ReadAllText(file));
                string type = (string)loadJson.Type;
                string name = (string)loadJson.Name;

                string filename = Path.GetFileName(file);
                string modFolder = Path.GetDirectoryName(file);

                Type T = Type.GetType(type);

                ICustomFarmingObject newMachine = (ICustomFarmingObject)Activator.CreateInstance(T);
                newMachine.build(modFolder, filename);
                string key = new DirectoryInfo(modFolder).Name + "." + new FileInfo(file).Name;
                if (!machinePile.ContainsKey(key))
                    machinePile.Add(key, newMachine);
                customItems.Add((Item)newMachine);
            }
  
        }

     

        private void ParseDir(string path)
        {

            foreach (string dir in Directory.EnumerateDirectories(path))
            {
                ParseDir(Path.Combine(path, dir));
            }
                
            foreach (string file in Directory.EnumerateFiles(path))
            {

                if (Path.GetExtension(file) == ".json")
                {
                    string filePath = Path.Combine(path, Path.GetDirectoryName(file), Path.GetFileName(file));
                    customFiles.Add(filePath);
                    Monitor.Log(filePath);
                }
                  
            }
        }

        

    }
}
