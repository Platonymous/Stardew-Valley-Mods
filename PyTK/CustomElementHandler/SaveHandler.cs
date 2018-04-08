using Microsoft.Xna.Framework;
using PyTK.Extensions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
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

        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        private static bool ceh = false;

        public static Type cehType;

        public static char seperator = '|';
        public static char seperatorLegacy = '/';
        public static char valueSeperator = '=';
        public static string oldPrefix = "CEHe";
        public static string newPrefix = "PyTK";

        private static List<Func<string, string>> preProcessors = new List<Func<string, string>>();
        private static List<Func<object, object>> objectPreProcessors = new List<Func<object, object>>();

        internal static bool hasSaveType(object o)
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

        internal static void setUpEventHandlers()
        {
            ceh = Helper.ModRegistry.IsLoaded("Platonymous.CustomElementHandler");

            if (ceh)
                cehType = Type.GetType("CustomElementHandler.ISaveElement, CustomElementHandler");

            SaveEvents.BeforeSave += (s, e) => Replace();
            SaveEvents.AfterSave += (s, e) => Rebuild();
            SaveEvents.AfterLoad += (s, e) => Rebuild();
        }

        internal static void Replace()
        {
            OnBeforeRemoving(EventArgs.Empty);
            ReplaceAllObjects<object>(FindAllObjects(Game1.locations, Game1.game1), o => hasSaveType(o), o => getReplacement(o), true);
            ReplaceAllObjects<object>(FindAllObjects(Game1.player, Game1.game1), o => hasSaveType(o), o => getReplacement(o), true);
            OnFinishedRemoving(EventArgs.Empty);
        }

        internal static void Rebuild()
        {
            OnBeforeRebuilding(EventArgs.Empty);
            ReplaceAllObjects<object>(FindAllObjects(Game1.locations, Game1.game1), o => getDataString(o).StartsWith(newPrefix) || getDataString(o).StartsWith(oldPrefix), o => rebuildElement(getDataString(o), o));
            ReplaceAllObjects<object>(FindAllObjects(Game1.player, Game1.game1), o => getDataString(o).StartsWith(newPrefix) || getDataString(o).StartsWith(oldPrefix), o => rebuildElement(getDataString(o), o));
            OnFinishedRebuilding(EventArgs.Empty);
        }

        internal static void RebuildRev()
        {
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

            if (dict != null)
                foreach (object item in dict.Values)
                    FindAllInstances(item, propNames, exploredObjects, found, dict);
            else if (list != null)
                foreach (object item in list)
                    FindAllInstances(item, propNames, exploredObjects, found, list);
            else
            {
                if (found.ContainsKey(parent))
                    found[parent].Add(value);
                else
                    found.Add(parent, new List<object>() { value });

                Type type = value.GetType();

                FieldInfo[] properties = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty);

                foreach (FieldInfo property in properties)
                {
                    if (!propNames.Contains(property.Name))
                        continue;

                    object propertyValue = property.GetValue(value);
                    FindAllInstances(propertyValue, propNames, exploredObjects, found, new KeyValuePair<FieldInfo, object>(property, value));
                }
            }

        }

        private static object checkReplacement(object replacement)
        {
            if (replacement is Chest chest)
            {
                Chest rchest = new Chest(true);
                rchest.name = chest.name;
                rchest.items = chest.items;
                return rchest;
            }
            return replacement;
        }

        private static string getDataString(object o)
        {
            if (o is SObject obj)
                return obj.name;
            if (o is Tool t)
                return t.name;
            if (o is Furniture f)
                return f.name;
            if (o is Hat h)
                return h.name;
            if (o is Boots b)
                return b.name;
            if (o is Ring r)
                return r.name;
            if (o is Building bl)
                return bl.nameOfIndoors;
            if (o is GameLocation gl)
                return (gl.lastQuestionKey != null) ? gl.lastQuestionKey : "not available";
            if (o is FarmAnimal a)
                return a.name;
            if (o is NPC n)
                return n.name;
            if (o is FruitTree ft)
                return ft.fruitSeason;

            return "not available";

        }

        public static string[] splitElemets(string dataString)
        {
            if (!dataString.Contains(seperator) && dataString.Contains(seperatorLegacy))
                return dataString.Split(seperatorLegacy);

            return dataString.Split(seperator);
        }

        private static object rebuildElement(string dataString, object replacement)
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

        private static string getTypeName(object o)
        {
            string[] aqn = o.GetType().AssemblyQualifiedName.Split(',');
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

        private static object getReplacement(ISaveElement ise)
        {
            object o = ise.getReplacement();
            setDataString(o, getReplacementName(ise));
            return o;
        }

        private static object getReplacement(object element)
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
                t.name = dataString;
            if (o is Furniture f)
                f.name = dataString;
            if (o is Hat h)
                h.name = dataString;
            if (o is Boots b)
                b.name = dataString;
            if (o is Ring r)
                r.name = dataString;
            if (o is Building bl)
                bl.nameOfIndoors = dataString;
            if (o is GameLocation gl)
                gl.lastQuestionKey = dataString;
            if (o is FarmAnimal a)
                a.name = dataString;
            if (o is NPC n)
                n.name = dataString;
            if (o is FruitTree ft)
                ft.fruitSeason = dataString;
        }

        private static Dictionary<object, List<object>> FindAllObjects(object obj, object parent)
        {
            return FindAllInstances(obj, parent, new List<string>() { "boots", "leftRing", "rightRing", "hat", "objects", "item", "debris", "attachments", "heldObject", "terrainFeatures", "largeTerrainFeatures", "items", "buildings", "indoors", "resourceClumps", "animals", "characters", "furniture", "input", "output", "storage", "itemsToStartSellingTomorrow", "itemsFromPlayerToSell", "fridge" });
        }

        private static void ReplaceAllObjects<TIn>(Dictionary<object, List<object>> found, Func<TIn, bool> predicate, Func<TIn, object> replacer, bool reverse = false)
        {
            List<object> objs = new List<object>(found.Keys.ToArray());

            if (reverse)
                objs.Reverse();
                
            foreach (object key in objs)
            {
                foreach (object obj in found[key])
                    if (obj is TIn item && predicate(item))
                    {
                        if (key is IDictionary<Vector2, SObject> dict)
                        {
                            foreach (Vector2 k in dict.Keys.Reverse())
                                if (dict[k] == obj)
                                {
                                    if (obj is SObject sobj && splitElemets(sobj.name).Count() > 2 && splitElemets(sobj.name)[1] == "Terrain")
                                    {
                                        GameLocation gl = PyUtils.getAllLocationsAndBuidlings().Find(l => l.objects == dict);
                                        if (gl != null)
                                        {
                                            gl.terrainFeatures.AddOrReplace(k, (TerrainFeature)replacer(item));
                                        }

                                        dict.Remove(k);
                                    }
                                    else
                                        dict[k] = (SObject)replacer(item);

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
                            list.CopyTo(lobj,0);
                            int index = new List<object>(lobj).FindIndex(p => p is TIn && p == obj);
                            list[index] = replacer(item);
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
                        else if (key is List<Item> list)
                        {
                            int index = list.FindIndex(p => p is TIn && p == obj);
                            list.RemoveAt(index);
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
                    }
            }
        }
    }
}
