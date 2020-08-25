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
        public static Type[] ExtraTypes = new Type[24]
        {
            typeof(Tool),
            typeof(Duggy),
            typeof(Ghost),
            typeof(GreenSlime),
            typeof(LavaCrab),
            typeof(RockCrab),
            typeof(ShadowGuy),
            typeof(Child),
            typeof(Pet),
            typeof(Dog),
            typeof(Cat),
            typeof(Horse),
            typeof(SquidKid),
            typeof(Grub),
            typeof(Fly),
            typeof(DustSpirit),
            typeof(Bug),
            typeof(BigSlime),
            typeof(BreakableContainer),
            typeof(MetalHead),
            typeof(ShadowGirl),
            typeof(Monster),
            typeof(JunimoHarvester),
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

            bool IsUnsafe = location is BuildableGameLocation || location is IAnimalLocation;

            var xmlOverrides = new XmlAttributeOverrides();

            if (!IsUnsafe)
            {
                // Move the base type out of the way to make its name available.
                var baseAttribs = new XmlAttributes();
                baseAttribs.XmlType = new XmlTypeAttribute("GameLocation1");
                xmlOverrides.Add(baseType, baseAttribs);

                // Make sure the custom type saves with the base name so it will deserialize correctly.
                var customAttribs = new XmlAttributes();
                customAttribs.XmlType = new XmlTypeAttribute("GameLocation");
                xmlOverrides.Add(customType, customAttribs);
            }

            XmlSerializer serializer = new XmlSerializer(customType, xmlOverrides, ExtraTypes, null, null);
            serializer.Serialize(writer, location);
        }

        public static object SafeDeSerialize(XmlReader reader, GameLocation location)
        {
            Type baseType = typeof(GameLocation);
            Type customType = location.GetType();

            bool IsUnsafe = location is BuildableGameLocation || location is IAnimalLocation || location is DecoratableLocation;

            // No need to fix types from the base game.
            if (!IsUnsafe || object.ReferenceEquals(customType.Assembly, baseType.Assembly))
                return SaveGame.locationSerializer.Deserialize(reader);

            var xmlOverrides = new XmlAttributeOverrides();
            XmlSerializer serializer = new XmlSerializer(customType, xmlOverrides, ExtraTypes, null, null);
            return serializer.Deserialize(reader);
        }
    }
}
