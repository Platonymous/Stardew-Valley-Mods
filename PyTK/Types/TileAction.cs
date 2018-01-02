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
        Func<List<string>, SFarmer, Location, bool> action;
        string trigger;

        public TileAction(string trigger, Func<List<string>, SFarmer, Location, bool> action)
        {
            this.trigger = trigger;
            this.action = action;
        }

        public void register()
        {
            OvLocations.actions.AddOrReplace(trigger, action);
        }


    }
}
