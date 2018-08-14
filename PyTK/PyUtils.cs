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

namespace PyTK
{
    public class PyUtils
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        public static bool CheckEventConditions(string conditions, object caller = null)
        {
            return checkEventConditions(conditions, caller);
        }

        public PyUtils()
        {

        }

        public static string getContentFolder()
        {
            string folder = Path.Combine(Environment.CurrentDirectory, Game1.content.RootDirectory);
            DirectoryInfo directoryInfo = new DirectoryInfo(folder);

            if (directoryInfo.Exists)
                return folder;

            folder = folder.Replace("MacOS", "Resources");

            directoryInfo = new DirectoryInfo(folder);
            if (directoryInfo.Exists)
                return folder;

            return null;
        }

        public static bool checkEventConditions(string conditions, object caller = null)
        {
            Monitor.Log("Check conditions:" + conditions);
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
                GameLocation location = Game1.currentLocation;
                if (location == null)
                    location = Game1.getFarm();

                if (location == null)
                    result = false;
                else
                    result = Helper.Reflection.GetMethod(location, "checkEventPrecondition").Invoke<int>("9999999/" + conditions) != -1;
            }

            Monitor.Log("Result:" + (result == comparer));
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
            if(caller != null)
                script.Globals["caller"] = caller;
            script.DoString("result = (" + conditions + ")");
            return (bool) script.Globals["result"];
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

        public static void loadContentPacks<TModel>(out List<TModel> packs, string folder, SearchOption option = SearchOption.AllDirectories, IMonitor monitor = null, string filesearch = "*.json") where TModel : class
        {
            packs = loadContentPacks<TModel>(folder,option,monitor,filesearch);
        }

        public static List<TModel> loadContentPacks<TModel>(string folder, SearchOption option = SearchOption.AllDirectories, IMonitor monitor = null, string filesearch = "*.json") where TModel : class
        {
            List<TModel>  packs = new List<TModel>();
            string[] files = Directory.GetFiles(folder, filesearch, option);
            foreach (string file in files)
            {
                TModel pack = Helper.ReadJsonFile<TModel>(file);
                packs.Add(pack);

                if (pack is Types.IContentPack p)
                {
                    p.fileName = new FileInfo(file).Name;
                    p.folderName = new FileInfo(file).Directory.Name;

                    if (monitor != null)
                    {
                        string author = p.author == "none" || p.author == null || p.author == "" ? "" : " by " + p.author;
                        monitor.Log(p.name + " " + p.version + author, LogLevel.Info);
                    }
                }
            }

            return packs;
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

        public static float calc(string expression, params KeyValuePair<string,object>[] paramters)
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
