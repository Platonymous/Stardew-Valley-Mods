using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace TMXLoader
{
    internal static class SerializationFix
    {
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

            if (object.ReferenceEquals(customType.Assembly, baseType.Assembly))
            {
                SaveGame.locationSerializer.Serialize(writer, location);
                return;
            }

            var xmlOverrides = new XmlAttributeOverrides();

            var baseAttribs = new XmlAttributes();   
            baseAttribs.XmlType = new XmlTypeAttribute("GameLocation1");
            xmlOverrides.Add(baseType, baseAttribs);

            var customAttribs = new XmlAttributes();   
            customAttribs.XmlType = new XmlTypeAttribute("GameLocation");
            xmlOverrides.Add(customType, customAttribs);

            XmlSerializer serializer = new XmlSerializer(customType, xmlOverrides, ExtraTypes, null, null);
            serializer.Serialize(writer, location);
        }
    }
}
