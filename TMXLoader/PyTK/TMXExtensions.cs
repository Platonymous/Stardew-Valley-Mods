using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMXTile;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

using SObject = StardewValley.Object;

namespace TMXLoader
{
    internal static class TMXExtensions
    {
        public static int getIndexByName(this IDictionary<int, string> dictionary, string name)
        {
            int found = 0;

            if (name.StartsWith("startswith:"))
                found = (dictionary.Where(d => d.Value.Split('/')[0].StartsWith(name.Split(':')[1])).FirstOrDefault()).Key;
            else if (name.StartsWith("endswith:"))
                found = (dictionary.Where(d => d.Value.Split('/')[0].EndsWith(name.Split(':')[1])).FirstOrDefault()).Key;
            else if (name.StartsWith("contains:"))
                found = (dictionary.Where(d => d.Value.Split('/')[0].Contains(name.Split(':')[1])).FirstOrDefault()).Key;
            else
                found = (dictionary.Where(d => d.Value.Split('/')[0] == name).FirstOrDefault()).Key;

            return found;
        }

        public static string getIndexByName(this IDictionary<string, string> dictionary, string name)
        {
            string found = "-1";

            if (name.StartsWith("startswith:"))
                found = (dictionary.Where(d => d.Value.Split('/')[0].StartsWith(name.Split(':')[1])).FirstOrDefault()).Key;
            else if (name.StartsWith("endswith:"))
                found = (dictionary.Where(d => d.Value.Split('/')[0].EndsWith(name.Split(':')[1])).FirstOrDefault()).Key;
            else if (name.StartsWith("contains:"))
                found = (dictionary.Where(d => d.Value.Split('/')[0].Contains(name.Split(':')[1])).FirstOrDefault()).Key;
            else
                found = (dictionary.Where(d => d.Value.Split('/')[0] == name).FirstOrDefault()).Key;

            return found;
        }


        public static T sObjectOnMap<T>(this Vector2 t) where T : SObject
        {
            if (Game1.currentLocation is GameLocation location)
            {
                if (location.netObjects.FieldDict.TryGetValue(t, out NetRef<SObject> netRaw) && netRaw.Value is T netValue)
                    return netValue;
                if (location.overlayObjects.TryGetValue(t, out SObject overlayRaw) && overlayRaw is T overlayValue)
                    return overlayValue;
            }
            return null;
        }

    }

    public class TileAction
    {
        public Func<string, string, GameLocation, Vector2, string, bool> action;
        internal static Dictionary<string, TileAction> actions = new Dictionary<string, TileAction>();
        public string trigger;
        public string currentAction = "";

        public TileAction(string trigger, Func<string, GameLocation, Vector2, string, bool> action)
        {
            this.trigger = trigger;
            this.action = (key, values, location, tile, layer) =>
            {
                return action.Invoke(key + " " + values, location, tile, layer);
            };
        }

        public TileAction(string trigger, Func<string, string, GameLocation, Vector2, string, bool> action)
        {
            this.trigger = trigger;
            this.action = action;
        }

        public TileAction register()
        {
            actions.Remove(trigger);
            actions.Add(trigger, this);
            return this;
        }

        public static TileAction getCustomAction(string action, string conditions = "", string fallback = "")
        {
            if (action == null || action == "")
                return null;

            TileAction result = null;

            string[] prop = action.Split(' ');
            if (actions.ContainsKey(prop[0]))
                if (PyUtils.checkEventConditions(conditions))
                {
                    result = actions[prop[0]];
                    result.currentAction = action;
                }
                else
                    result = getCustomAction(fallback);

            return result;
        }

        private static bool invokeAction(string actionString, GameLocation location, Vector2 tile, string layer, string conditions = "", string fallback = "")
        {
            bool result = true;
            string[] tileActions = actionString.Split(';');

            foreach (string action in tileActions)
            {

                string nextAction = action.Replace(" § ", "§");
                if (nextAction.Contains("§"))
                {
                    string[] data = action.Split('§');
                    string actionConditions = data[0];
                    string successAction = data[1];
                    string failAction = "---";

                    if (data.Length > 2)
                        failAction = data[2];

                    if (PyUtils.checkEventConditions(actionConditions))
                        nextAction = successAction;
                    else
                        nextAction = failAction;
                }

                if (getCustomAction(nextAction, conditions, fallback) is TileAction customAction)
                {
                    List<string> text = new List<string>(customAction.currentAction.Split(' '));
                    string key = text[0];
                    text.RemoveAt(0);

                    result = customAction.action(key, String.Join(" ", text), location, tile, layer) && result;
                }
            }
            return result;
        }

        public static bool invokeCustomTileActions(string key, GameLocation location, Vector2 tile, string layer)
        {
            tile = new Vector2((int)tile.X, (int)tile.Y);

            bool standartAction = (key == "Action" || key == "TouchAction");
            bool mapAction = (layer == "Map");
            string conditions = standartAction ? location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Conditions", layer) : mapAction ? location.map.Properties.ContainsKey("Conditions") ? location.map.Properties["Conditions"].ToString() : "" : "";
            string fallback = standartAction ? location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Fallback", layer) : mapAction ? location.map.Properties.ContainsKey("Fallback") ? location.map.Properties["Fallback"].ToString() : "" : "";

            if (!mapAction && location.doesTileHaveProperty((int)tile.X, (int)tile.Y, key, layer) is string actionString)
                return invokeAction(actionString, location, tile, layer, conditions, fallback);
            else if (mapAction && location.map.Properties.ContainsKey(key))
                return invokeAction(location.map.Properties[key].ToString(), location, tile, layer, conditions, fallback);
            return false;
        }
    }
}
