using System;
using System.Collections.Generic;
using xTile.Dimensions;
using SFarmer = StardewValley.Farmer;
using PyTK.Overrides;
using PyTK.Extensions;

namespace PyTK.Types
{
    public class TileAction
    {
        public Func<List<string>, bool> action;
        public string trigger;

        public TileAction(string trigger, Func<List<string>, bool> action)
        {
            this.trigger = trigger;
            this.action = action;
        }

        public TileAction(string trigger, Action<List<string>> action)
        {
            this.trigger = trigger;
            this.action = delegate(List<string> s) { action.Invoke(s);  return true; };
        }

        public TileAction(string trigger, Action action)
        {
            this.trigger = trigger;
            this.action = delegate (List<string> s) { action.Invoke(); return true; };
        }

        public TileAction register()
        {
            OvLocations.actions.AddOrReplace(trigger, action);
            return this;
        }


    }
}
