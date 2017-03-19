using System;
using System.IO;
using System.Collections.Generic;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;

using Newtonsoft.Json.Linq;
using Microsoft.Xna.Framework;

namespace CustomFarming
{
    public class customFarmingMod : Mod
    {
        public string customContentFolder;
        public List<string> customFiles = new List<string>();
        public List<Item> customItems = new List<Item>();
        public string key;
        private Dictionary<string, simpleMachine> machinesForCrafting = new Dictionary<string, simpleMachine>();

        public override void Entry(IModHelper helper)
        {
            this.customContentFolder = Path.Combine(helper.DirectoryPath, "CustomContent");

            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            SaveEvents.BeforeSave += (x,y) => save();
            SaveEvents.AfterSave += (x, y) => load();
            SaveEvents.AfterLoad += (x, y) => load();
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            PlayerEvents.InventoryChanged += PlayerEvents_InventoryChanged;
            GameEvents.FourthUpdateTick += GameEvents_FourthUpdateTick;
           
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

                        if (heldItem != null && machinesForCrafting.ContainsKey(heldItem.Name))
                        {
                            heldItem = machinesForCrafting[heldItem.Name].getOne();

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

                        foreach(Dictionary<ClickableTextureComponent, CraftingRecipe> dict in (menu as CraftingPage).pagesOfCraftingRecipes)
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
            SaveHandler.SaveAndRemove();
            Monitor.Log("Saving: " + SaveHandler.saveString);
            SaveHandler.saveToFile("CustomFarmingMod");
        }

        private void load()
        {
            customFiles = new List<string>();
            ParseDir(customContentFolder);
            SaveHandler.loadFromFile("CustomFarmingMod");
            Monitor.Log("Loading: " + SaveHandler.saveString);
            SaveHandler.LoadAndReplace();
            buildMaschines();
            buildCraftingPage();
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            string key = e.KeyPressed.ToString();

            if ( key == "V" && this.key == "V")
            {

                //showMachineList();

            }

            if (key == "I")
            {

                
            }

            if (key == "O")
            {
               
            }

            this.key = e.KeyPressed.ToString();

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

                string craftingInformation = $"{name}/50/-300/Crafting -24/{m.getDescription()}/true/true/0";
                Game1.bigCraftablesInformation.Add(count, craftingInformation);
                CraftingRecipe.craftingRecipes.Add($"{name}", $"{(m as simpleMachine).crafting}/Home/{count.ToString()}/true/null/{name}/simpleMachine/{name}");
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
