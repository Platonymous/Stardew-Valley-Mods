using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace TMXLoader
{
    public interface ITMXLAPI
    {
        void AddContentPack(IContentPack pack);

        event EventHandler<GameLocation> OnLocationRestoring;

        string BuildBuildable(string id, GameLocation location, Point position);

        void RemoveBuildable(string uniqueid);

        void MoveBuildable(string uniqueid, GameLocation location, Point position);

        bool TryGetSaveDataForLocation(GameLocation location, out GameLocation saved);
    }

    public class TMXLAPI : ITMXLAPI
    {
        protected static event EventHandler<GameLocation> OnLocationRestoringEvent;
        
        public event EventHandler<GameLocation> OnLocationRestoring
        {
            add
            {
                OnLocationRestoringEvent += value;
            }
            remove
            {
                OnLocationRestoringEvent -= value;
            }
        }

        internal static void RaiseOnLocationRestoringEvent(GameLocation inGame)
        {
            OnLocationRestoringEvent?.Invoke(null, inGame);
        }

        public void AddContentPack(IContentPack pack)
        {
            if (TMXLoaderMod.AddedContentPacks.Contains(pack))
                return;

            TMXLoaderMod.AddedContentPacks.Add(pack);

            if (TMXLoaderMod.contentPacksLoaded)
                TMXLoaderMod._instance.loadPack(pack,"content");
        }

        public string BuildBuildable(string id, GameLocation location, Point position)
        {
            if(TMXLoaderMod.buildables.FirstOrDefault(be => be.id == id) is BuildableEdit b)
            {
                string uid = ((ulong)TMXLoaderMod._instance.Helper.Multiplayer.GetNewID()).ToString();
                TMXLoaderMod._instance.buildBuildableEdit(false, b, location, position, new System.Collections.Generic.Dictionary<string, string>(), uniqueId: uid);
                return uid;
            }

            return null;
        }

        public void RemoveBuildable(string uniqueid)
        {
            if (TMXLoaderMod.buildablesBuild.FirstOrDefault(bb => bb.UniqueId != uniqueid) is SaveBuildable sb)
                TMXLoaderMod._instance.removeSavedBuildable(sb, false, true);
        }

        public void MoveBuildable(string uniqueid, GameLocation location, Point position)
        {
            if (TMXLoaderMod.buildablesBuild.FirstOrDefault(bb => bb.UniqueId != uniqueid) is SaveBuildable sb &&
                TMXLoaderMod.buildables.FirstOrDefault(be => be.id == sb.Id) is BuildableEdit b)
            {
                SaveLocation sl = null;
                if (b.indoorsFile != null && Game1.getLocationFromName(TMXLoaderMod._instance.getLocationName(uniqueid)) is GameLocation sblocation && TMXLoaderMod._instance.getLocationSaveData(sblocation) is SaveLocation savd)
                    sl = savd;

                RemoveBuildable(uniqueid);
                TMXLoaderMod._instance.buildBuildableEdit(false, b, location, position, new Dictionary<string, string>(), uniqueId: uniqueid);

                TMXLoaderMod._instance.setLocationObjects(sl);                
            }
        }

        public bool TryGetSaveDataForLocation(GameLocation location, out GameLocation saved)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Auto;
            saved = location;
            var saveData = TMXLoaderMod._instance.Helper.Data.ReadSaveData<SaveData>("Locations");
            if (saveData.Locations.FirstOrDefault(l => l.Name == location.Name) is SaveLocation loc)
            {
                StringReader objReader = new StringReader(loc.Objects);

                using (var reader = XmlReader.Create(objReader, settings))
                {
                    try
                    {
                        saved = (GameLocation)SerializationFix.SafeDeSerialize(reader, location);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
        }
    }
}
