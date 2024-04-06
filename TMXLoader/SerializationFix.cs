using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Xml;
using System.Xml.Serialization;
using xTile.ObjectModel;

namespace TMXLoader
{
    internal static class SerializationFix
    {
        // This list must be kept in sync with StardewValley.SaveGame.locationSerializer.
        public static Type[] ExtraTypes = new Type[3]
        {
            typeof(Character),
            typeof(Item),
            typeof(TerrainFeature)
        };

        public static void SafeSerialize(XmlWriter writer, GameLocation location)
        {
            Type baseType = typeof(GameLocation);
            Type customType = location.GetType();

            // No need to fix types from the base game.
            if (object.ReferenceEquals(customType.Assembly, baseType.Assembly))
            {
                SaveGame.locationSerializer.Serialize(writer, location);
                return;
            }

            var xmlOverrides = new XmlAttributeOverrides();

            XmlSerializer serializer = new XmlSerializer(customType, xmlOverrides, ExtraTypes, null, null);
            serializer.Serialize(writer, location);
        }

        public static object SafeDeSerialize(XmlReader reader, GameLocation location)
        {
            Type baseType = typeof(GameLocation);
            Type customType = location.GetType();

            var xmlOverrides = new XmlAttributeOverrides();
            XmlSerializer serializer = new XmlSerializer(customType, xmlOverrides, ExtraTypes, null, null);
            return serializer.Deserialize(reader);
        }
    }
}
