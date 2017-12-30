using PyTK.Extensions;
using StardewValley;
using StardewModdingAPI;
using System;
using StardewModdingAPI.Events;
using System.Collections.Generic;
using StardewValley.Locations;
using StardewValley.Buildings;
using SObject = StardewValley.Object;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace PyTK
{
    public static class PyUtils
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        public static bool CheckEventConditions(string conditions)
        {
            return Helper.Reflection.GetMethod(Game1.currentLocation, "checkEventPrecondition").Invoke<int>(new object[] { "9999999/" + conditions }) != -1;
        }

        public static List<GameLocation> getAllLocationsAndBuidlings()
        {
            List<GameLocation> list = Game1.locations;

            for(int i = 0; i < Game1.locations.Count; i++)
                if (Game1.locations[i] is BuildableGameLocation bgl)
                    for (int j = 0; j < bgl.buildings.Count; j++)
                        if(bgl.buildings[j].indoors != null)
                            list.Add(bgl.buildings[j].indoors);

            return list;
        }

        public static IEnumerable<FieldInfo> getAllPublicFileds(object obj)
        {
                Type type = obj.GetType();
                foreach (var f in type.GetFields().Where(f => f.IsPublic))
                yield return f;
        }

        public static IEnumerable<FieldInfo> getAllPublicFileds<T>(object obj)
        {
            foreach (var f in getAllPublicFileds(obj))
                if (f.GetType() == typeof(T))
                    yield return f;
        }

        public static IEnumerable<FieldInfo> getAllPrivateFileds(object obj)
        {
            Type type = obj.GetType();
            foreach (var f in type.GetFields().Where(f => f.IsPrivate))
                yield return f;
        }

        public static IEnumerable<FieldInfo> getAllPrivateFileds<T>(object obj)
        {
            foreach (var f in getAllPrivateFileds(obj))
                if (f.GetType() == typeof(T))
                    yield return f;
        }

        public static void replaceAllPublicObjects<TIn>(Func<TIn,object> replacer)
        {
            /*
            foreach (GameLocation l in getAllLocationsAndBuidlings())
            {
                Dictionary<int, KeyValuePair<FieldInfo, object>> allFields = new Dictionary<int, KeyValuePair<FieldInfo, object>>();

                List<KeyValuePair<Vector2, SObject>> objects = new List<KeyValuePair<Vector2, SObject>>(l.objects.toList(k => k));
                for(int o = 0; o < objects.Count; o++)
                {
                    List<FieldInfo> allFields = new List<FieldInfo>(getAllPublicFileds(objects[o].Value));
                    for (int i = 0; i < allFields.Count(); i++)
                        if (allFields[i].GetValue(objects[o].Value) is TIn ov)
                            allFields[i].SetValue(l.objects[objects[o].Key], replacer(ov));
                        else if (allFields[i].GetValue(objects[o].Value) is List<Item> il)
                            for (int j = 0; j < il.Count; j++)
                                if (il[j] is TIn)
                                    il[j] = (Item) replacer((TIn) (object) il[j]);   

                    if (objects[o] is TIn)
                        l.objects[objects[o].Key] = (SObject) replacer((TIn) (object) objects[o]);
                }

            }
                */
        }

    }
}
