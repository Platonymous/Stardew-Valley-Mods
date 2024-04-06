using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMXTile;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;
using SObject = StardewValley.Object;


namespace TMXLoader
{
    public class LuaUtils
    {
        internal static IModHelper Helper { get; } = TMXLoaderMod.helper;
        internal static IMonitor Monitor { get; } = TMXLoaderMod.monitor;

        public static void log(string text)
        {
            Monitor.Log(text, LogLevel.Info);
        }

        public static bool hasMod(string mod)
        {
            return Helper.ModRegistry.IsLoaded(mod);
        }

        public static int setCounter(string id, int value)
        {
            return counters(id, value, true);
        }

        public static int counters(string id, int value = 0, bool set = false)
        {
            if (!TMXLoaderMod.pytksaveData.Counters.ContainsKey(id))
                TMXLoaderMod.pytksaveData.Counters.Add(id, 0);

            int before = TMXLoaderMod.pytksaveData.Counters[id];

            if (!set)
                TMXLoaderMod.pytksaveData.Counters[id] += value;
            else
                TMXLoaderMod.pytksaveData.Counters[id] = value;

            int after = TMXLoaderMod.pytksaveData.Counters[id];

            int dif = after - before;
            if (dif != 0 && Game1.IsMultiplayer)
                TMXLoaderMod.syncCounter(id, dif);

            return TMXLoaderMod.pytksaveData.Counters[id];
        }

        public static object getInstance(string type, params object[] args)
        {
           return Activator.CreateInstance(Type.GetType(type),args);
        }
        public object callStaticMethod(Type type, string method, params object[] args)
        {
            if (type.GetMethod(method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) is MethodInfo methodInfo)
                return methodInfo.Invoke(null, args);
            else
                return false;
        }

        public object callStaticMethod(string typeName, string method, params object[] args)
        {
           return callStaticMethod(Type.GetType(typeName),method,args);
        }

        public static bool invertSwitch(string id)
        {
            id = "switch_" + id;
            return setCounter(id, counters(id) == 1 ? 0 : 1) == 1;
        }

        public static bool switches(string id, bool? value = null)
        {
            id = "switch_" + id;
            if (value.HasValue)
                setCounter(id, value.Value ? 1 : 0);

            return counters(id) == 1;
        }

        public static bool setMapProperty(Map map, string property, string value)
        {
                map.Properties[property] = value;
                return true;
        }

        public static bool setMapProperty(string locationName, string property, string value)
        {
            return setMapProperty(Game1.getLocationFromName(locationName).Map, property, value);
        }

        public static bool setLayerProperty(string locationName, string layer, string property, string value)
        {
            return setLayerProperty(Game1.getLocationFromName(locationName).Map, layer, property, value);
        }

        public static string getMapProperty(Map map, string property)
        {
            PropertyValue p = "";
            if (map.Properties.TryGetValue(property, out p))
                return p.ToString();

            return "";
        }

        public static string getLayerProperty(Map map, string layer, string property)
        {
            PropertyValue p = "";
            var mapLayer = map.GetLayer(layer);
            if (mapLayer != null && mapLayer.Properties.TryGetValue(property, out p))
                return p.ToString();

            return "";
        }

        public static bool setLayerProperty(Map map, string layer, string property, string value)
        {
            var mapLayer = map.GetLayer(layer);
            if (mapLayer != null)
            {
                mapLayer.Properties[property] = value;
                return true;
            }
            return false;
        }

        public static string getMapProperty(string locationName, string layer, string property)
        {
            return getLayerProperty(Game1.getLocationFromName(locationName).Map, layer, property);
        }

        public static GameLocation getLocation(string locationName)
        {
            return Game1.getLocationFromName(locationName);
        }

        public static string getMapProperty(string locationName, string property)
        {
            return getMapProperty(Game1.getLocationFromName(locationName).Map, property);
        }

        public static void updateWarps(string locationName)
        {
            updateWarps(Game1.getLocationFromName(locationName));
        }

        public static void updateWarps(GameLocation location)
        {
            location.warps.Clear();
            PropertyValue p = "";
            if (location.Map.Properties.TryGetValue("Warp", out p) && p != "")
                location.updateWarps();
        }

        public static bool setGameValue(string field, object value, int delay = 0, object root = null)
        {
            List<string> tree = new List<string>(field.Split('.'));
            FieldInfo fieldInfo = null;
                
            object currentBranch = root == null ? Game1.game1 : root;

            fieldInfo = typeof(Game1).GetField(tree[0], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            tree.Remove(tree[0]);

            if (tree.Count > 0)
                foreach (string branch in tree)
                {
                    currentBranch = fieldInfo.GetValue(currentBranch);
                    fieldInfo = currentBranch.GetType().GetField(branch, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                }

            if (delay > 0)
                PyUtils.setDelayedAction(delay, () => fieldInfo.SetValue(fieldInfo.IsStatic ? null : currentBranch, value));
            else
                fieldInfo.SetValue(fieldInfo.IsStatic ? null : currentBranch, value);

            return true;
        }

        public static double getDistance(Vector2 p1, Vector2 p2)
        {
            float distX = Math.Abs(p1.X - p2.X);
            float distY = Math.Abs(p1.Y - p2.Y);
            double dist = (distX * distX) + (distY * distY);
            return dist;
        }
        
        public static double getTileDistance(Vector2 p1, Vector2 p2)
        {
            return Math.Sqrt(getDistance(p1, p2));
        }

        public static string getObjectType(object o)
        {
            return o.GetType().ToString().Split(',')[0];
        }

        public static string getFullObjectType(object o)
        {
            return o.GetType().AssemblyQualifiedName;
        }

        public static void registerTypeFromObject(object obj, bool showErrors = true, bool registerAssembly = false, Func<Type, bool> predicate = null)
        {
            PyLua.registerTypeFromObject(obj, showErrors, registerAssembly, predicate);
        }

        public static void registerTypeFromString(string fullTypeName, bool showErrors = true, bool registerAssembly = false, Func<Type, bool> predicate = null)
        {
            PyLua.registerTypeFromString(fullTypeName, showErrors, registerAssembly, predicate);
        }

        public static void loadGlobals()
        {
            PyLua.loadGlobals();
        }

        public static void addGlobal(string name, object obj)
        {
            PyLua.addGlobal(name, obj);
        }

        public static object getObjectByName(string name, bool bigCraftable = false)
        {
            string index = Game1.objectData.Keys.FirstOrDefault(k => k == name || Game1.objectData[k].Name == name);
            return getObjectByIndex(index, bigCraftable);
        }

        public static object getObjectByIndex(string index, bool bigCraftable = false)
        {
            if (bigCraftable)
                return new SObject(Vector2.Zero, index);
            return new SObject(index, 1);
        }

        public static Color? getColorFromProperty(Map map, string property)
        {
            if (map.Properties.ContainsKey(property) && TMXColor.FromString(map.Properties[property]) is TMXColor color)
                return color.toColor();

            return null;
        }

        public static Color? getColorFromProperty(Layer layer, string property)
        {
            if (layer.Properties.ContainsKey(property) && TMXColor.FromString(layer.Properties[property]) is TMXColor color)
                return color.toColor();

            return null;
        }

        public static Color? getColorFromProperty(Tile tile, string property)
        {
            if (tile.Properties.ContainsKey(property) && TMXColor.FromString(tile.Properties[property]) is TMXColor color)
                return color.toColor();

            return null;
        }

        public static Color getColor(int r, int g, int b, int a)
        {
            return new Color(r, g, b, a);
        }

        public static void setColorProperty(Tile tile, Color color, string property)
        {
            tile.Properties[property] = color.toTMXColor().ToString();
        }

        public static void setColorProperty(Layer layer, Color color, string property)
        {
            layer.Properties[property] = color.toTMXColor().ToString();
        }

        public static void setColorProperty(Map map, Color color, string property)
        {
            map.Properties[property] = color.toTMXColor().ToString();
        }

        public static Color safeColor(Color? color)
        {
            if (color.HasValue)
                return color.Value;

            return Color.White;
        }

    }
}
