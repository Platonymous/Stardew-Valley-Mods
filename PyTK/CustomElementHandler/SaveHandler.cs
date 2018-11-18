using Microsoft.Xna.Framework;
using Netcode;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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
using System.Threading.Tasks;
using SObject = StardewValley.Object;

namespace PyTK.CustomElementHandler
{
    public static class SaveHandler
    {
        public static event EventHandler FinishedRebuilding;
        public static event EventHandler BeforeRebuilding;
        public static event EventHandler BeforeRemoving;
        public static event EventHandler FinishedRemoving;
        internal static IPyResponder TypeChecker;
        internal static Dictionary<Type, FieldInfo[]> fieldInfoChache;
        internal static Dictionary<Type, PropertyInfo[]> propInfoChache;
        public static bool inSync = false;

        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;
        internal static bool inSaveMode { get; set; } = false;

        private static bool ceh = false;

        public static Type cehType;

        public static char seperator = '|';
        public static char seperatorLegacy = '/';
        public static char valueSeperator = '=';
        public static string oldPrefix = "CEHe";
        public static string newPrefix = "PyTK";
        internal static string typeCheckerName = "PyTK.TypeChecker";

        private static List<Func<string, string>> preProcessors = new List<Func<string, string>>();
        private static List<Func<object, object>> objectPreProcessors = new List<Func<object, object>>();

        public static bool hasSaveType(object o)
        {
            return (o is ISaveElement || ceh && o.GetType().GetInterfaces().Contains(cehType));
        }

        public static void addPreprocessor(Func<string,string> pp)
        {
            preProcessors.AddOrReplace(pp);
        }

        public static void addReplacementPreprocessor(Func<object, object> pp)
        {
            objectPreProcessors.AddOrReplace(pp);
        }

        internal static void rebuildIfAvailable()
        {
            Task.Run(() =>
            {
                OnBeforeRebuilding(EventArgs.Empty);

                ReplaceAllObjects<object>(FindAllObjects(Game1.player, Game1.game1), (o) => canBeRebuildInMultiplayer(o), o => rebuildElement(getDataString(o), o));
                ReplaceAllObjects<object>(FindAllObjects(Game1.locations, Game1.game1), (o) => canBeRebuildInMultiplayer(o), o => rebuildElement(getDataString(o), o));
                foreach (Farmer farmer in Game1.otherFarmers.Values)
                    ReplaceAllObjects<object>(FindAllObjects(farmer, Game1.game1), (o) => canBeRebuildInMultiplayer(o), o => rebuildElement(getDataString(o), o));

                OnFinishedRebuilding(EventArgs.Empty);

            });
        }

        internal static List<string> typeCheckCache = new List<string>();

        internal static bool canBeRebuildInMultiplayer(object obj)
        {
            if (!(getDataString(obj).StartsWith(newPrefix) || getDataString(obj).StartsWith(oldPrefix)))
                return false;

            string dataString = getDataString(obj);
            preProcessors.useAll(p => dataString = p.Invoke(dataString));

            dataString = dataString.Replace(" " + valueSeperator.ToString() + " ", valueSeperator.ToString());
            string[] data = splitElemets(dataString);

            bool result = true;

            if (typeCheckCache.Contains(data[2]))
                return true;

            foreach (Farmer farmer in Game1.otherFarmers.Values.Where(f => f.isActive() && f != Game1.player))
            {
                Task<bool> fResult = PyNet.sendRequestToFarmer<bool>(typeCheckerName, data[2], farmer);
                fResult.Wait();
                result = result && fResult.Result;
            }

            if (result)
                typeCheckCache.Add(data[2]);

            return result;
        }

        internal static void setUpEventHandlers()
        {
            ceh = Helper.ModRegistry.IsLoaded("Platonymous.CustomElementHandler");

            if (ceh)
                cehType = Type.GetType("CustomElementHandler.ISaveElement, CustomElementHandler");

            fieldInfoChache = new Dictionary<Type, FieldInfo[]>();
            propInfoChache = new Dictionary<Type, PropertyInfo[]>();

            SaveEvents.BeforeSave += (s, e) => Replace();
            SaveEvents.AfterSave += (s, e) => Rebuild();
            SaveEvents.AfterLoad += (s, e) => Rebuild();
            Events.PyTimeEvents.BeforeSleepEvents += (s, e) => { if (Game1.IsClient) { Replace(); } };
            TimeEvents.AfterDayStarted += (s, e) => {
                Rebuild();
                Game1.objectSpriteSheet.Tag = "cod_objects";
                    Game1.bigCraftableSpriteSheet.Tag = "cod_bigobject";
            };

            TypeChecker = new PyResponder<bool, string>(typeCheckerName, (s) =>
              {
                  return Type.GetType(s) != null;
              },16);
            TypeChecker.start();
        }

        private static bool isRebuildable(object o)
        {
            return getDataString(o).StartsWith(newPrefix) || getDataString(o).StartsWith(oldPrefix);
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
            ReplaceAllObjects<object>(FindAllObjects(obj, parent), o => getDataString(o).StartsWith(newPrefix) || getDataString(o).StartsWith(oldPrefix), o => rebuildElement(getDataString(o), o));
        }

        public static void ReplaceAll(object obj, object parent)
        {
            ReplaceAllObjects<object>(FindAllObjects(obj, parent), o => hasSaveType(o), o => getReplacement(o), true);
        }

        internal static void Replace()
        {
            OnBeforeRemoving(EventArgs.Empty);
            ReplaceAllObjects<object>(FindAllObjects(Game1.locations, Game1.game1), o => hasSaveType(o), o => getReplacement(o), true);
            ReplaceAllObjects<object>(FindAllObjects(Game1.player, Game1.game1), o => hasSaveType(o), o => getReplacement(o), true);
            foreach (Farmer farmer in Game1.otherFarmers.Values)
                ReplaceAllObjects<object>(FindAllObjects(farmer, Game1.game1), o => hasSaveType(o), o => getReplacement(o), true);
            OnFinishedRemoving(EventArgs.Empty);
        }

        internal static void Rebuild()
        {
            if (Game1.IsMultiplayer)
            {
                rebuildIfAvailable();
                return;
            }

            OnBeforeRebuilding(EventArgs.Empty);
            ReplaceAllObjects<object>(FindAllObjects(Game1.locations, Game1.game1), o => getDataString(o).StartsWith(newPrefix) || getDataString(o).StartsWith(oldPrefix), o => rebuildElement(getDataString(o), o));
            ReplaceAllObjects<object>(FindAllObjects(Game1.player, Game1.game1), o => getDataString(o).StartsWith(newPrefix) || getDataString(o).StartsWith(oldPrefix), o => rebuildElement(getDataString(o), o));
            foreach (Farmer farmer in Game1.otherFarmers.Values)
                ReplaceAllObjects<object>(FindAllObjects(farmer, Game1.game1), o => getDataString(o).StartsWith(newPrefix) || getDataString(o).StartsWith(oldPrefix), o => rebuildElement(getDataString(o), o));
            OnFinishedRebuilding(EventArgs.Empty);
        }
     
        internal static void RebuildRev()
        {
            if (Game1.IsMultiplayer && Game1.numberOfPlayers() > 1 && Game1.IsServer)
                return;

            OnBeforeRebuilding(EventArgs.Empty);
            ReplaceAllObjects<object>(FindAllObjects(Game1.locations, Game1.game1), o => getDataString(o).StartsWith(newPrefix) || getDataString(o).StartsWith(oldPrefix), o => rebuildElement(getDataString(o), o), true);
            ReplaceAllObjects<object>(FindAllObjects(Game1.player, Game1.game1), o => getDataString(o).StartsWith(newPrefix) || getDataString(o).StartsWith(oldPrefix), o => rebuildElement(getDataString(o), o), true);
            OnFinishedRebuilding(EventArgs.Empty);
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
            if(checkForErrorItem(o) || getDataString(o).StartsWith(newPrefix) || getDataString(o).StartsWith(oldPrefix))
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


            if (dict != null)
                foreach (object item in dict.Values)
                    FindAllInstances(item, propNames, exploredObjects, found, dict);
            else if (list != null)
                foreach (object item in list)
                    FindAllInstances(item, propNames, exploredObjects, found, list);
            else if (col != null)
                foreach (object item in col)
                    FindAllInstances(item, propNames, exploredObjects, found, col);
            else if (ovd != null)
                foreach (object item in ovd.Values)
                    FindAllInstances(item, propNames, exploredObjects, found, ovd);
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
                else {
                    fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    fieldInfoChache.Add(type, fields);
                }

                foreach (FieldInfo field in fields.Where(f => propNames.Contains(f.Name)))
                {
                    object propertyValue = field.GetValue(value);
                    if (propertyValue != null && propertyValue.GetType() is Type t && t.IsClass)
                        FindAllInstances(propertyValue, propNames, exploredObjects, found, new KeyValuePair<FieldInfo, object>(field, value));
                }

            }
        }

        private static object checkReplacement(object replacement)
        {
            if (replacement is Chest chest)
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
            if (!dataString.Contains(seperator) && dataString.Contains(seperatorLegacy))
                return dataString.Split(seperatorLegacy);

            return dataString.Split(seperator);
        }

        public static object rebuildElement(string dataString, object replacement)
        {
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
                else
                {
                    Dictionary<string, string> cehAdditionalSaveData = new Dictionary<string, string>();
                    foreach (KeyValuePair<string, string> k in additionalSaveData)
                        cehAdditionalSaveData.Add(k.Key, k.Value);

                    MethodInfo rebuild = cehType.GetMethods(BindingFlags.Instance | BindingFlags.Public).ToList().Find(m => m.Name == "rebuild");
                    Monitor.Log(rebuild.Name + " - " + o.GetType().AssemblyQualifiedName);
                    rebuild.Invoke(o, new[] { cehAdditionalSaveData, replacement });
                }

                return o;
            }
            catch (Exception e)
            {
                Monitor.Log("Exception while rebuilding element: " + dataString + ":" +e.Message + ":" +e.StackTrace, LogLevel.Trace);
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

        private static string getReplacementName(ISaveElement element, string cat = "Item")
        {
            string additionalSaveData = string.Join(seperator.ToString(), element.getAdditionalSaveData().Select(x => x.Key + "=" + x.Value));
            string type = getTypeName(element);
            string name = newPrefix + seperator + cat + seperator + type + seperator + additionalSaveData;
            return name;
        }

        private static string getReplacementName(object element, string cat = "Item")
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
            else
                replacement = Helper.Reflection.GetMethod(element, "getReplacement").Invoke<object>();

            if (element is TerrainFeature && !(replacement is FruitTree))
                replacement = new FruitTree(628, 1);

            setDataString(replacement, getReplacementName(element));
            return replacement;
        }

        private static void setDataString(object o, string dataString)
        {
            if (o is SObject obj)
                obj.name = dataString;
            if (o is Tool t)
                t.Name = dataString;
            if (o is Furniture f)
                f.name = dataString;
            if (o is Hat h)
                h.Name = dataString;
            if (o is Boots b)
                b.Name = dataString;
            if (o is Ring r)
                r.Name = dataString;
            if (o is Mill bl)
            {
                if (bl.input.Value == null)
                    bl.input.Value = new Chest(true);
                bl.input.Value.name = dataString;
            }
            if (o is GameLocation gl)
                gl.uniqueName.Value = dataString;
            if (o is FarmAnimal a)
                a.Name = dataString;
            if (o is NPC n)
                n.Name = dataString;
            if (o is FruitTree ft)
                ft.fruitSeason.Value = dataString;
        }

        internal static string getDataString(object o)
        {
            string data = "not available";

            if (o is SObject obj)
                data = obj.name;
            if (o is Tool t)
                data = t.Name;
            if (o is Furniture f)
                data = f.name;
            if (o is Hat h)
                data = h.Name;
            if (o is Boots b)
                data = b.Name;
            if (o is Ring r)
                data = r.Name;
            if (o is Mill bl && bl.input.Value is Chest bgl)
                data = bgl.name;
            if (o is GameLocation gl)
                data = gl.uniqueName.Value;
            if (o is FarmAnimal a)
                data = a.Name;
            if (o is NPC n)
                data = n.Name;
            if (o is FruitTree ft)
                data = ft.fruitSeason.Value;

            return (data == null ? "not available" : data);

        }

        private static Dictionary<object, List<object>> FindAllObjects(object obj, object parent)
        {
            return FindAllInstances(obj, parent, new List<string>() {"displayItem","netObjects","overlayObjects", "farmhand","owner", "elements", "parent","value", "array", "FieldDict", "boots", "leftRing", "rightRing", "hat", "objects", "item", "debris", "attachments", "heldObject", "terrainFeatures", "largeTerrainFeatures", "items", "Items", "buildings", "indoors", "resourceClumps", "animals", "characters", "furniture", "input", "output", "storage", "itemsToStartSellingTomorrow", "itemsFromPlayerToSell", "fridge" });
        }

        private static void ReplaceAllObjects<TIn>(Dictionary<object, List<object>> found, Func<TIn, bool> predicate, Func<TIn, object> replacer, bool reverse = false)
        {
            List<object> objs = new List<object>(found.Keys.ToArray());

            if (reverse)
                objs.Reverse();

            foreach (object key in objs)
            {
                foreach (object obj in found[key])
                {

                    if (obj is TIn item && predicate(item))
                    {

                        if (key is IDictionary<Vector2, SObject> dict)
                        {
                            foreach (Vector2 k in dict.Keys.Reverse())
                                if (dict[k] == obj)
                                {
                                    if (obj is SObject sobj && splitElemets(sobj.name).Count() > 2 && splitElemets(sobj.name)[1] == "Terrain")
                                    {
                                        GameLocation gl = PyUtils.getAllLocationsAndBuidlings().Find(l => l.overlayObjects == dict);
                                        if (gl != null)
                                        {
                                            if (gl.terrainFeatures.ContainsKey(k))
                                                gl.terrainFeatures[k] = (TerrainFeature)replacer(item);
                                            else
                                                gl.terrainFeatures.Add(k, (TerrainFeature)replacer(item));
                                        }

                                        dict.Remove(k);
                                    }
                                    else
                                        dict[k] = (SObject)replacer(item);

                                    break;
                                }
                        }
                        else if (key is OverlaidDictionary ovdict)
                        {
                            foreach (Vector2 k in ovdict.Keys.Reverse())
                                if (ovdict[k] == obj)
                                {
                                    if (obj is SObject sobj && splitElemets(sobj.name).Count() > 2 && splitElemets(sobj.name)[1] == "Terrain")
                                    {
                                        GameLocation gl = PyUtils.getAllLocationsAndBuidlings().Find(l => l.objects == ovdict);
                                        if (gl != null)
                                        {
                                            if (gl.terrainFeatures.ContainsKey(k))
                                                gl.terrainFeatures[k] = (TerrainFeature)replacer(item);
                                            else
                                                gl.terrainFeatures.Add(k, (TerrainFeature)replacer(item));
                                        }

                                        ovdict.Remove(k);
                                    }
                                    else
                                        ovdict[k] = (SObject)replacer(item);

                                    break;
                                }
                        }
                        else if (key is SerializableDictionary<Vector2, TerrainFeature> sdictT)
                        {
                            foreach (Vector2 k in sdictT.Keys.Reverse())
                                if (sdictT[k] == obj)
                                {
                                    sdictT[k] = (TerrainFeature)replacer(item);
                                    break;
                                }
                        }
                        else if (key is IDictionary<Vector2, TerrainFeature> dictT)
                        {
                            foreach (Vector2 k in dictT.Keys.Reverse())
                                if (dictT[k] == obj)
                                {
                                    dictT[k] = (TerrainFeature)replacer(item);
                                    break;
                                }
                        }
                        else if (key is IList list)
                        {
                            object[] lobj = new object[list.Count];
                            list.CopyTo(lobj, 0);
                            int index = new List<object>(lobj).FindIndex(p => p is TIn && p == obj);
                            list[index] = replacer(item);
                        }
                        else if (key is NetObjectList<Item> noli)
                        {
                            Item[] lobj = new Item[noli.Count];
                            noli.CopyTo(lobj, 0);
                            int index = new List<object>(lobj).FindIndex(p => p is TIn && p == obj);
                            noli[index] = (Item)replacer(item);
                        }
                        else if (key is NetCollection<Building> ncbld)
                        {
                            Building[] lobj = new Building[ncbld.Count];
                            ncbld.CopyTo(lobj, 0);
                            int index = new List<object>(lobj).FindIndex(p => p is TIn && p == obj);
                            ncbld[index] = (Building)replacer(item);
                        }
                        else if (key is NetCollection<Furniture> ncfur)
                        {
                            Furniture[] lobj = new Furniture[ncfur.Count];
                            ncfur.CopyTo(lobj, 0);
                            int index = new List<object>(lobj).FindIndex(p => p is TIn && p == obj);
                            ncfur[index] = (Furniture)replacer(item);
                        }
                        else if (key is Array arr)
                        {
                            object[] lobj = new object[arr.Length];
                            arr.CopyTo(lobj, 0);
                            int index = new List<object>(lobj).FindIndex(p => p is TIn && p == obj);
                            arr.SetValue(replacer(item), index);
                        }
                        else if (key is KeyValuePair<FieldInfo, object> kpv)
                        {
                            kpv.Key.SetValue(kpv.Value, replacer(item));
                        }
                        else if (key is KeyValuePair<PropertyInfo, object> kppv)
                        {
                            kppv.Key.SetValue(kppv.Value, replacer(item));
                        }
                        else if (key is NetArray<SObject, NetRef<SObject>> naso)
                        {
                            List<SObject> slist = naso.ToList();
                            int idx = slist.ToList().FindIndex(s => s == (SObject)(object)item);
                            if (idx >= 0)
                                naso[idx] = (SObject) replacer(item);
                        }

                    }
                }
            }
        }

        private static void RemoveAllObjects<TIn>(Dictionary<object, List<object>> found, Func<TIn, bool> predicate)
        {

            foreach (object key in found.Keys)
            {
                foreach (object obj in found[key])
                    if (obj is TIn item && predicate(item))
                    {

                        if (key is Dictionary<Vector2, SObject> dict)
                        {
                            foreach (Vector2 k in dict.Keys)
                                if (dict[k] == obj)
                                {
                                    dict.Remove(k);
                                    break;
                                }
                        }
                        else if (key is OverlaidDictionary ovdict)
                        {
                            foreach (Vector2 k in ovdict.Keys)
                                if (ovdict[k] == obj)
                                {
                                    ovdict.Remove(k);
                                    break;
                                }
                        }
                        else if (key is SerializableDictionary<Vector2, TerrainFeature> sdict)
                        {
                            foreach (Vector2 k in sdict.Keys)
                                if (sdict[k] == obj)
                                {
                                    sdict.Remove(k);
                                    break;
                                }
                        }
                        else if (key is List<Item> list)
                        {
                            int index = list.FindIndex(p => p is TIn && p == obj);
                            list.RemoveAt(index);
                        }
                        else if (key is NetObjectList<Item> noli)
                        {
                            int index = noli.ToList<Item>().FindIndex(p => p is TIn && p == obj);
                            noli.RemoveAt(index);
                        }
                        else if (key is NetCollection<Building> ncbld)
                        {
                            int index = ncbld.ToList<Building>().FindIndex(p => p is TIn && p == obj);
                            ncbld.RemoveAt(index);
                        }
                        else if (key is NetCollection<Furniture> ncfur)
                        {
                            int index = ncfur.ToList<Furniture>().FindIndex(p => p is TIn && p == obj);
                            ncfur.RemoveAt(index);
                        }
                        else if (key is List<Furniture> furniture)
                        {
                            int index = furniture.FindIndex(p => p is TIn && p == obj);
                            furniture.RemoveAt(index);
                        }
                        else if (key is KeyValuePair<FieldInfo, object> kpv)
                        {
                            kpv.Key.SetValue(kpv.Value, null);
                        }
                        else if (key is KeyValuePair<PropertyInfo, object> kppv)
                        {
                            kppv.Key.SetValue(kppv.Value, null);
                        }
                        else if (key is NetArray<SObject, NetRef<SObject>> naso)
                        {
                            List<SObject> slist = naso.ToList();
                            int idx = slist.ToList().FindIndex(s => s == (SObject)(object)item);
                            if (idx >= 0)
                                naso[idx] = null;
                        }


                    }
            }
        }
    }
}
