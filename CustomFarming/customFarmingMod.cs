using System;
using System.IO;
using System.Collections.Generic;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;

using Newtonsoft.Json.Linq;

namespace CustomFarming
{
    public class customFarmingMod : Mod
    {
        public string customContentFolder;
        public List<string> customFiles = new List<string>();
        public List<Item> customItems = new List<Item>();
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
            customFiles = new List<string>();
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

        private void addObjectInformation()
        {

        }

    }
}
