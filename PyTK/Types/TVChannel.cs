using StardewValley;
using System;
using StardewValley.Objects;
using SFarmer = StardewValley.Farmer;
using PyTK.CustomTV;

namespace PyTK.Types
{
    public class TVChannel
    {
        public TemporaryAnimatedSprite sprite;
        public TemporaryAnimatedSprite overlay ;
        public string text;
        public string id;
        public Action<TV, TemporaryAnimatedSprite, SFarmer, string> action;
        public Action afterDialogues;

        public TVChannel(string id, string text, Action<TV, TemporaryAnimatedSprite, SFarmer, string> action = null, TemporaryAnimatedSprite sprite = null, Action afterDialogues = null, TemporaryAnimatedSprite overlay = null)
        {
            this.sprite = sprite;
            this.overlay = overlay;
            this.text = text;
            this.id = id;
            this.action = (action == null) ? (tv, ta, sf, s) => CustomTVMod.showProgram(this) : action;
            this.afterDialogues = (afterDialogues == null) ? endProgram : afterDialogues;
        }

        public static void endProgram()
        {
            CustomTVMod.endProgram();
        }

        public void register()
        {
            CustomTVMod.addChannel(this);
        }

    }
}
