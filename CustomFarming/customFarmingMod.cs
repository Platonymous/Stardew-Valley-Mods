using System;
using System.IO;
using System.Collections.Generic;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;

using Newtonsoft.Json.Linq;
using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace CustomFarming
{
    public class customFarmingMod : Mod
    {
        public string customContentFolder;
        public List<string> customFiles = new List<string>();
        public List<Item> customItems = new List<Item>();
        public List<CustomRecipe> recipes = new List<CustomRecipe>();
        public string key;

        public override void Entry(IModHelper helper)
        {
            this.customContentFolder = Path.Combine(helper.DirectoryPath, "CustomContent");

            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            SaveEvents.BeforeSave += (x,y) => save();
            SaveEvents.AfterSave += (x, y) => load();
            SaveEvents.AfterLoad += (x, y) => load();
           
        }

        private void save()
        {
            SaveHandler.SaveAndRemove();
            Monitor.Log("Saving: " + SaveHandler.saveString);
            SaveHandler.saveToFile("CustomFarmingMod");
        }

        private void load()
        {
            ParseDir(customContentFolder);
            SaveHandler.loadFromFile("CustomFarmingMod");
            Monitor.Log("Loading: " + SaveHandler.saveString);
            SaveHandler.LoadAndReplace();
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            string key = e.KeyPressed.ToString();

            if ( key == "V" && this.key == "V")
            {

                showMachineList();

            }

            this.key = e.KeyPressed.ToString();

        }

        
        public void showMachineList()
        {
            buildMaschines();
            Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, 0, 0);
            Game1.activeClickableMenu = (IClickableMenu)new CustomCraftingPage((int)centeringOnScreen.X, (int)centeringOnScreen.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, recipes, true);


        }

        public void buildMaschines()
        {
            recipes = new List<CustomRecipe>();
            customItems = new List<Item>();

            foreach (string file in customFiles)
            {
                Monitor.Log("Adding:" + file);

                dynamic loadJson = JObject.Parse(File.ReadAllText(file));
                string type = (string)loadJson.Type;
                string name = (string)loadJson.Name;
                
                string filename = Path.GetFileName(file);
                string modFolder = Path.GetDirectoryName(file);
                string crafting = (string)loadJson.CraftingString;
               
                Type T = Type.GetType(type);

                ICustomFarmingObject newMachine = (ICustomFarmingObject)Activator.CreateInstance(T);
                newMachine.build(modFolder, filename);

                recipes.Add(new CustomRecipe(newMachine));

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

        private void addObjectInformation()
        {

        }

    }
}
