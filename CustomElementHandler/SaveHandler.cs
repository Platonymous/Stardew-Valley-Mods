using System;
using System.Collections.Generic;
using System.Linq;

using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using StardewValley.Locations;
using StardewValley.Buildings;
using StardewModdingAPI;

namespace CustomElementHandler
{

    public class SaveHandler
    {
        
        internal static IMonitor Monitor;

        private static List<object> objects;
        private static List<object> storage;
        private static List<object> attachements;
        private static List<object> animals;
        private static List<object> characters;
        private static List<object> elements;

        public static event EventHandler FinishedRebuilding;
        public static event EventHandler BeforeRebuilding;
        public static event EventHandler BeforeRemoving;
        public static event EventHandler FinishedRemoving;

        public SaveHandler() {

        }

        private static void OnFinishedRebuilding(EventArgs e)
        {
           
            FinishedRebuilding?.Invoke(null, e);

        }

        private static void OnBeforeRebuilding(EventArgs e)
        {
            BeforeRebuilding?.Invoke(null, e);

        }


        private static void OnBeforeRemoving(EventArgs e)
        {
            BeforeRemoving?.Invoke(null, e);

        }

        private static void OnFinishedRemoving(EventArgs e)
        {
            FinishedRemoving?.Invoke(null, e);

        }


        private static void findElements()
        {
          
            storage = new List<object>();
            objects = new List<object>();
            attachements = new List<object>();
            animals = new List<object>();
            characters = new List<object>();

            findObjects();
            findStorage();
            findAttachements();

        }

        private static object rebuildElement(string[] data, object replacement)
        {
           
            Type T = Type.GetType(data[2]);
            if (T == null)
            {
                return replacement;
            }

            ISaveElement newElement = (ISaveElement)Activator.CreateInstance(T);

            Dictionary<string, string> additionalSaveData = new Dictionary<string, string>();

           for (int i = 3; i < data.Length; i++)
            {
                string[] entry = data[i].Split('=');
                additionalSaveData.Add(entry[0], entry[1]);
            }

            newElement.rebuild(additionalSaveData,replacement);

            return newElement;
        }

        internal static void placeElements()
        {

            OnBeforeRebuilding(EventArgs.Empty);

            findElements();

            elements = new List<object>();
            elements.AddRange(characters);
            elements.AddRange(animals);
            elements.AddRange(objects);
            elements.AddRange(storage);
            elements.AddRange(attachements);

            if (Game1.player.hat != null && Game1.player.hat.name.Contains("CEHe"))
            {
                string name = Game1.player.hat.name;
                string[] data = name.Split('/');

                object replacement = rebuildElement(data, Game1.player.hat);

                Game1.player.hat = (Hat)replacement;

            }
            if (Game1.player.boots != null && Game1.player.boots.name.Contains("CEHe"))
            {
                string name = Game1.player.boots.name;
                string[] data = name.Split('/');

                object replacement = rebuildElement(data, Game1.player.boots);
                Game1.player.boots = (Boots)replacement;

            }

            if (Game1.player.leftRing != null && Game1.player.leftRing.name.Contains("CEHe"))
            {
                string name = Game1.player.leftRing.name;
                string[] data = name.Split('/');

                object replacement = rebuildElement(data, Game1.player.leftRing);
                Game1.player.leftRing = (Ring)replacement;

            }

            if (Game1.player.rightRing != null && Game1.player.rightRing.name.Contains("CEHe"))
            {
                string name = Game1.player.rightRing.name;
                string[] data = name.Split('/');

                object replacement = rebuildElement(data, Game1.player.rightRing);
                Game1.player.rightRing = (Ring)replacement;

            }

            for (int i = 0; i < elements.Count; i++)
            {

                if (elements[i] is List<Item>)
                {
                    List<Item> list = (List<Item>)elements[i];
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j] != null && list[j].Name.Contains("CEHe"))
                        {
                            string name = list[j].Name;
                            if (list[j] is StardewValley.Object)
                            {
                                name = (list[j] as StardewValley.Object).name;
                            }

                            if (list[j] is StardewValley.Tool)
                            {
                                name = (list[j] as StardewValley.Tool).name;
                            }

                            string[] data = name.Split('/');
                            
                            object replacement = rebuildElement(data, list[j]);
                            list[j] = (Item) replacement;
                            
                        }
                    }
                }
                else if (elements[i] is SerializableDictionary<Vector2, StardewValley.Object>)
                {
                    SerializableDictionary<Vector2, StardewValley.Object> changes = new SerializableDictionary<Vector2, StardewValley.Object>();
                    SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature> terrainChanges = new SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature>();
                    SerializableDictionary<Vector2, StardewValley.Object> dict = (SerializableDictionary<Vector2, StardewValley.Object>)elements[i];
                    SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature> terrainDict = new SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature>();
                    if (elements[i+1] is SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature>)
                    {
                        terrainDict = (SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature>)elements[i + 1];
                    }

                    foreach (Vector2 keyV in dict.Keys)
                    {
                        if (dict[keyV].Name.Contains("CEHe"))
                        {
                            string[] preData = dict[keyV].Name.Split('#');
                            string[] data = preData[0].Split('/');

                            if (data[1] != "Terrain") {
                            object replacement = rebuildElement(data, dict[keyV]);
                            changes.Add(keyV,(StardewValley.Object) replacement);
                            }
                            else
                            {
                                object replacement = rebuildElement(data, dict[keyV]);
                                terrainChanges.Add(keyV, (StardewValley.TerrainFeatures.TerrainFeature)replacement);

                                if (preData.Length == 1)
                                {
                                    changes.Add(keyV, new StardewValley.Object(-999, 1));
                                }
                                else
                                {
                                    
                                    if (!preData.Contains("CEHe")){
                                        changes[keyV] = dict[keyV];
                                        changes[keyV].name = preData[1];
                                    }
                                    else
                                    {
                          
                                        string[] data2 = preData[1].Split('/');
                                        object replacement2 = rebuildElement(data2, dict[keyV]);
                                        changes.Add(keyV, (StardewValley.Object)replacement2);
                                    }

                                }
                            }

                        }

                    }

                    foreach (Vector2 keyV in changes.Keys)
                    {
                        if (changes[keyV].parentSheetIndex != -999)
                        {
                            dict[keyV] = changes[keyV];
                        }
                        else
                        {
                            dict.Remove(keyV);
                        }
                    }

                    foreach (Vector2 keyV in terrainChanges.Keys)
                    {
                        terrainDict[keyV] = terrainChanges[keyV];  
                    }

                }else if (elements[i] is StardewValley.Object[])
                {
                    StardewValley.Object[] list = (StardewValley.Object[])elements[i];
                    for (int j = 0; j < list.Length; j++)
                    {
                        if (list[j] != null && list[j].name.Contains("CEHe"))
                        {
                            string[] data = list[j].name.Split('/');

                            object replacement = rebuildElement(data, list[j]);
                            list[j] = (StardewValley.Object)replacement;
                        }
                    }
                }
                else if (elements[i] is SerializableDictionary<long, FarmAnimal>)
                {
                    SerializableDictionary<long, FarmAnimal> changes = new SerializableDictionary<long, FarmAnimal>();
                    SerializableDictionary<long, FarmAnimal> dict = (SerializableDictionary<long, FarmAnimal>)elements[i];

                    foreach (long keyL in dict.Keys)
                    {
                        if (dict[keyL].name.Contains("CEHe"))
                        {
                            string[] data = dict[keyL].name.Split('/');

                            object replacement = rebuildElement(data, dict[keyL]);
                            changes[keyL] = (FarmAnimal)replacement;
                        }

                    }

                    foreach (long keyL in changes.Keys)
                    {
                        dict[keyL] = changes[keyL];

                    }
                }
                else if (elements[i] is List<NPC>)
                {
                    List<NPC> list = (List<NPC>)elements[i];
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j] != null && list[j].name.Contains("CEHe"))
                        {
                            string[] data = list[j].name.Split('/');

                            object replacement = rebuildElement(data, list[j]);
                            list[j] = (NPC)replacement;
                        }
                    }
                }
                else if (elements[i] is List<Furniture>)
                {
                    List<Furniture> list = (List<Furniture>)elements[i];
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j] != null && list[j].name.Contains("CEHe"))
                        {
                            string[] data = list[j].name.Split('/');
                            
                            object replacement = rebuildElement(data, list[j]);
                            list[j] = (Furniture)replacement;
                        }
                    }
                }

            }

            OnFinishedRebuilding(EventArgs.Empty);
        }

        private static string getTypeName(object o)
        {
                return o.GetType().AssemblyQualifiedName;
        }

        internal static void removeElements()
        {
            OnBeforeRemoving(EventArgs.Empty);

            findElements();

            elements = new List<object>();
            elements.AddRange(attachements);
            elements.AddRange(storage);
            elements.AddRange(objects);
            elements.AddRange(animals);
            elements.AddRange(characters);

           if (Game1.player.hat is ISaveElement)
           {
               ISaveElement element = (ISaveElement)Game1.player.hat;
               string additionalSaveData = string.Join("/", element.getAdditionalSaveData().Select(x => x.Key + "=" + x.Value));
               string type = getTypeName(element);
               
               string name = "CEHe/Item/" + type + "/" + additionalSaveData;
                Hat replacement = (Hat) element.getReplacement();
               replacement.name = name;

               Game1.player.hat = (Hat) replacement;

           }


            if (Game1.player.boots is ISaveElement)
            {
                ISaveElement element = (ISaveElement)Game1.player.boots;
                string additionalSaveData = string.Join("/", element.getAdditionalSaveData().Select(x => x.Key + "=" + x.Value));
                string type = getTypeName(element);

                string name = "CEHe/Item/" + type + "/" + additionalSaveData;
                 Boots replacement = (Boots) element.getReplacement();
                replacement.name = name;

                Game1.player.boots = (Boots)replacement;

            }

            if (Game1.player.leftRing is ISaveElement)
            {
                ISaveElement element = (ISaveElement)Game1.player.leftRing;
                string additionalSaveData = string.Join("/", element.getAdditionalSaveData().Select(x => x.Key + "=" + x.Value));
                string type = getTypeName(element);

                string name = "CEHe/Item/" + type + "/" + additionalSaveData;
                 Ring replacement = (Ring) element.getReplacement();
                replacement.name = name;

                Game1.player.leftRing = (Ring)replacement;

            }

            if (Game1.player.rightRing is ISaveElement)
            {
                ISaveElement element = (ISaveElement)Game1.player.rightRing;
                string additionalSaveData = string.Join("/", element.getAdditionalSaveData().Select(x => x.Key + "=" + x.Value));
                string type = getTypeName(element);

                string name = "CEHe/Item/" + type + "/" + additionalSaveData;
                 Ring replacement = (Ring) element.getReplacement();
                replacement.name = name;

                Game1.player.rightRing = (Ring)replacement;

            }


            for (int i = 0; i < elements.Count; i++)
            {

                if (elements[i] is List<Item>)
                {
                    List<Item> list = (List<Item>) elements[i];
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j] is ISaveElement)
                        {
                            ISaveElement element = (ISaveElement)list[j];
                            string additionalSaveData = string.Join("/", element.getAdditionalSaveData().Select(x => x.Key + "=" + x.Value));
                            string type = getTypeName(element);
                            string name = "CEHe/Item/" + type + "/" + additionalSaveData;
                            Item replacement = (Item) element.getReplacement();
                            
                            if(replacement is StardewValley.Object obj)
                            {
                                obj.name = name;
                                list[j] = obj;
                            }
                            else if (replacement is Tool tool)
                            {
                                tool.name = name;
                                list[j] = tool;
                                
                            }
                            else
                            {
                                list[j] = replacement;
                            }

                        }
                    }
                }
                else if (elements[i] is List<Furniture>)
                {
                    List<Furniture> list = (List<Furniture>)elements[i];
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j] is ISaveElement)
                        {
                            ISaveElement element = (ISaveElement)list[j];
                            string additionalSaveData = string.Join("/", element.getAdditionalSaveData().Select(x => x.Key + "=" + x.Value));
                            string type = getTypeName(element);
                            string name = "CEHe/Item/" + type + "/" + additionalSaveData;
                            StardewValley.Object replacement = (StardewValley.Object) element.getReplacement();
                            replacement.name = name;


                            list[j] = (Furniture)replacement;


                        }
                    }
                }
                else if (elements[i] is SerializableDictionary<Vector2,StardewValley.Object>)
                {
                    SerializableDictionary<Vector2, StardewValley.Object> changes = new SerializableDictionary<Vector2, StardewValley.Object>();
                    SerializableDictionary<Vector2, StardewValley.Object> dict = (SerializableDictionary<Vector2, StardewValley.Object>)elements[i];

                    foreach (Vector2 keyV in dict.Keys)
                    {
                        if (dict[keyV] is ISaveElement)
                        {
                            ISaveElement element = (ISaveElement)dict[keyV];
                            string additionalSaveData = string.Join("/", element.getAdditionalSaveData().Select(x => x.Key + "=" + x.Value));
                            string type = getTypeName(element);
                            string name = "CEHe/Object/" + type + "/" + additionalSaveData;
                            StardewValley.Object replacement = (StardewValley.Object) element.getReplacement();
                            replacement.name = name;
                            changes.Add(keyV, (StardewValley.Object) replacement);
                        }

                    }

                    foreach(Vector2 keyV in changes.Keys)
                    {
                        dict[keyV] = changes[keyV];
                    } 

                }
                else if (elements[i] is SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature>)
                {
                    SerializableDictionary<Vector2, StardewValley.Object> changes = new SerializableDictionary<Vector2, StardewValley.Object>();
                    SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature> dict = (SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature>)elements[i];

                    foreach (Vector2 keyV in dict.Keys)
                    {
                        if (dict[keyV] is ISaveElement)
                        {
                            ISaveElement element = (ISaveElement)dict[keyV];
                            string additionalSaveData = string.Join("/", element.getAdditionalSaveData().Select(x => x.Key + "=" + x.Value));
                            string type = getTypeName(element);
                            string name = "CEHe/Terrain/"+ type + "/" + additionalSaveData;
                            StardewValley.Object replacement = (StardewValley.Object) element.getReplacement();
                            replacement.name = name;
                            changes.Add(keyV, (StardewValley.Object)replacement);
                        }

                    }

                    SerializableDictionary<Vector2, StardewValley.Object> objectLayer = (SerializableDictionary<Vector2, StardewValley.Object>)elements[i - 1];

                    foreach (Vector2 keyV in changes.Keys)
                    {
                        dict[keyV] = new StardewValley.TerrainFeatures.Flooring(0);
                        if (objectLayer.ContainsKey(keyV))
                        {

                            objectLayer[keyV].name = changes[keyV].name + "#"+objectLayer[keyV].name;
                        }
                        else
                        {
                            objectLayer[keyV] = changes[keyV];
                        }
                        }

                }else if (elements[i] is SerializableDictionary<long, FarmAnimal>)
                {
                    SerializableDictionary<long, FarmAnimal> changes = new SerializableDictionary<long, FarmAnimal>();
                    SerializableDictionary<long, FarmAnimal> dict = (SerializableDictionary<long, FarmAnimal>)elements[i];

                    foreach (long keyL in dict.Keys)
                    {
                        if (dict[keyL] is ISaveElement)
                        {
                            ISaveElement element = (ISaveElement)dict[keyL];
                            string additionalSaveData = string.Join("/", element.getAdditionalSaveData().Select(x => x.Key + "=" + x.Value));
                            string type = getTypeName(element);
                            string name = "CEHe/Animal/" + type + "/" + additionalSaveData;
                            FarmAnimal replacement = (FarmAnimal) element.getReplacement();
                            replacement.name = name;

                            changes.Add(keyL, (FarmAnimal)replacement);
                        }

                    }

                    foreach (long keyL in changes.Keys)
                    {
                        dict[keyL] = changes[keyL];

                    }
                }else if (elements[i] is List<NPC>)
                {
                    List<NPC> list = (List<NPC>) elements[i];
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j] is ISaveElement)
                        {
                            ISaveElement element = (ISaveElement)list[j];
                            string additionalSaveData = string.Join("/", element.getAdditionalSaveData().Select(x => x.Key + "=" + x.Value));
                            string type = getTypeName(element);
                            string name = "CEHe/NPC/" + type + "/" + additionalSaveData;
                            NPC replacement = (NPC) element.getReplacement();
                            replacement.name = name;
                            list[j] = (NPC) replacement;
                        }
                    }
                }
                else if (elements[i] is StardewValley.Object[])
                {
                    StardewValley.Object[] list = (StardewValley.Object[])elements[i];
                    for (int j = 0; j < list.Length; j++)
                    {
                        if (list[j] is ISaveElement)
                        {
                            ISaveElement element = (ISaveElement)list[j];
                            string additionalSaveData = string.Join("/", element.getAdditionalSaveData().Select(x => x.Key + "=" + x.Value));
                            string type = getTypeName(element);
                            string name = "CEHe/Attachement/" + type + "/" + additionalSaveData;
                            StardewValley.Object replacement = (StardewValley.Object) element.getReplacement();
                            replacement.name = name;
                            list[j] = (StardewValley.Object)replacement;
                        }
                    }
                }

            }
            OnFinishedRemoving(EventArgs.Empty);
            
        }


        private static void findObjects()
        {
            foreach(GameLocation location in Game1.locations)
            {
                objects.Add(location.objects);
                objects.Add(location.terrainFeatures);

                characters.Add(location.characters);

                if (location is DecoratableLocation dl)
                {
                    objects.Add(dl.furniture);
                }

                if (location is BuildableGameLocation)
                {
                    if (location is Farm)
                    {
                        animals.Add((location as Farm).animals);
                    }

                    foreach (Building building in (location as BuildableGameLocation).buildings)
                    {
                        if(building.indoors != null)
                        {
                            objects.Add(building.indoors.objects);
                            objects.Add(building.indoors.terrainFeatures);
                            characters.Add(building.indoors.characters);
                            if (building.indoors is AnimalHouse)
                            {
                                animals.Add((building.indoors as AnimalHouse).animals);
                            }
                        }

                        if(building is JunimoHut)
                        {
                            storage.Add((building as JunimoHut).output.items);
                        }

                        if (building is Mill)
                        {
                            storage.Add((building as Mill).output.items);
                            storage.Add((building as Mill).input.items);
                        }

                        if(building is ISaveIOChest)
                        {
                            storage.Add((building as ISaveIOChest).output.items);
                            storage.Add((building as ISaveIOChest).input.items);
                        }

                        if(building is ISaveChestList)
                        {
                            for (int i = 0; i < (building as ISaveChestList).storage.Count; i++)
                            {
                                storage.Add((building as ISaveChestList).storage[i].items);
                            }
                            
                        }
                        

                    }
                }
            }
            

        }

        private static void findStorage()
        {
              
            storage.Add(Game1.player.items);
            storage.Add((Game1.getLocationFromName("SeedShop") as SeedShop).itemsToStartSellingTomorrow);
            storage.Add((Game1.getLocationFromName("SeedShop") as SeedShop).itemsFromPlayerToSell);
            storage.Add((Game1.getLocationFromName("FarmHouse") as FarmHouse).fridge.items);
            
            

            foreach (object dict in objects)
            {
                if (dict is SerializableDictionary<Vector2, StardewValley.Object>)
                {
                    foreach (StardewValley.Object obj in (dict as SerializableDictionary<Vector2, StardewValley.Object>).Values)
                    {
                        if (obj is Chest)
                        {
                            storage.Add((obj as Chest).items);
                        }

                        if (obj is ISaveIOChest)
                        {
                            storage.Add((obj as ISaveIOChest).output.items);
                            storage.Add((obj as ISaveIOChest).input.items);
                        }

                        if (obj is ISaveChestList)
                        {
                            for (int i = 0; i < (obj as ISaveChestList).storage.Count; i++)
                            {
                                storage.Add((obj as ISaveChestList).storage[i].items);
                            }

                        }

                    }
                }
            }

            
        }

        private static void findAttachements()
        {

            for (int i = 0; i < storage.Count; i++)
            {

                for (int j = 0; j < (storage[i] as List<Item>).Count; j++)
                {

                    if ((storage[i] as List<Item>)[j] is Tool && ((storage[i] as List<Item>)[j] as Tool).attachments != null && ((storage[i] as List<Item>)[j] as Tool).attachments.Length > 0)
                    {
                        attachements.Add(((storage[i] as List<Item>)[j] as Tool).attachments);
                    }
                }

            }

        }


    }
}
