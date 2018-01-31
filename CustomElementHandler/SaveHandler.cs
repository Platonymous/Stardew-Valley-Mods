using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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
        private static List<object> buildings;


        public static event EventHandler FinishedRebuilding;
        public static event EventHandler BeforeRebuilding;
        public static event EventHandler BeforeRemoving;
        public static event EventHandler FinishedRemoving;

        public static char seperator = '|';
        public static char seperatorLegacy = '/';
        public static char seperator2 = '#';
        public static string prefix = "CEHe";

        public SaveHandler()
        {

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
            buildings = new List<object>();

            findObjects();
            findStorage();
            findAttachements();
        }

        private static string[] splitElemets(string dataString)
        {
            if(!dataString.Contains(seperator) && dataString.Contains(seperatorLegacy))
                return dataString.Split(seperatorLegacy);

            return dataString.Split(seperator);
        }

        private static object rebuildElement(string dataString, object replacement, bool cleanup)
        {
            string[] data = splitElemets(dataString);

            if (cleanup)
                return null;

            try
            {
                Type T = Type.GetType(data[2]);

                if (T == null)
                {
                    Monitor.Log("Couldn't load: " + data[2]);
                    return replacement;
                }

                ISaveElement newElement = (ISaveElement)Activator.CreateInstance(T);

                Dictionary<string, string> additionalSaveData = new Dictionary<string, string>();

                if (data.Length > 3)
                    for (int i = 3; i < data.Length; i++)
                    {
                        if (!data[i].Contains("="))
                            continue;
                        string[] entry = data[i].Split('=');
                        additionalSaveData.Add(entry[0], entry[1]);
                    }

                if (replacement == null)
                    replacement = new Chest(true);

                newElement.rebuild(additionalSaveData, replacement);

                return newElement;
            }
            catch (Exception e)
            {
                Monitor.Log("Exception while rebuilding element: " + e.Message, LogLevel.Error);
                Monitor.Log("" + e.StackTrace, LogLevel.Error);
                return replacement;
            }
        }


        internal static void placeElements(bool cleanup = false)
        {
            OnBeforeRebuilding(EventArgs.Empty);

            findElements();
            elements = new List<object>();
            elements.AddRange(buildings);
            elements.AddRange(characters);
            elements.AddRange(animals);
            elements.AddRange(objects);
            elements.AddRange(storage);
            elements.AddRange(attachements);

            for (int i = 0; i < Game1.locations.Count; i++)
                if (Game1.locations[i] is BuildableGameLocation buildable && buildable.buildings is List<Building> lb)
                    for (int j = 0; j < lb.Count; j++)
                        if (lb[j].indoors is GameLocation bglgl && bglgl.objects is SerializableDictionary<Vector2, StardewValley.Object> objs && objs.ContainsKey(Vector2.Zero) && objs[Vector2.Zero] is Chest dataobject && dataobject.name.StartsWith(prefix))
                        {
                            string name = dataobject.name;
                            lb[j].indoors.objects.Remove(Vector2.Zero);
                            object replacement = rebuildElement(name, lb[j].indoors, cleanup);
                            lb[j].indoors = (GameLocation)replacement;
                        }

            if (Game1.player.hat != null && Game1.player.hat.name.StartsWith(prefix))
            {
                string name = Game1.player.hat.name;
                object replacement = rebuildElement(name, Game1.player.hat, cleanup);
                Game1.player.hat = (Hat)replacement;
            }

            if (Game1.player.boots != null && Game1.player.boots.name.StartsWith(prefix))
            {
                string name = Game1.player.boots.name;
                object replacement = rebuildElement(name, Game1.player.boots, cleanup);
                Game1.player.boots = (Boots)replacement;
            }

            if (Game1.player.leftRing != null && Game1.player.leftRing.name.StartsWith(prefix))
            {
                string name = Game1.player.leftRing.name;
                object replacement = rebuildElement(name, Game1.player.leftRing, cleanup);
                Game1.player.leftRing = (Ring)replacement;
            }

            if (Game1.player.rightRing != null && Game1.player.rightRing.name.StartsWith(prefix))
            {
                string name = Game1.player.rightRing.name;
                object replacement = rebuildElement(name, Game1.player.rightRing, cleanup);
                Game1.player.rightRing = (Ring)replacement;
            }

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i] is List<Building> bl)
                {
                    for (int j = 0; j < bl.Count; j++)
                    {
                        if (bl[j].nameOfIndoors.StartsWith(prefix))
                        {
                            string name = bl[j].nameOfIndoors;
                            object replacement = rebuildElement(name, bl[j], cleanup);
                            Building nullBuilding = new Building();
                            nullBuilding.nameOfIndoors = "cehRemove";
                            if (cleanup)
                                replacement = nullBuilding;
                            bl[j] = (Building)replacement;

                        }
                    }
                }
                else if (elements[i] is List<Item>)
                {
                    List<Item> list = (List<Item>)elements[i];
                    for (int j = 0; j < list.Count; j++)
                        if (list[j] != null && list[j].Name.StartsWith(prefix))
                        {
                            string name = list[j].Name;

                            if (list[j] is StardewValley.Object)
                                name = (list[j] as StardewValley.Object).name;

                            if (list[j] is StardewValley.Tool)
                                name = (list[j] as StardewValley.Tool).name;

                            object replacement = rebuildElement(name, list[j], cleanup);
                            list[j] = (Item)replacement;
                        }
                }
                else if (elements[i] is SerializableDictionary<Vector2, StardewValley.Object>)
                {
                    SerializableDictionary<Vector2, StardewValley.Object> changes = new SerializableDictionary<Vector2, StardewValley.Object>();
                    SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature> terrainChanges = new SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature>();
                    SerializableDictionary<Vector2, StardewValley.Object> dict = (SerializableDictionary<Vector2, StardewValley.Object>)elements[i];
                    SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature> terrainDict = new SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature>();

                    if (elements[i + 1] is SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature>)
                        terrainDict = (SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature>)elements[i + 1];

                    foreach (Vector2 keyV in dict.Keys)
                        if (dict[keyV].Name.StartsWith(prefix) && !dict[keyV].Name.StartsWith(prefix + seperator + "Loaction"))
                        {
                            string[] preData = dict[keyV].Name.Split(seperator2);
                            string[] data = splitElemets(preData[0]);
                            if (data[1] != "Terrain")
                            {
                                object replacement = rebuildElement(preData[0], dict[keyV], cleanup);
                                changes.Add(keyV, (StardewValley.Object)replacement);
                            }
                            else
                            {
                                object replacement = rebuildElement(preData[0], dict[keyV], cleanup);
                                terrainChanges.Add(keyV, (StardewValley.TerrainFeatures.TerrainFeature)replacement);

                                if (preData.Length == 1)
                                    changes.Add(keyV, new StardewValley.Object(-999, 1));
                                else
                                {

                                    if (!preData.Contains(prefix))
                                    {
                                        changes[keyV] = dict[keyV];
                                        changes[keyV].name = preData[1];
                                    }
                                    else
                                    {
                                        object replacement2 = rebuildElement(preData[1], dict[keyV], cleanup);
                                        changes.Add(keyV, (StardewValley.Object)replacement2);
                                    }
                                }
                            }

                        }

                    foreach (Vector2 keyV in changes.Keys)
                    {
                        if (changes[keyV] is StardewValley.Object o && o.parentSheetIndex != -999)
                            dict[keyV] = o;
                        else
                            dict.Remove(keyV);
                    }

                    foreach (Vector2 keyV in terrainChanges.Keys)
                        terrainDict[keyV] = terrainChanges[keyV];
                }
                else if (elements[i] is StardewValley.Object[])
                {
                    StardewValley.Object[] list = (StardewValley.Object[])elements[i];
                    for (int j = 0; j < list.Length; j++)
                        if (list[j] != null && list[j].name.StartsWith(prefix))
                        {
                            object replacement = rebuildElement(list[j].name, list[j], cleanup);
                            list[j] = (StardewValley.Object)replacement;
                        }
                }
                else if (elements[i] is SerializableDictionary<long, FarmAnimal>)
                {
                    SerializableDictionary<long, FarmAnimal> changes = new SerializableDictionary<long, FarmAnimal>();
                    SerializableDictionary<long, FarmAnimal> dict = (SerializableDictionary<long, FarmAnimal>)elements[i];

                    foreach (long keyL in dict.Keys)
                        if (dict[keyL].name.StartsWith(prefix))
                        {
                            object replacement = rebuildElement(dict[keyL].name, dict[keyL], cleanup);
                            changes[keyL] = (FarmAnimal)replacement;
                        }

                    foreach (long keyL in changes.Keys)
                        dict[keyL] = changes[keyL];
                }
                else if (elements[i] is List<NPC>)
                {
                    List<NPC> list = (List<NPC>)elements[i];
                    for (int j = 0; j < list.Count; j++)
                        if (list[j] != null && list[j].name.StartsWith(prefix))
                        {
                            object replacement = rebuildElement(list[j].name, list[j], cleanup);
                            list[j] = (NPC)replacement;
                        }
                }
                else if (elements[i] is List<Furniture> list)
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j] != null && list[j].name.StartsWith(prefix))
                        {
                            object replacement = rebuildElement(list[j].name, list[j], cleanup);
                            Furniture nullFurniture = new Furniture();
                            nullFurniture.name = "cehRemove";
                            if (cleanup)
                                replacement = nullFurniture;
                            list[j] = (Furniture)replacement;
                        }

                        if (list[j] != null && list[j].heldObject != null && list[j].heldObject.name.StartsWith(prefix))
                        {
                            object replacement = rebuildElement(list[j].heldObject.name, list[j].heldObject, cleanup);
                            Furniture nullFurniture = new Furniture();
                            nullFurniture.name = "cehRemove";
                            if (cleanup)
                                replacement = nullFurniture;
                            list[j].heldObject = (Furniture)replacement;
                        }
                    }
                }

                if (cleanup)
                    foreach (GameLocation l in Game1.locations)
                    {
                        if (l is BuildableGameLocation bgl)
                            bgl.buildings.RemoveAll(b => b.nameOfIndoors == "cehRemove");

                        if (l is DecoratableLocation dl)
                            dl.furniture.RemoveAll(f => f.name == "cehRemove");
                    }
            }

            OnFinishedRebuilding(EventArgs.Empty);
        }

        private static string getTypeName(object o)
        {
            string[] aqn = o.GetType().AssemblyQualifiedName.Split(',');
            return aqn[0] + ", " + aqn[1];
        }

        private static string getReplacementName(ISaveElement element, string cat = "Item")
        {
            string additionalSaveData = string.Join(seperator.ToString(), element.getAdditionalSaveData().Select(x => x.Key + "=" + x.Value));
            string type = getTypeName(element);
            string name = prefix + seperator + cat + seperator + type + seperator + additionalSaveData;
            return name;
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
            elements.AddRange(buildings);

            if (Game1.player.hat is ISaveElement)
            {
                ISaveElement element = (ISaveElement)Game1.player.hat;
                string name = getReplacementName(element);
                Hat replacement = (Hat)element.getReplacement();
                replacement.name = name;
                Game1.player.hat = (Hat)replacement;
            }

            if (Game1.player.boots is ISaveElement)
            {
                ISaveElement element = (ISaveElement)Game1.player.boots;
                string name = getReplacementName(element);
                Boots replacement = (Boots)element.getReplacement();
                replacement.name = name;
                Game1.player.boots = (Boots)replacement;
            }

            if (Game1.player.leftRing is ISaveElement)
            {
                ISaveElement element = (ISaveElement)Game1.player.leftRing;
                string name = getReplacementName(element);
                Ring replacement = (Ring)element.getReplacement();
                replacement.name = name;
                Game1.player.leftRing = (Ring)replacement;
            }

            if (Game1.player.rightRing is ISaveElement)
            {
                ISaveElement element = (ISaveElement)Game1.player.rightRing;
                string name = getReplacementName(element);
                Ring replacement = (Ring)element.getReplacement();
                replacement.name = name;
                Game1.player.rightRing = (Ring)replacement;
            }


            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i] is List<Building> bl)
                {
                    for (int j = 0; j < bl.Count; j++)
                        if (bl[j] is ISaveElement)
                        {
                            ISaveElement element = (ISaveElement)bl[j];
                            string name = getReplacementName(element, "Building");
                            Building replacement = (Building)element.getReplacement();
                            replacement.nameOfIndoors = name;
                            bl[j] = replacement;
                        }
                }
                else if (elements[i] is List<Item>)
                {
                    List<Item> list = (List<Item>)elements[i];
                    for (int j = 0; j < list.Count; j++)
                        if (list[j] is ISaveElement)
                        {
                            ISaveElement element = (ISaveElement)list[j];
                            string name = getReplacementName(element);
                            Item replacement = (Item)element.getReplacement();

                            if (replacement is StardewValley.Object obj)
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
                                list[j] = replacement;
                        }
                }
                else if (elements[i] is List<Furniture> elist)
                {
                    for (int j = 0; j < elist.Count; j++)
                    {
                        if (elist[j].heldObject is ISaveElement)
                        {
                            ISaveElement element = (ISaveElement)elist[j].heldObject;
                            string name = getReplacementName(element);
                            StardewValley.Object replacement = (StardewValley.Object)element.getReplacement();

                            if (replacement is Chest ch)
                                ch.playerChest = true;

                            replacement.name = name;
                            elist[j].heldObject = (Furniture)replacement;
                        }

                        if (elist[j] is ISaveElement)
                        {
                            ISaveElement element = (ISaveElement)elist[j];
                            string name = getReplacementName(element);
                            StardewValley.Object replacement = (StardewValley.Object)element.getReplacement();
                            replacement.name = name;

                            if (replacement is Chest ch)
                                ch.playerChest = true;

                            elist[j] = (Furniture)replacement;
                        }
                    }
                }
                else if (elements[i] is SerializableDictionary<Vector2, StardewValley.Object>)
                {
                    SerializableDictionary<Vector2, StardewValley.Object> changes = new SerializableDictionary<Vector2, StardewValley.Object>();
                    SerializableDictionary<Vector2, StardewValley.Object> dict = (SerializableDictionary<Vector2, StardewValley.Object>)elements[i];

                    foreach (Vector2 keyV in dict.Keys)
                        if (dict[keyV] is ISaveElement)
                        {
                            ISaveElement element = (ISaveElement)dict[keyV];
                            string name = getReplacementName(element, "Object");
                            StardewValley.Object replacement = (StardewValley.Object)element.getReplacement();

                            if (replacement is Chest ch)
                                ch.playerChest = true;

                            replacement.name = name;
                            changes.Add(keyV, (StardewValley.Object)replacement);
                        }

                    foreach (Vector2 keyV in changes.Keys)
                        dict[keyV] = changes[keyV];
                }
                else if (elements[i] is SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature>)
                {
                    SerializableDictionary<Vector2, StardewValley.Object> changes = new SerializableDictionary<Vector2, StardewValley.Object>();
                    SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature> dict = (SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature>)elements[i];

                    foreach (Vector2 keyV in dict.Keys)
                        if (dict[keyV] is ISaveElement)
                        {
                            ISaveElement element = (ISaveElement)dict[keyV];
                            string name = getReplacementName(element, "Terrain");
                            StardewValley.Object replacement = (StardewValley.Object)element.getReplacement();
                            replacement.name = name;

                            if (replacement is Chest ch)
                                ch.playerChest = true;

                            changes.Add(keyV, (StardewValley.Object)replacement);
                        }

                    SerializableDictionary<Vector2, StardewValley.Object> objectLayer = (SerializableDictionary<Vector2, StardewValley.Object>)elements[i - 1];

                    foreach (Vector2 keyV in changes.Keys)
                    {
                        dict[keyV] = new StardewValley.TerrainFeatures.Flooring(0);
                        if (objectLayer.ContainsKey(keyV))
                            objectLayer[keyV].name = changes[keyV].name + "#" + objectLayer[keyV].name;
                        else
                            objectLayer[keyV] = changes[keyV];
                    }
                }
                else if (elements[i] is SerializableDictionary<long, FarmAnimal>)
                {
                    SerializableDictionary<long, FarmAnimal> changes = new SerializableDictionary<long, FarmAnimal>();
                    SerializableDictionary<long, FarmAnimal> dict = (SerializableDictionary<long, FarmAnimal>)elements[i];

                    foreach (long keyL in dict.Keys)
                        if (dict[keyL] is ISaveElement)
                        {
                            ISaveElement element = (ISaveElement)dict[keyL];
                            string name = getReplacementName(element, "Animal");
                            FarmAnimal replacement = (FarmAnimal)element.getReplacement();
                            replacement.name = name;
                            changes.Add(keyL, (FarmAnimal)replacement);
                        }

                    foreach (long keyL in changes.Keys)
                        dict[keyL] = changes[keyL];
                }
                else if (elements[i] is List<NPC>)
                {
                    List<NPC> list = (List<NPC>)elements[i];
                    for (int j = 0; j < list.Count; j++)
                        if (list[j] is ISaveElement)
                        {
                            ISaveElement element = (ISaveElement)list[j];
                            string name = getReplacementName(element, "NPC");
                            NPC replacement = (NPC)element.getReplacement();
                            replacement.name = name;
                            list[j] = (NPC)replacement;
                        }
                }
                else if (elements[i] is StardewValley.Object[])
                {
                    StardewValley.Object[] list = (StardewValley.Object[])elements[i];
                    for (int j = 0; j < list.Length; j++)
                        if (list[j] is ISaveElement)
                        {
                            ISaveElement element = (ISaveElement)list[j];
                            string name = getReplacementName(element, "Attachement");
                            StardewValley.Object replacement = (StardewValley.Object)element.getReplacement();

                            if (replacement is Chest ch)
                                ch.playerChest = true;

                            replacement.name = name;
                            list[j] = (StardewValley.Object)replacement;
                        }
                }

            }

            for (int i = 0; i < Game1.locations.Count; i++)
                if (Game1.locations[i] is BuildableGameLocation buildable && buildable.buildings is List<Building> lb)
                    for (int j = 0; j < lb.Count; j++)
                        if (lb[j].indoors is GameLocation ind && ind is ISaveElement element)
                        {
                            string name = getReplacementName(element, "Location");
                            GameLocation replacement = (GameLocation)element.getReplacement();
                            Chest dataobject = new Chest(true);
                            dataobject.name = name;
                            replacement.objects.Add(Vector2.Zero, dataobject);
                            lb[j].indoors = replacement;
                        }

            for (int i = 0; i < Game1.locations.Count; i++)
                if (Game1.locations[i] is GameLocation gl && gl is ISaveElement)
                    Game1.locations.Remove(gl);

            OnFinishedRemoving(EventArgs.Empty);
        }


        private static void findObjects()
        {

            foreach (GameLocation location in Game1.locations)
            {
                objects.Add(location.objects);
                objects.Add(location.terrainFeatures);

                characters.Add(location.characters);

                if (location is DecoratableLocation dl)
                    objects.Add(dl.furniture);

                if (location is BuildableGameLocation bgl)
                {
                    if (location is Farm)
                        animals.Add((location as Farm).animals);

                    buildings.Add(bgl.buildings);

                    foreach (Building building in bgl.buildings)
                    {
                        if (building.indoors is GameLocation bl)
                        {
                            if (bl.objects is SerializableDictionary<Vector2, StardewValley.Object>)
                                objects.Add(building.indoors.objects);

                            if (bl.terrainFeatures is SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature>)
                                objects.Add(building.indoors.terrainFeatures);

                            if (bl.characters is List<NPC>)
                                characters.Add(building.indoors.characters); ;

                            if (building.indoors is AnimalHouse ah && ah.animals is SerializableDictionary<long, FarmAnimal>)
                                animals.Add(ah.animals);
                        }

                        if (building is JunimoHut)
                            storage.Add((building as JunimoHut).output.items);

                        if (building is Mill)
                        {
                            storage.Add((building as Mill).output.items);
                            storage.Add((building as Mill).input.items);
                        }

                        if (building is ISaveIOChest)
                        {
                            storage.Add((building as ISaveIOChest).output.items);
                            storage.Add((building as ISaveIOChest).input.items);
                        }

                        if (building is ISaveChestList)
                            for (int i = 0; i < (building as ISaveChestList).storage.Count; i++)
                                storage.Add((building as ISaveChestList).storage[i].items);
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
                if (dict is SerializableDictionary<Vector2, StardewValley.Object>)
                    foreach (StardewValley.Object obj in (dict as SerializableDictionary<Vector2, StardewValley.Object>).Values)
                    {
                        if (obj is Chest)
                            storage.Add((obj as Chest).items);

                        if (obj is ISaveIOChest)
                        {
                            storage.Add((obj as ISaveIOChest).output.items);
                            storage.Add((obj as ISaveIOChest).input.items);
                        }

                        if (obj is ISaveChestList)
                            for (int i = 0; i < (obj as ISaveChestList).storage.Count; i++)
                                storage.Add((obj as ISaveChestList).storage[i].items);
                    }
        }

        private static void findAttachements()
        {
            for (int i = 0; i < storage.Count; i++)
                for (int j = 0; j < (storage[i] as List<Item>).Count; j++)
                    if ((storage[i] as List<Item>)[j] is Tool && ((storage[i] as List<Item>)[j] as Tool).attachments != null && ((storage[i] as List<Item>)[j] as Tool).attachments.Length > 0)
                        attachements.Add(((storage[i] as List<Item>)[j] as Tool).attachments);
        }
    }
}
