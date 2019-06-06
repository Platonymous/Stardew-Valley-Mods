using StardewValley;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using StardewValley.Locations;
using System.IO;
using System.Linq;
using StardewValley.Buildings;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using NCalc;
using Harmony;
using System.Reflection;
using PyTK.Lua;
using PyTK.Extensions;
using xTile.Layers;
using xTile;

namespace PyTK
{
    public class PyUtils
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;
        internal static string _contentPath = "";
        public static string ContentPath
        {
            get
            {
                if (_contentPath == "")
                {
                    _contentPath = getContentFolder();
                    Monitor.Log("ContentPath:" + _contentPath, LogLevel.Info);
                }
                return _contentPath;
            }
        }

        public static bool CheckEventConditions(string conditions, object caller = null)
        {
            return checkEventConditions(conditions, caller);
        }

        public PyUtils()
        {

        }

        public static void checkDrawConditions(Map map)
        {
            foreach (Layer layer in map.Layers.Where(l => l.Properties.ContainsKey("DrawConditions")))
                layer.Properties.AddOrReplace("DrawConditionsResult", PyUtils.CheckEventConditions(layer.Properties["DrawConditions"], layer) ? "T" : "F");
        }

        internal static List<GameLocation> getWarpLocations(GameLocation location)
        {
            List<GameLocation> locations = new List<GameLocation>();

            foreach (Warp warp in location.warps)
                if (Game1.getLocationFromName(warp.TargetName) is GameLocation l)
                    locations.AddOrReplace(l);

            return locations;
        }

        public static void adjustWarps(string name)
        {
            if (Game1.getLocationFromName(name) is GameLocation target)
                foreach (var location in PyUtils.getWarpLocations(target))
                    PyUtils.adjustInboundWarps(location, target);
            else
                Monitor.Log("Could not find Location: " + name);
        }

        internal static List<GameLocation> adjustInboundWarps(GameLocation location, GameLocation toLocation = null)
        {
            if (location == null)
                return new List<GameLocation>();

            List<GameLocation> locations = new List<GameLocation>();

            foreach (Warp warp in location.warps)
            {
                Point move = Point.Zero;

                if (warp.X >= location.map.DisplayWidth / Game1.tileSize)
                    move.X++;
                else if (warp.X <= 0)
                    move.X--;
                else if (warp.Y >= location.map.DisplayHeight / Game1.tileSize)
                    move.Y++;
                else
                    move.Y--;

                var target = Game1.getLocationFromName(warp.TargetName);
                if (target == null)
                    continue;

                if (toLocation == null || target == toLocation)
                {
                    locations.AddOrReplace(target);
                    Warp inbound = target.warps.Where(w => w.TargetName == location.Name).OrderBy(w => LuaUtils.getDistance(new Vector2(w.X + move.X, w.Y + move.Y), new Vector2(warp.TargetX, warp.TargetY))).First();
                    if (inbound != null && !(inbound.X == 0 && inbound.Y == 0))
                    {
                        warp.TargetX = inbound.X + move.X;
                        warp.TargetY = inbound.Y + move.Y;
                    }
                }
            }

            Monitor.Log("Adjusted Warps: "+location.Name,LogLevel.Trace);

            return locations;
        }

        public static string getContentFolder()
        {
            string folder = Path.Combine(Constants.ExecutionPath, Game1.content.RootDirectory);
            DirectoryInfo directoryInfo = new DirectoryInfo(folder);

            if (directoryInfo.Exists)
                return folder;

            folder = folder.Replace("MacOS", "Resources").Replace("smapi-internal/","");

            directoryInfo = new DirectoryInfo(folder);
            if (directoryInfo.Exists)
                return folder;
            else
                Monitor.Log("DebugF:" + folder);

            return @"failed";
        }

        public static bool checkEventConditions(string conditions)
        {
            return checkEventConditions(conditions, null, null);
        }

        public static bool checkEventConditions(string conditions, object caller)
        {
            return checkEventConditions(conditions, caller, null);
        }

        public static bool checkEventConditions(string conditions, object caller, GameLocation location)
        {
            if (!Context.IsWorldReady)
                return false;

            if (conditions == null || conditions == "")
                return true;

            bool result = false;
            bool comparer = true;

            if (conditions.StartsWith("NOT "))
            {
                conditions = conditions.Replace("NOT ", "");
                comparer = false;

            }

            if (conditions.StartsWith("PC "))
                result = checkPlayerConditions(conditions.Replace("PC ", ""));
            else if (conditions.StartsWith("LC "))
                result = checkLuaConditions(conditions.Replace("LC ", ""), caller);
            else
            {
                if(location == null)
                    location = Game1.currentLocation;

                if (!(location is GameLocation))
                    location = Game1.getFarm();

                if (location == null)
                {
                    if (conditions.StartsWith("r "))
                    {
                        string[] cond = conditions.Split(' ');
                        return Game1.random.NextDouble() <= double.Parse(cond[1]);
                    }

                    result = false;
                }
                else
                {
                    try
                    {
                        result = Helper.Reflection.GetMethod(location, "checkEventPrecondition").Invoke<int>("9999999/" + conditions) != -1;
                    }
                    catch
                    {
                        try
                        {
                            var m = typeof(GameLocation).GetMethod("checkEventPrecondition", BindingFlags.NonPublic | BindingFlags.Instance);
                            result = (int)m.Invoke(location, new string[] { ("9999999/" + conditions) }) != -1;
                        }
                        catch
                        {
                            result = false;
                        }
                    }
                }
            }

            return result == comparer;
        }

        public static bool checkPlayerConditions(string conditions)
        {
            return Helper.Reflection.GetField<bool>(Game1.player, conditions).GetValue();
        }

        public static bool checkLuaConditions(string conditions, object caller = null)
        {
            var script = PyLua.getNewScript();
            script.Globals["result"] = false;
            if (caller != null)
                script.Globals["caller"] = caller;
            script.DoString("result = (" + conditions + ")");
            return (bool)script.Globals["result"];
        }

        public static List<GameLocation> getAllLocationsAndBuidlings()
        {
            List<GameLocation> list = Game1.locations.ToList();
            foreach (GameLocation location in Game1.locations)
                if (location is BuildableGameLocation bgl)
                    foreach (Building building in bgl.buildings)
                        if (building.indoors.Value != null)
                            list.Add(building.indoors.Value);

            return list;
        }

        public static DelayedAction setDelayedAction(int delay, Action action)
        {
            DelayedAction d = new DelayedAction(delay, () => action());
            Game1.delayedActions.Add(d);
            return d;
        }

        public static Type getTypeSDV(string type)
        {
            string prefix = "StardewValley.";
            Type defaulSDV = Type.GetType(prefix + type + ", Stardew Valley");

            if (defaulSDV != null)
                return defaulSDV;
            else
                return Type.GetType(prefix + type + ", StardewValley");
        }

        public static Texture2D getRectangle(int width, int height, Color color)
        {
            Texture2D rect = new Texture2D(Game1.graphics.GraphicsDevice, width, height);

            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i) data[i] = color;
            rect.SetData(data);
            return rect;
        }

        public static Texture2D getWhitePixel()
        {
            return getRectangle(1, 1, Color.White);
        }



        public static byte[] BitArrayToByteArray(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        internal static void checkAllSaves()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "Saves");
            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (!file.Contains(".") && !file.Contains("old") && !file.Contains("SaveGame"))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();

                    XmlReader reader = XmlReader.Create(Path.Combine(Helper.DirectoryPath, file), settings);
                    FileInfo info = new FileInfo(file);
                    try
                    {
                        while (reader.Read()) ;
                        Monitor.Log(info.Directory.Name + "/" + info.Name + " (OK)");
                    }
                    catch (Exception e)
                    {
                        Monitor.Log("Error in " + info.Directory.Name + "/" + info.Name + ": " + e.Message, LogLevel.Error);
                    }
                }
            }
        }

        public static float calc(string expression, params KeyValuePair<string, object>[] paramters)
        {
            Expression e = new Expression(expression);

            foreach (KeyValuePair<string, object> p in paramters)
                e.Parameters.Add(p.Key, p.Value);

            return float.Parse(e.Evaluate().ToString());
        }

        public static void initOverride(IModHelper helper, Type type, Type patch, List<string> toPatch)
        {
            initOverride(helper.ModRegistry.ModID, type, patch, toPatch);
        }


        public static void initOverride(string harmonyId, Type type, Type patch, List<string> toPatch)
        {
            HarmonyInstance harmony = HarmonyInstance.Create("Platonymous.PyTK.PyUtils." + harmonyId);
            MethodInfo prefix = patch.GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name.ToLower() == "prefix");
            MethodInfo postfix = patch.GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().Find(m => m.Name.ToLower() == "postfix");
            List<MethodInfo> originals = type.GetMethods().ToList();

            foreach (MethodInfo method in originals)
                if (toPatch.Contains(method.Name))
                    harmony.Patch(method, prefix == null ? null : new HarmonyMethod(prefix), postfix == null ? null : new HarmonyMethod(postfix));
        }

    }
}
