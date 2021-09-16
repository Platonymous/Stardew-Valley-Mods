using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using PyTK.Extensions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SObject = StardewValley.Object;

namespace PyTK.CustomElementHandler
{
    public static class SaveHandler
    {
        public static event EventHandler FinishedRebuilding;
        public static event EventHandler BeforeRebuilding;
        public static event EventHandler BeforeRemoving;
        public static event EventHandler FinishedRemoving;
        internal static Dictionary<Type, FieldInfo[]> fieldInfoChache;
        internal static Dictionary<Type, PropertyInfo[]> propInfoChache;
        public static bool inSync = false;

        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        public static char seperator = '|';
        public static char valueSeperator = '=';
        public static string newPrefix = "PyTK";

        private static List<Func<string, string>> preProcessors = new List<Func<string, string>>();
        private static List<Func<object, object>> objectPreProcessors = new List<Func<object, object>>();

        internal static List<Action> rebuildActions = new List<Action>();


        public static bool hasSaveType(object o)
        {
            return (o is ISaveElement);
        }

        public static void addPreprocessor(Func<string,string> pp)
        {
            preProcessors.AddOrReplace(pp);
        }

        public static void addReplacementPreprocessor(Func<object, object> pp)
        {
            objectPreProcessors.AddOrReplace(pp);
        }

        internal static void setUpEventHandlers()
        {
            fieldInfoChache = new Dictionary<Type, FieldInfo[]>();
            propInfoChache = new Dictionary<Type, PropertyInfo[]>();
            
            Helper.Events.GameLoop.SaveLoaded += (s, e) =>
            {
                Game1.objectSpriteSheet.Tag = "cod_objects";
                Game1.bigCraftableSpriteSheet.Tag = "cod_objects";
            };

            Harmony instance = new Harmony("PytK.Savehandler.SyncFix");
        }

        public static bool isRebuildable(object o)
        {
            return getDataString(o).StartsWith(newPrefix);
        }

        public static Dictionary<string,string> getAdditionalSaveData(object obj)
        {
            if (obj is ISaveElement ise)
                return ise.getAdditionalSaveData();

            if(isRebuildable(obj))
            {
                string dataString = getDataString(obj).Replace(" " + valueSeperator.ToString() + " ", valueSeperator.ToString());
                string[] data = splitElemets(dataString);

                Dictionary<string, string> additionalSaveData = new Dictionary<string, string>();

                if (data.Length > 3)
                    for (int i = 3; i < data.Length; i++)
                    {
                        if (!data[i].Contains(valueSeperator))
                            continue;
                        string[] entry = data[i].Split(valueSeperator);
                        additionalSaveData.Add(entry[0], entry[1]);
                    }

                return additionalSaveData;
            }

            return null;
        }

        public static void RebuildAll(object obj, object parent)
        {
                return;
        }

        public static void ReplaceAll(object obj, object parent)
        {
                return;
        }

        internal static void RebuildFromActions()
        {
                return;
        }

        internal static void Replace()
        {
                return;
        }


        internal static void Rebuild()
        {
                return;
        }       

        internal static void Cleanup()
        {
            Func<object, bool> predicate = o => cleanupPredicate(o);
            RemoveAllObjects(FindAllObjects(Game1.locations, Game1.game1), predicate);
            RemoveAllObjects(FindAllObjects(Game1.player, Game1.game1),predicate);

            Monitor.Log("Cleanup complete.");
        }

        public static object RebuildObject(object obj)
        {
            return rebuildElement(getDataString(obj), obj);
        }

        private static bool cleanupPredicate(object o)
        {
            if(checkForErrorItem(o) || getDataString(o).StartsWith(newPrefix))
            {
                if(o is Item i)
                    Monitor.Log("Removing " + i.Name);

                return true;
            }

            return false;
        }

        private static bool checkForErrorItem(object o)
        {
            if (o is SObject obj && !(obj is ISaveElement) && obj.getDescription().Contains("???") && Game1.objectInformation.Values.ToList().Find(v => v.StartsWith(obj.name + "/")) == null)
                return true;

            return false;
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

        private static Dictionary<object, List<object>> FindAllInstances(object value, object parent, List<string> propNames)
        {

            HashSet<object> exploredObjects = new HashSet<object>();
            Dictionary<object, List<object>> found = new Dictionary<object, List<object>>();

            FindAllInstances(value, propNames, exploredObjects, found, parent);

            return found;
        }


        private static void FindAllInstances(object value, List<string> propNames, HashSet<object> exploredObjects, Dictionary<object, List<object>> found, object parent)
        {
               if (value == null || exploredObjects.Contains(value))
                   return;

               exploredObjects.Add(value);
               IDictionary dict = value as IDictionary;
               IList list = value as IList;
               ICollection col = value as ICollection;
               OverlaidDictionary ovd = value as OverlaidDictionary;
               NetObjectList<Item> noli = value as NetObjectList<Item>;
               NetCollection<Building> netBuildings = value as NetCollection<Building>;
               NetCollection<Furniture> netFurniture = value as NetCollection<Furniture>;
               NetArray<SObject, NetRef<SObject>> netObjectArray = value as NetArray<SObject, NetRef<SObject>>;
               NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>> terrain = value as NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>;

            if (dict != null)
                foreach (object item in dict.Keys)
                {
                    if (dict[item] != null)
                        FindAllInstances(dict[item], propNames, exploredObjects, found, new KeyValuePair<IDictionary, object>(dict, item));
                }
            else if (list != null)
                foreach (object item in list)
                    FindAllInstances(item, propNames, exploredObjects, found, list);
            else if (col != null)
                foreach (object item in col)
                    FindAllInstances(item, propNames, exploredObjects, found, col);
            else if (ovd != null)
                foreach (Vector2 item in ovd.Keys)
                {
                    if (ovd[item] != null)
                        FindAllInstances(ovd[item], propNames, exploredObjects, found, new KeyValuePair<OverlaidDictionary, object>(ovd, item));
                    FindAllInstances(item, propNames, exploredObjects, found, ovd);
                }
            else if (noli != null)
                foreach (object item in noli)
                    FindAllInstances(item, propNames, exploredObjects, found, noli);
            else if (netBuildings != null)
                foreach (object item in netBuildings)
                    FindAllInstances(item, propNames, exploredObjects, found, netBuildings);
            else if (netFurniture != null)
                foreach (object item in netFurniture)
                    FindAllInstances(item, propNames, exploredObjects, found, netFurniture);
            else if (netObjectArray != null)
                foreach (SObject item in netObjectArray.Where(no => no != null))
                    FindAllInstances(item, propNames, exploredObjects, found, netObjectArray);
            else if (terrain != null)
            {
                var fd = terrain.FieldDict;
                foreach (var item in terrain.Keys.Where(v => terrain[v] != null && terrain[v] is ISaveElement || getDataString(terrain[v]).StartsWith(newPrefix)))
                        FindAllInstances(fd[item], propNames, exploredObjects, found, new KeyValuePair<IDictionary, object>(fd, item));
            }
            else
            {
                if (found.ContainsKey(parent))
                    found[parent].Add(value);
                else
                    found.Add(parent, new List<object>() { value });

                Type type = value.GetType();

                FieldInfo[] fields;

                if (fieldInfoChache.ContainsKey(type))
                    fields = fieldInfoChache[type];
                else
                {
                    fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    fieldInfoChache.Add(type, fields);
                }

                foreach (FieldInfo field in fields.Where(f => propNames.Contains(f.Name)))
                {
                    object propertyValue = field.GetValue(value);
                    if (propertyValue != null && propertyValue.GetType() is Type ty && ty.IsClass)
                        FindAllInstances(propertyValue, propNames, exploredObjects, found, new KeyValuePair<FieldInfo, object>(field, value));
                }
            }
        }

        private static object checkReplacement(object replacement)
        {
            if (replacement is Chest chest && !chest.playerChest.Value)
            {
                Chest rchest = new Chest(true);
                rchest.name = chest.name;
                rchest.items.Clear();
                rchest.items.AddRange(chest.items);
                return rchest;
            }
            return replacement;
        }

        public static string[] splitElemets(string dataString)
        {
            return dataString.Split(seperator);
        }

        public static Dictionary<string, string> parseDataString(object o)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();

            if (!SaveHandler.isRebuildable(o))
                return data;

            string dataString = SaveHandler.getDataString(o);

            if (!dataString.StartsWith(newPrefix))
                return data;

            string[] dataParts = splitElemets(dataString);

            if (dataParts.Length < 3)
                return data;

            data.Add("@Type", dataParts[2]);

            if (dataParts.Length > 3)
                for (int i = 3; i < dataParts.Length; i++)
                {
                    if (!dataParts[i].Contains(valueSeperator))
                        continue;
                    string[] entry = dataParts[i].Split(valueSeperator);
                    data.Add(entry[0], entry[1]);
                }

            return data;
        }

        public static object rebuildElement(string dataString, object replacement)
        {
            if (!dataString.StartsWith(newPrefix))
                return replacement;

            CustomObjectData.collection.useAll(k => k.Value.sdvId = k.Value.getNewSDVId());

            replacement = checkReplacement(replacement);
            objectPreProcessors.useAll(o => replacement = o.Invoke(replacement));
            preProcessors.useAll(p => dataString = p.Invoke(dataString));

            dataString = dataString.Replace(" " + valueSeperator.ToString() + " ", valueSeperator.ToString());
            string[] data = splitElemets(dataString);
            try
            {
                Type T = Type.GetType(data[2]);
                if (T == null)
                    return replacement;

                object o = Activator.CreateInstance(T);

                if (!(hasSaveType(o)))
                    return replacement;

                if (o is Building && replacement is Chest)
                    return replacement;

                Dictionary<string, string> additionalSaveData = new Dictionary<string, string>();

                if (data.Length > 3)
                    for (int i = 3; i < data.Length; i++)
                    {
                        if (!data[i].Contains(valueSeperator))
                            continue;
                        string[] entry = data[i].Split(valueSeperator);
                        additionalSaveData.Add(entry[0], entry[1]);
                    }

                if (o is ICustomObject ico)
                    o = ico.recreate(additionalSaveData, replacement);

                if (o is ISaveElement ise)
                    ise.rebuild(additionalSaveData, replacement);

                if(o is ISaveElement)
                    RebuildAll(o, o);

                return o;
            }
            catch (Exception e)
            {
                Monitor.Log(dataString, LogLevel.Error);

                Monitor.Log("Exception while rebuilding element: " + dataString + ":" + e.Message + ":" + e.StackTrace, LogLevel.Trace);

                return replacement;
            }
        }

        internal static string getTypeName(object o)
        {
            return (getAssemblyTypeName(o.GetType()));
        }

        internal static string getAssemblyTypeName(Type t)
        {
            string[] aqn = t.AssemblyQualifiedName.Split(',');
            return aqn[0] + ", " + aqn[1];
        }

        internal static string getReplacementName(ISaveElement element, string cat = "Item")
        {
            string additionalSaveData = string.Join(seperator.ToString(), new Dictionary<string, string>().Select(x => x.Key + "=" + x.Value));
            try
            {
                if (element.getAdditionalSaveData() is Dictionary<string, string> d)
                    additionalSaveData = string.Join(seperator.ToString(), d.Select(x => x.Key + "=" + x.Value));
            }
            catch
            {

            }
            string type = getTypeName(element);
            string name = newPrefix + seperator + cat + seperator + type + seperator + additionalSaveData;
            return name;
        }

        internal static string getReplacementName(object element, string cat = "Item")
        {
            if (element is ISaveElement ise)
                return getReplacementName(ise, cat);

            string additionalSaveData = string.Join(seperator.ToString(), Helper.Reflection.GetMethod(element, "getAdditionalSaveData").Invoke<Dictionary<string,string>>().Select(x => x.Key + " = " + x.Value));
            string type = getTypeName(element);
            string name = newPrefix + seperator + cat + seperator + type + seperator + additionalSaveData;
            return name;
        }

        public static object getReplacement(ISaveElement ise)
        {
            object o = ise.getReplacement();
            setDataString(o, getReplacementName(ise));
            return o;
        }

        public static object getReplacement(object element)
        {
            object replacement = element;

            if (element is ISaveElement ise)
                replacement = getReplacement(ise);

            return replacement;
        }

        internal static void setDataString(object o, string dataString)
        {
            if (o is SObject obj)
                obj.name = dataString;
            else if (o is Tool t)
                t.Name = dataString;
            else if (o is Furniture f)
                f.name = dataString;
            else if (o is Hat h)
                h.Name = dataString;
            else if (o is Boots b)
                b.Name = dataString;
            else if (o is Ring r)
                r.Name = dataString;
            else if (o is Mill bl)
            {
                if (bl.input.Value == null)
                    bl.input.Value = new Chest(true);
                bl.input.Value.name = dataString;
            }
            else if (o is GameLocation gl)
                gl.uniqueName.Value = dataString;
            else if (o is FarmAnimal a)
                a.Name = dataString;
            else if (o is NPC n)
                n.Name = dataString;
            else if (o is FruitTree ft)
                ft.fruitSeason.Value = dataString;
        }

        internal static string getDataString(object o)
        {
            string data = "not available";

            if (o is SObject obj)
                data = obj.name;
            else if (o is Tool t)
                data = t.Name;
            else if (o is Furniture f)
                data = f.name;
            else if (o is Hat h)
                data = h.Name;
            else if (o is Boots b)
                data = b.Name;
            else if (o is Ring r)
                data = r.Name;
            else if (o is Mill bl && bl.input.Value is Chest bgl)
                data = bgl.name;
            else if (o is GameLocation gl)
                data = gl.uniqueName.Value;
            else if (o is FarmAnimal a)
                data = a.Name;
            else if (o is NPC n)
                data = n.Name;
            else if (o is FruitTree ft)
                data = ft.fruitSeason.Value;

            return (data == null ? "not available" : data);

        }

        private static Dictionary<object, List<object>> FindAllObjects(object obj, object parent)
        {
            return FindAllInstances(obj, parent, new List<string>() {"displayItem","netObjects","overlayObjects", "farmhand","owner", "elements", "parent","value", "array", "FieldDict", "boots", "leftRing", "rightRing", "hat", "objects", "item", "debris", "attachments", "heldObject", "terrainFeatures", "largeTerrainFeatures", "items", "Items", "buildings", "indoors", "resourceClumps", "animals", "characters", "furniture", "input", "output", "storage", "itemsToStartSellingTomorrow", "itemsFromPlayerToSell", "fridge" });
        }

        private static void ReplaceAllObjects<TIn>(Dictionary<object, List<object>> found, Func<TIn, bool> predicate, Func<TIn, object> replacer, bool reverse = false, bool setReverseAction = true)
        {
            List<object> objs = new List<object>(found.Keys.ToArray());

            if (reverse)
                objs.Reverse();

            foreach (object key in objs)
                foreach (object obj in found[key])
                    if (obj is TIn item && predicate(item) && replacer(item) is object r)
                        if (key is IDictionary<Vector2, SObject> dict)
                        {
                            foreach (Vector2 k in dict.Keys.Where(v => dict[v] == obj))
                            {
                                if (setReverseAction)
                                    rebuildActions.AddOrReplace(() => RebuildAll(key, r));

                                dict[k] = (SObject)r;
                            }
                        }
                        else if (key is OverlaidDictionary ovdict)
                        {
                            foreach (Vector2 k in ovdict.Keys.Where(v => ovdict[v] == obj))
                            {
                                if (setReverseAction)
                                    rebuildActions.AddOrReplace(() => RebuildAll(key, r));
                                ovdict[k] = (SObject)r;
                            }
                        }
                        else if (key is SerializableDictionary<Vector2, TerrainFeature> sdictT)
                        {
                            foreach (Vector2 k in sdictT.Keys.Where(v => sdictT[v] == obj))
                            {
                                if (setReverseAction)
                                {
                                    var value = sdictT[k];
                                    rebuildActions.AddOrReplace(() => sdictT.AddOrReplace(k, value));
                                }
                                sdictT[k] = (TerrainFeature)r;
                            }
                        }
                        else if (key is IDictionary<Vector2, TerrainFeature> dictT)
                        {
                            foreach (Vector2 k in dictT.Keys.Where(v => dictT[v] == obj))
                            {
                                if (setReverseAction)
                                {
                                    var value = dictT[k];
                                    rebuildActions.AddOrReplace(() => dictT.AddOrReplace(k, value));
                                }
                                dictT[k] = (TerrainFeature)r;
                            }
                        }
                        else if (key is IList list)
                        {
                            object[] lobj = new object[list.Count];
                            list.CopyTo(lobj, 0);
                            int index = new List<object>(lobj).FindIndex(p => p is TIn && p == obj); if (setReverseAction)
                            {
                                var value = list[index];
                                rebuildActions.AddOrReplace(() => list[index] = value);
                            }
                            list[index] = r;
                        }
                        else if (key is NetObjectList<Item> noli)
                        {
                            Item[] lobj = new Item[noli.Count];
                            noli.CopyTo(lobj, 0);
                            int index = new List<object>(lobj).FindIndex(p => p is TIn && p == obj);
                            if (setReverseAction)
                            {
                                var value = noli[index];
                                rebuildActions.AddOrReplace(() => noli[index] = value);
                            }
                            noli[index] = (Item)r;
                        }
                        else if (key is NetCollection<Building> ncbld)
                        {
                            Building[] lobj = new Building[ncbld.Count];
                            ncbld.CopyTo(lobj, 0);
                            int index = new List<object>(lobj).FindIndex(p => p is TIn && p == obj);
                            if (setReverseAction)
                            {
                                var value = ncbld[index];
                                rebuildActions.AddOrReplace(() => ncbld[index] = value);
                            }
                            ncbld[index] = (Building)r;
                        }
                        else if (key is NetCollection<Furniture> ncfur)
                        {
                            Furniture[] lobj = new Furniture[ncfur.Count];
                            ncfur.CopyTo(lobj, 0);
                            int index = new List<object>(lobj).FindIndex(p => p is TIn && p == obj);
                            if (setReverseAction)
                            {
                                var value = ncfur[index];
                                rebuildActions.AddOrReplace(() => ncfur[index] = value);
                            }
                            ncfur[index] = (Furniture)r;
                        }
                        else if (key is Array arr)
                        {
                            object[] lobj = new object[arr.Length];
                            arr.CopyTo(lobj, 0);
                            int index = new List<object>(lobj).FindIndex(p => p is TIn && p == obj);
                            if (setReverseAction)
                            {
                                var value = arr.GetValue(index);
                                rebuildActions.AddOrReplace(() => arr.SetValue(value, index));
                            }
                            arr.SetValue(r, index);
                        }
                        else if (key is KeyValuePair<FieldInfo, object> kpv)
                        {
                            if (setReverseAction)
                            {
                                var value = kpv.Key.GetValue(kpv.Value);
                                rebuildActions.AddOrReplace(() => kpv.Key.SetValue(kpv.Value, value));
                            }
                            kpv.Key.SetValue(kpv.Value, r);
                        }
                        else if (key is KeyValuePair<PropertyInfo, object> kppv)
                        {
                            if (setReverseAction)
                            {
                                var value = kppv.Key.GetValue(kppv.Value);
                                rebuildActions.AddOrReplace(() => kppv.Key.SetValue(kppv.Value, value));
                            }
                            kppv.Key.SetValue(kppv.Value, r);
                        }
                        else if (key is NetArray<SObject, NetRef<SObject>> naso)
                        {
                            List<SObject> slist = naso.ToList();
                            int idx = slist.ToList().FindIndex(s => s == (SObject)(object)item);
                            if (idx >= 0)
                            {
                                if (setReverseAction)
                                {
                                    var value = naso[idx];
                                    rebuildActions.AddOrReplace(() => naso[idx] = value);
                                }
                                naso[idx] = (SObject)r;
                            }
                        }
                        else if (key is KeyValuePair<IDictionary, object> idict)
                        {
                            if (setReverseAction)
                            {
                                var value = idict.Key[idict.Value];
                                rebuildActions.AddOrReplace(() => idict.Key[idict.Value] = value);
                            }
                            idict.Key[idict.Value] = r;
                        }
                        else if (key is KeyValuePair<OverlaidDictionary, object> odict)
                        {
                            if (setReverseAction)
                            {
                                var value = odict.Key[(Vector2)odict.Value];
                                rebuildActions.AddOrReplace(() => odict.Key[(Vector2)odict.Value] = value);
                            }
                            odict.Key[(Vector2)odict.Value] = (SObject)r;
                        }
        }

        private static void RemoveAllObjects<TIn>(Dictionary<object, List<object>> found, Func<TIn, bool> predicate)
        {
            List<object> objs = new List<object>(found.Keys.ToArray());

            foreach (object key in objs)
                foreach (object obj in found[key])
                    if (obj is TIn item && predicate(item))
                        if (key is IDictionary<Vector2, SObject> dict)
                            foreach (Vector2 k in dict.Keys.Where(v => dict[v] == obj))
                                dict.Remove(k);
                        else if (key is OverlaidDictionary ovdict)
                            foreach (Vector2 k in ovdict.Keys.Where(v => ovdict[v] == obj))
                                ovdict.Remove(k);
                        else if (key is SerializableDictionary<Vector2, TerrainFeature> sdictT)
                            foreach (Vector2 k in sdictT.Keys.Where(v => sdictT[v] == obj))
                                sdictT.Remove(k);
                        else if (key is IDictionary<Vector2, TerrainFeature> dictT)
                            foreach (Vector2 k in dictT.Keys.Where(v => dictT[v] == obj))
                                dictT.Remove(k);
                        else if (key is IList list)
                            list.Remove(obj);
                        else if (key is NetObjectList<Item> noli)
                            noli.Remove((Item)obj);
                        else if (key is NetCollection<Building> ncbld)
                            ncbld.Remove((Building)obj);
                        else if (key is NetCollection<Furniture> ncfur)
                            ncfur.Remove((Furniture)obj);
                        else if (key is Array arr)
                        {
                            object[] lobj = new object[arr.Length];
                            arr.CopyTo(lobj, 0);
                            int index = new List<object>(lobj).FindIndex(p => p is TIn && p == obj);
                            arr.SetValue(null, index);
                        }
                        else if (key is KeyValuePair<FieldInfo, object> kpv)
                            kpv.Key.SetValue(kpv.Value, null);
                        else if (key is KeyValuePair<PropertyInfo, object> kppv)
                            kppv.Key.SetValue(kppv.Value, null);
                        else if (key is NetArray<SObject, NetRef<SObject>> naso)
                            naso.Remove((SObject)obj);
                        else if (key is KeyValuePair<IDictionary, object> idict)
                            idict.Key.Remove(idict.Value);
                        else if (key is KeyValuePair<OverlaidDictionary, object> odict)
                            odict.Key.Remove((Vector2)odict.Value);
        }
    }
}
