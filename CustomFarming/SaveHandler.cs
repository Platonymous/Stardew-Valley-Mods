
using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Objects;
using StardewValley.Locations;

using Newtonsoft.Json.Linq;

using CustomElementHandler;

namespace CustomFarming
{
    public class SaveHandler
    {


        public static string saveString = "";
        private static List<ISaveObject> registry = new List<ISaveObject>();

        public SaveHandler()
        {

        }

        private static ISaveObject loadObject(string type, dynamic additionalSaveData)
        {

            Type T = Type.GetType(type);
            ISaveObject newObject = (ISaveObject) Activator.CreateInstance(T);

            newObject.rebuildFromSave(additionalSaveData);

            return newObject;
        }

        public static void LoadAndReplace()
        {
            if (saveString == "")
            {
                return;
            }

            JArray loadJson = JArray.Parse(saveString);


            foreach(dynamic obj in loadJson)
            {
           
                 string type = (string)obj.type;
                dynamic data = (dynamic)obj.data;
                string location = (string)obj.placement.location;
                Vector2 position = (Vector2)obj.placement.position;
                int index = (int)obj.placement.index;
                int building = (int)obj.placement.building;
     
                ISaveObject next = loadObject(type, data);

             
                if (location.Contains("Barn") || location.Contains("Coop") || location.Contains("Shed") || location.Equals("House"))
                {
                    location = "Farm";
                }

                if (location == "Inventory")
                {

                    Game1.player.items[index] = (Item) next;
                    continue;
                }

                if (location == "Fridge")
                {
        
                    (Game1.getLocationFromName("FarmHouse") as FarmHouse).fridge.items[index] = (Item)next;
            
                    continue;
                }

                GameLocation g = Game1.getLocationFromName(location);
                GameLocation place;

                if (building >= 0)
                {
                    place = (g as BuildableGameLocation).buildings[building].indoors;
                }
                else
                {
                    place = g;
                }



                if (index >= 0)
                {

                    (place.objects[position] as Chest).items[index] = (Item) next;
                    continue;
                }

               

                place.objects[position] = (StardewValley.Object)next;

            }


        }

        public static void loadFromFile(string modSaveName)
        {
            saveString = loadSavStringFromFile(modSaveName, Game1.uniqueIDForThisGame, Game1.player.name);
            if(saveString != "")
            {
                string filename = modSaveName + "_" + Game1.player.name + "_" + Game1.uniqueIDForThisGame + ".sav";
                string str = Game1.player.name;
                foreach (char c in str)
                {
                    if (!char.IsLetterOrDigit(c))
                        str = str.Replace(c.ToString() ?? "", "");
                }
                string path2 = Path.Combine(str + "_" + (object)Game1.uniqueIDForThisGame, filename);
                string path = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves"), path2);
                File.Copy(path, path+".old");
                File.Delete(path);
            }
        }

        private static string loadSavStringFromFile(string name, ulong GID, string PN)
        {
            if (!(doesSavFileExist(name, GID, PN)))
            {
                return "";
            }
            string filename = name + "_" + PN + "_" + GID + ".sav";
            FileInfo fi = ensureFolderStructureExists(PN, GID, filename);

            using (StreamReader sr = fi.OpenText())
            {
                return sr.ReadToEnd();

            }
        }

       


        private static FileInfo ensureFolderStructureExists(string PN, ulong GID, string tmpString)
        {
            string str = PN;
            foreach (char c in str)
            {
                if (!char.IsLetterOrDigit(c))
                    str = str.Replace(c.ToString() ?? "", "");
            }
            string path2 = Path.Combine(str + "_" + (object)GID, tmpString);
            FileInfo fileInfo1 = new FileInfo(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves"), path2));
            if (!fileInfo1.Directory.Exists)
                fileInfo1.Directory.Create();

            return fileInfo1;
        }

        private static bool doesSavFileExist(string name, ulong GID, string PN)
        {
            string filename = name + "_" + PN + "_" + GID + ".sav";
            string str = PN;
            foreach (char c in str)
            {
                if (!char.IsLetterOrDigit(c))
                    str = str.Replace(c.ToString() ?? "", "");
            }
            string path2 = Path.Combine(str + "_" + (object)GID, filename);
            FileInfo fileInfo1 = new FileInfo(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves"), path2));
            if (!fileInfo1.Exists)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void register(ISaveObject obj)
        {
            registry.Add(obj);
            obj.Environment = Game1.currentLocation;
            obj.InStorage = true;
        }

        public static void draw(ISaveObject obj, Vector2 tilelocation)
        {
            obj.Environment = Game1.currentLocation;
            obj.InStorage = false;
            obj.Position = tilelocation;
        }

        public static void drawInMenu(ISaveObject obj)
        {
            obj.Environment = Game1.currentLocation;
            obj.InStorage = true;
        }

        private static int getBuilding(ISaveObject obj)
        {
            if (!obj.Environment.isStructure) { return -1; }
            return Game1.getFarm().buildings.FindIndex(x => x.indoors == obj.Environment);
        }

        private static Vector3 inChest(ISaveObject obj)
        {
            if (!obj.InStorage) { return new Vector3(-1); }
            foreach (Vector2 keyV in obj.Environment.objects.Keys)
            {
                if ((obj.Environment.objects[keyV] is Chest) && (obj.Environment.objects[keyV] as Chest).items.Contains((Item)obj))
                {

                    return new Vector3((obj.Environment.objects[keyV] as Chest).items.FindIndex(x => x == obj), keyV.X, keyV.Y);

                }
            }

            return new Vector3(-1); ;
        }

        private static int inFridge(ISaveObject obj)
        {
            if (!obj.InStorage) { return -1; }

            return (Game1.getLocationFromName("FarmHouse") as FarmHouse).fridge.items.FindIndex(x => x == obj);
        }

        private static int inInventory(ISaveObject obj)
        {
            if (!obj.InStorage) { return -1; }

            return Game1.player.items.FindIndex(x => x == obj);
        }




    }
}
