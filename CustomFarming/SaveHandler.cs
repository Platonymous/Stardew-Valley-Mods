
using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Objects;
using StardewValley.Locations;

using Newtonsoft.Json.Linq;


namespace CustomFarming
{
    public class SaveHandler
    {


        private static List<ISaveObject> registry = new List<ISaveObject>();
        public static string saveString = "";
      

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
                next.Position = position;
                next.InStorage = false;

                if (location == "Inventory")
                {
                    next.InStorage = true;
                    next.Environment = Game1.getFarm();
                    Game1.player.items[index] = (Item) next;
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

                next.Environment = place;

                if (index >= 0)
                {
                    next.InStorage = true;
                    (place.objects[position] as Chest).items[index] = (Item) next;
                    continue;
                }

                place.objects[position] = (StardewValley.Object)next;

            }


        }

        public static void loadFromFile(string modSaveName)
        {
            saveString = loadSavStringFromFile(modSaveName, Game1.uniqueIDForThisGame, Game1.player.name);
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

        public static void saveToFile(string modSaveName)
        {
            saveSavStringToFile(modSaveName, saveString, Game1.uniqueIDForThisGame, Game1.player.name);
            saveString = "";
        }

        private static void saveSavStringToFile(string name, string savstring, ulong GID, string PN)
        {
            string filename = name + "_" + PN + "_" + GID + ".sav";
            FileInfo fi = ensureFolderStructureExists(PN, GID, filename);


            using (StreamWriter sw = fi.CreateText())
            {
                sw.WriteLine(savstring);
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

        public static void SaveAndRemove()
        {
            List<object> save = new List<object>();

            int c = 0;

            foreach(ISaveObject s in registry)
            {
                if (s != null) { 
                dynamic item = removeObject(s);
                if (item != null)
                {
                    save.Add((object)item);
                    c++;
                }
                }
            }

            object[] saveobjects = save.ToArray();

            saveString = Newtonsoft.Json.JsonConvert.SerializeObject(saveobjects);

            registry = new List<ISaveObject>();
            
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
            if (obj.Environment.GetType().GetMethod("getBuilding") == null) { return -1; }
            return Game1.getFarm().buildings.FindIndex(x => x.indoors == obj.Environment);
        }

        private static Vector3 inChest(ISaveObject obj)
        {
            if (!obj.InStorage) { return new Vector3(-1); }
            foreach (Vector2 keyV in obj.Environment.objects.Keys)
            {
                if ((obj.Environment.objects[keyV] is Chest) && (obj.Environment.objects[keyV] as Chest).items.Contains((Item) obj))
                {

                    return new Vector3((obj.Environment.objects[keyV] as Chest).items.FindIndex(x => x == obj), keyV.X, keyV.Y);
                    
                }
            }

            return new Vector3(-1); ;
        }

        private static int inInventory(ISaveObject obj)
        {
            if (!obj.InStorage) { return -1; }

            return Game1.player.items.FindIndex(x => x == obj);
        }


        private static dynamic getPlacementSaveData(ISaveObject obj)
        {
            int pIndex = inInventory(obj);
            string eName = obj.Environment.name;
            Vector2 oPosition = obj.Position;

            if (pIndex >= 0)
            {
                eName = "Inventory";
            }
            else
            {
                Vector3 chest = inChest(obj);
                pIndex = (int) chest.X;
                if (pIndex >= 0)
                {
                    oPosition = new Vector2(chest.Y, chest.Z);
                }
            }

            dynamic placement = new { position = oPosition, building = getBuilding(obj),  location = eName, index = pIndex };
            return placement;
        }



        private static dynamic removeObject(ISaveObject obj)
        {
            dynamic placementSaveData = getPlacementSaveData(obj);
            dynamic additionalSaveData = obj.getAdditionalSaveData();

            int index = (int)placementSaveData.index;
            string loaction = (string)placementSaveData.location;
            Vector2 oPosition = (Vector2)placementSaveData.position;

            if (loaction == "Inventory" && Game1.player.items[index] == obj)
            {
                Game1.player.items[index] = obj.getReplacement();
            }else if (index >= 0 && obj.Environment.objects.ContainsKey(oPosition) && obj.Environment.objects[oPosition] is Chest && (obj.Environment.objects[oPosition] as Chest).items[index] == obj)
            {
                (obj.Environment.objects[oPosition] as Chest).items[index] = obj.getReplacement();
            } else if (obj.Environment.objects.ContainsKey(oPosition) && obj.Environment.objects[oPosition] == obj)
            {
                obj.Environment.objects[oPosition] = obj.getReplacement();
            } else
            {
                return null;
            }

            string type = obj.GetType().FullName;

            dynamic saveObject = new { type = type, placement = placementSaveData, data = additionalSaveData };

            return saveObject;

        }

    }
}
