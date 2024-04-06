using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using xTile;

namespace TMXLoader
{
    public class ContentSyncHandler
    {
        internal static IPyResponder contentResponder;
        internal static IPyResponder contentReceiver;
        internal static Dictionary<string,object> content;
        internal static Dictionary<Farmer,List<string>> contentPipe;
        internal static IModHelper Helper { get; } = TMXLoaderMod.helper;
        internal static string responderName = "PyTK.ContentResponder";
        internal static string receiverName = "PyTK.ContentReceiver";
        internal const int dTimeout = 3000;


        internal static void initialize()
        {
            content = new Dictionary<string, object>();
            contentPipe = new Dictionary<Farmer, List<string>>();
            contentResponder = new PyResponder<ContentResponse, ContentRequest>(responderName, getSerializedContent, 8, SerializationType.JSON, SerializationType.JSON);
            contentReceiver = new PyResponder<bool, ContentResponse>(receiverName, receiveInstruction, 16, SerializationType.JSON, SerializationType.JSON);
            contentResponder.start();
            contentReceiver.start();
        }

        internal static void requestContent<T>(string assetName, Farmer fromFarmer, Action<T> callback, int timeout = dTimeout)
        {
            ContentType? type = getContentType<T>();
            if (type.HasValue)
                getResponse(assetName, type.Value, false, fromFarmer, callback, deserialize<T>, timeout);
            else
                TMXLoaderMod.monitor.Log("ContentRequest Failed: Type (" + typeof(T).ToString() + ") not supported for " + assetName);
        }

        internal static void requestGameContent<T>(string assetName, Farmer fromFarmer, Action<T> callback, int timeout = dTimeout)
        {
            ContentType? type = getContentType<T>();
            if (type.HasValue)
                getResponse(assetName, type.Value, true, fromFarmer, callback, deserialize<T>, timeout);
            else
                TMXLoaderMod.monitor.Log("ContentRequest Failed: Type (" + typeof(T).ToString() + ") not supported for " + assetName);
        }

        internal static void sendContent<T>(string assetName, T asset, Farmer toFarmer, Action<bool> callback, int timeout = dTimeout)
        {
            ContentType? type = getContentType<T>();
            if (type.HasValue)
                sendInstruction(assetName, asset, type.Value, false, toFarmer, callback, timeout);
            else
                TMXLoaderMod.monitor.Log("ContentRequest Failed: Type (" + typeof(T).ToString() + ") not supported for " + assetName);
        }

        internal static void sendGameContent<T>(string assetName, T asset, Farmer toFarmer, Action<bool> callback, int timeout = dTimeout)
        {
            ContentType? type = getContentType<T>();
            if (type.HasValue)
                sendInstruction(assetName, asset, type.Value, true, toFarmer, callback, timeout);
            else
                TMXLoaderMod.monitor.Log("ContentRequest Failed: Type (" + typeof(T).ToString() + ") not supported for " + assetName);
        }

        internal static void sendGameContent<T>(string[] assetName, T asset, Farmer toFarmer, Action<bool> callback, int timeout = dTimeout)
        {
            ContentType? type = getContentType<T>();
            if (type.HasValue)
                sendInstruction(assetName, asset, type.Value, true, toFarmer, callback, timeout);
            else
                TMXLoaderMod.monitor.Log("ContentRequest Failed: Type (" + typeof(T).ToString() + ") not supported for " + assetName[0]);
        }

        private static void sendInstruction<T>(string assetName, T asset, ContentType type, bool toGameContent, Farmer farmer, Action<bool> callback, int timeout)
        {
            if (contentPipe.ContainsKey(farmer) && contentPipe[farmer].Contains(assetName))
                return;

            if (contentPipe.ContainsKey(farmer))
            {
                contentPipe[farmer].Remove(assetName);
                contentPipe[farmer].Add(assetName);
            }
            else
                contentPipe.Add(farmer, new List<string>() { assetName });

            Task.Run(async () =>
           {
               await PyNet.sendRequestToFarmer<bool>(receiverName, new ContentResponse(assetName, (int)type, serialize(asset, type), toGameContent), farmer, (r) =>
                {
                    if (!r)
                        callback(false);
                    else
                        callback(r);
                }, SerializationType.JSON, timeout);
           });
        }

        private static void sendInstruction<T>(string[] assetName, T asset, ContentType type, bool toGameContent, Farmer farmer, Action<bool> callback, int timeout)
        {
            foreach (string assetKey in assetName)
                sendInstruction(assetKey, asset, type, toGameContent, farmer, callback, timeout);
        }

        private static bool receiveInstruction(ContentResponse instruction)
        {
            if (contentPipe.ContainsKey(Game1.player) && contentPipe[Game1.player].Contains(instruction.assetName))
                return true;

            if (contentPipe.ContainsKey(Game1.player))
            {
                contentPipe[Game1.player].Remove(instruction.assetName);
                contentPipe[Game1.player].Add(instruction.assetName);
            }
            else
                contentPipe.Add(Game1.player, new List<string>() { instruction.assetName });

            TMXLoaderMod.monitor.Log("Receiving Content: " + instruction.assetName + " (" + (instruction.toGameContent ? "Game Content" : "Content") + ")");
            try
            {
                if (instruction.toGameContent)
                    incjectToGameContent(instruction);
                else
                {
                    content.Remove(instruction.assetName);
                    content.Add(instruction.assetName, deserialize<object>(instruction));
                }
            }
            catch (Exception e)
            {
                TMXLoaderMod.monitor.Log(e.Message + ":" + e.StackTrace);
                return false;
            }

            return true;
        }

        private static void incjectToGameContent(ContentResponse instruction)
        {
            if (instruction.type == (int) ContentType.DictInt)
            {
                SerializableDictionary<int, string> asset = deserialize<SerializableDictionary<int, string>>(instruction);
                SerializableDictionary<int, string> content = null;
                try
                {
                    content = Helper.GameContent.Load<SerializableDictionary<int, string>>(instruction.assetName);
                }
                catch
                {

                }

                if (content == null)
                    asset.inject(instruction.assetName);
                else
                    asset.injectInto(instruction.assetName);
            }

            if (instruction.type == (int)ContentType.DictString)
            {
                SerializableDictionary<string, string> asset = deserialize<SerializableDictionary<string, string>>(instruction);

                SerializableDictionary<string, string> content = null;
                try
                {
                    content = Helper.GameContent.Load<SerializableDictionary<string, string>>(instruction.assetName);
                }
                catch
                {

                }

                if (content == null)
                    asset.inject(instruction.assetName);
                else
                    asset.injectInto(instruction.assetName);

            }

            if (instruction.type == (int)ContentType.Texture)
            {
                Texture2D asset = deserialize<Texture2D>(instruction);
                Texture2D content = null;
                try
                {
                    content = Helper.GameContent.Load<Texture2D>(instruction.assetName);
                }
                catch
                {

                }

                if (content == null)
                    asset.inject(instruction.assetName);
                else
                    asset.injectAs(instruction.assetName);
            }

            if (instruction.type == (int)ContentType.Map)
            {
                Map asset = deserialize<Map>(instruction);
                Map content = null;
                try
                {
                    content = Helper.GameContent.Load<Map>(instruction.assetName);
                }
                catch
                {

                }

                if(content == null)
                    asset.inject(instruction.assetName);
                else
                    asset.injectAs(instruction.assetName);
            }
        }

        private static void incjectToGameContent(ContentResponse instruction, string[] assetNames)
        {
            foreach (string asset in assetNames)
                incjectToGameContent(instruction);
        }

        public static void addContent<T>(string assetName, T contentAsset)
        {
            content.Remove(assetName);
            content.Add(assetName, contentAsset);
        }

        public static void removeContent<T>(string assetName, T contentAsset)
        {
            if(content.ContainsKey(assetName))
                content.Remove(assetName);
        }

        public static T getContent<T>(string assetName)
        {
            if (content.ContainsKey(assetName) && content[assetName] is T asset)
                return asset;
            else
                return (T)(object)null;
        }

        public static T deserialize<T>(ContentResponse response)
        {
            if (response.type == (int)ContentType.DictInt)
            {
                SerializableDictionary<int, string> newAsset = new SerializableDictionary<int, string>();
                StringReader reader = new StringReader(PyNet.DecompressString(response.content));
                newAsset.ReadXml(XmlReader.Create(reader));
                return (T) (object) newAsset;
            }

            if (response.type == (int)ContentType.DictString)
            {
                SerializableDictionary<string, string> newAsset = new SerializableDictionary<string, string>();
                StringReader reader = new StringReader(PyNet.DecompressString(response.content));
                newAsset.ReadXml(XmlReader.Create(reader));
                return (T)(object)newAsset;
            }

            if (response.type == (int) ContentType.Texture)
            {
                    SerializationTexture2D sTexture = JsonConvert.DeserializeObject<SerializationTexture2D>(PyNet.DecompressString(response.content));
                    return (T) (object) sTexture.getTexture();
            }

            if (response.type == (int)ContentType.Map)
            {
                TMXTile.TMXFormat format = new TMXTile.TMXFormat(Game1.tileSize / Game1.pixelZoom, Game1.tileSize / Game1.pixelZoom, Game1.pixelZoom, Game1.pixelZoom);
                StringReader reader = new StringReader(PyNet.DecompressString(response.content));
                Map map = format.Load(XmlReader.Create(reader));
                return (T)(object)map;
            }

            return (T) (object) null;
        }

        private static ContentType? getContentType<T>()
        {
            if (typeof(T) == typeof(SerializableDictionary<string, string>))
                return ContentType.DictString;

            if (typeof(T) == typeof(SerializableDictionary<int, string>))
                return ContentType.DictInt;

            if (typeof(T) == typeof(Texture2D))
                return ContentType.Texture;

            if (typeof(T) == typeof(Map))
                return ContentType.Map;

            return null;
        }

        private static void getResponse<T>(string assetName, ContentType type, bool fromGameContent, Farmer farmer, Action<T> callback, Func<ContentResponse, T> deserializer, int timeout)
        {       
            Task.Run(async () =>
           {
               await PyNet.sendRequestToFarmer<ContentResponse>(responderName, new ContentRequest(type, assetName, fromGameContent), farmer, (r) =>
               {
                   if (r.content == "na")
                   {
                       TMXLoaderMod.monitor.Log("ContentRequest Failed: Could not obtain asset: " + r.assetName);
                       callback((T)(object)null);
                   }
                   else
                       callback(deserializer(r));
               }, SerializationType.JSON, timeout);
           });
        }       

        public static string serialize(object contents, ContentType type)
        {
            if(type == ContentType.DictInt)
            {
                StringWriter writer = new StringWriter();
                (contents as SerializableDictionary<int,string>).WriteXml(XmlWriter.Create(writer));
                return PyNet.CompressString(writer.ToString());
            }

            if(type == ContentType.DictString)
            {
                StringWriter writer = new StringWriter();
                (contents as SerializableDictionary<string, string>).WriteXml(XmlWriter.Create(writer));
                return PyNet.CompressString(writer.ToString());
            }

            if (type == ContentType.Texture)
                return PyNet.CompressString(JsonConvert.SerializeObject(new SerializationTexture2D((contents as Texture2D))));

            if(type == ContentType.Map)
            {
                var format = new TMXTile.TMXFormat(Game1.tileSize / Game1.pixelZoom,Game1.tileSize / Game1.pixelZoom,Game1.pixelZoom,Game1.pixelZoom);
                return PyNet.CompressString(format.StoreAsString((contents as Map)));
            }

            return "na";
        }

        private static ContentResponse getSerializedContent(ContentRequest contentRequest)
        {
            bool game = contentRequest.fromGameContent;
            string result = "na";

            if (!game && !content.ContainsKey(contentRequest.assetName))
                return new ContentResponse(contentRequest.assetName, contentRequest.type, result, contentRequest.fromGameContent);
  
            try
            {
                object asset = null;
                if (contentRequest.type == (int) ContentType.DictInt)
                    asset = game ? Helper.GameContent.Load<SerializableDictionary<int, string>>(contentRequest.assetName) : (SerializableDictionary<int, string>)content[contentRequest.assetName];

                if (contentRequest.type == (int)ContentType.DictString)
                   asset = game ? Helper.GameContent.Load<SerializableDictionary<string, string>>(contentRequest.assetName) : (SerializableDictionary<string, string>)content[contentRequest.assetName];

                if (contentRequest.type == (int)ContentType.Texture)
                    asset = game ? Helper.GameContent.Load<Texture2D>(contentRequest.assetName) : (Texture2D)content[contentRequest.assetName];

                if (contentRequest.type == (int)ContentType.Map)
                    asset = game ? Helper.GameContent.Load<Map>(contentRequest.assetName) : (Map)content[contentRequest.assetName];

                result = serialize(asset, (ContentType)contentRequest.type);
            }
            catch
            {
                result = "na";
            }

            return new ContentResponse(contentRequest.assetName, contentRequest.type, result, game);
        }



    }



}
