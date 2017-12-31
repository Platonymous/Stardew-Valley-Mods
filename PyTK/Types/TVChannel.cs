using StardewValley;
using StardewValley.Locations;
using System;
using PyTK.Extensions;
using StardewValley.Objects;
using SFarmer = StardewValley.Farmer;
using PyTK.CustomTV;

namespace PyTK.Types
{
    public class TVChannel
    {
        public TemporaryAnimatedSprite sprite = null;
        public TemporaryAnimatedSprite overlay = null;
        public string text;
        public string id;
        public Action<TV, TemporaryAnimatedSprite, SFarmer, string> action = null;
        public Action afterDialogues = null;

        public TVChannel(string id, string text,Action<TV, TemporaryAnimatedSprite, SFarmer, string> action, TemporaryAnimatedSprite sprite = null, Action afterDialogues = null, TemporaryAnimatedSprite overlay = null)
        {
            this.sprite = sprite;
            this.overlay = overlay;
            this.text = text;
            this.id = id;
            this.action = action;

            if (afterDialogues == null)
                afterDialogues = endProgram;

            this.afterDialogues = afterDialogues;
        }

        public TVChannel(string id, string text, Action action = null, TemporaryAnimatedSprite sprite = null, Action afterDialogues = null, TemporaryAnimatedSprite overlay = null)
        {
            this.sprite = sprite;
            this.overlay = overlay;
            this.text = text;
            this.id = id;

            if (afterDialogues == null)
                afterDialogues = endProgram;
            this.afterDialogues = afterDialogues;

            if (action != null)
                this.action = (a, b, c, d) => action();
            
        }

        public static void endProgram()
        {
            CustomTVMod.endProgram();
        }


    }
}
