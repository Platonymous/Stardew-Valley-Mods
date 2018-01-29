using System;
using System.Collections.Generic;
using xTile.Dimensions;
using SFarmer = StardewValley.Farmer;
using PyTK.Overrides;
using PyTK.Extensions;
using Microsoft.Xna.Framework;
using StardewValley;

namespace PyTK.Types
{
    public class TileAction
    {
        public Func<string, GameLocation, Vector2, string, bool> action;
        internal static Dictionary<string, TileAction> actions = new Dictionary<string, TileAction>();
        public string trigger;
        public string currentAction = "";

        public TileAction(string trigger, Func<string, GameLocation, Vector2, string, bool> action)
        {
            this.trigger = trigger;
            this.action = action;
        }

        public TileAction register()
        {
            actions.AddOrReplace(trigger, this);
            return this;
        }

        public static TileAction getCustomAction(string action, string conditions = "", string fallback = "")
        {
            if (action == null || action == "")
                return null;

            TileAction result = null;

            string[] prop = action.Split(' ');
            if (actions.ContainsKey(prop[0]))
                if (PyUtils.checkEventConditions(conditions)) {
                    result = actions[prop[0]];
                    result.currentAction = action;
                }
                else
                    result = getCustomAction(fallback);

            return result;
        }

        public static bool invokeCustomTileActions(string key, GameLocation location, Vector2 tile, string layer)
        {
            bool standartAction = (key == "Action" || key == "TouchAction");
            string conditions = standartAction ? location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Conditions", layer) : "";
            string fallback = standartAction ? location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Fallback", layer) : "";

            PyTKMod._monitor.Log("InvokeTileAction:" + key);
            if (location.doesTileHaveProperty((int)tile.X, (int)tile.Y, key, layer) is string actionString)
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
                        result = customAction.action(customAction.currentAction, location, tile, layer) && result;
                }
                return result;
            }

            return false;
        }
    }
}
