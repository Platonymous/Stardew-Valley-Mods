using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StardewValley.Locations;
using Microsoft.Xna.Framework;

namespace CustomWallsAndFloorsRedux
{
    public static class FHRHandler
    {
        public static List<int> GetConnectedWalls(FarmHouse farmhouse, int index, bool floor = false)
        {
            List<int> results = new List<int>();
            string field = floor ? "floorDictionary" : "wallDictionary";
            try
            {
                var fhs = Type.GetType("FarmHouseRedone.FarmHouseStates,FarmHouseRedone");

                if (fhs != null)
                {
                    object state = fhs.GetMethod("getState", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { farmhouse });
                    Dictionary<Rectangle, string> roomDictionary = (Dictionary<Rectangle, string>)state.GetType().GetField(field, BindingFlags.Instance | BindingFlags.Public).GetValue(state);
                    var frooms = floor ? farmhouse.getFloors() : farmhouse.getWalls();
                    if (roomDictionary.ContainsKey(frooms[index]) && roomDictionary[frooms[index]] is string room)
                        foreach (var data in roomDictionary.Where(d => d.Value == room && frooms.IndexOf(d.Key) != index))
                            results.Add(frooms.IndexOf(data.Key));
                }
            }
            catch (Exception e)
            {

            }

            return results;
        }

    }
}
