using PyTK.Types;
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
using PyTK.Extensions;
using StardewValley.Network;

namespace PyTK
{
    public class PyUtils
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;
        internal static Dictionary<string, List<MPStringMessage>> messages = new Dictionary<string, List<MPStringMessage>>();

        public static bool CheckEventConditions(string conditions)
        {
            return checkEventConditions(conditions);
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

        public static bool checkEventConditions(string conditions)
        {
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
            else
                result = Helper.Reflection.GetMethod(Game1.currentLocation, "checkEventPrecondition").Invoke<int>("9999999/" + conditions) != -1;

            return result == comparer;
        }

        public static bool checkPlayerConditions(string conditions)
        {
            return Helper.Reflection.GetField<bool>(Game1.player, conditions).GetValue();
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
            packs = new List<TModel>();
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

        internal static void checkAllSaves()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley", "Saves");
            string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if(!file.Contains(".") && !file.Contains("old") && !file.Contains("SaveGame"))
                {
                        XmlReaderSettings settings = new XmlReaderSettings();

                        XmlReader reader = XmlReader.Create(Path.Combine(Helper.DirectoryPath, file), settings);
                    FileInfo info = new FileInfo(file);
                    try
                    {
                        while (reader.Read());
                        Monitor.Log( info.Directory.Name + "/" + info.Name + " (OK)");
                    }
                    catch (Exception e)
                    {
                        Monitor.Log("Error in " + info.Directory.Name + "/" + info.Name + ": " + e.Message,LogLevel.Error);
                    }
                }
            }           
        }

        public static void sendMPString(string address, string message)
        {
            sendMPString(new MPStringMessage(address, Game1.player, message));
        }

        public static void sendMPString(MPStringMessage msg)
        {
            if (Game1.IsServer)
                foreach (long key in Game1.otherFarmers.Keys)
                {
                    if (key != msg.sender.UniqueMultiplayerID)
                        Game1.server.sendMessage(key, 99, msg.sender, msg.address, (Int16) 0, msg.message);
                }
            else
                Game1.client.sendMessage(new OutgoingMessage(99, msg.sender, msg.address, (Int16) 0, msg.message));
        }

        public static void receiveMPString(IncomingMessage inc)
        {
            string address = inc.Reader.ReadString();
            inc.Reader.ReadInt16();
            string message = inc.Reader.ReadString();

            if (!messages.ContainsKey(address))
                messages.Add(address, new List<MPStringMessage>());

            messages[address].Add(new MPStringMessage(address,inc.SourceFarmer,message));
        }

        public static List<MPStringMessage> getMPStringMessages(string address)
        {
            if (!messages.ContainsKey(address))
                messages.Add(address, new List<MPStringMessage>());

            return messages[address];
        }

        public static List<MPStringMessage> getNewMPStringMessages(string address)
        {
            List<MPStringMessage> msgs = new List<MPStringMessage>(getMPStringMessages(address));
            messages[address].Clear();
            return msgs;
        }
    }
}
