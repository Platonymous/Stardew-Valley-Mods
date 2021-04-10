using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.Objects;

namespace ArcadeParachute
{
    class MachineParachute : PySObject
    {
        public MachineParachute()
        {

        }

        public MachineParachute(CustomObjectData data)
            : base(data, Vector2.Zero)
        {
        }

        public MachineParachute(CustomObjectData data, Vector2 tileLocation)
            : base(data, tileLocation)
        {
        }

        public override bool checkForAction(StardewValley.Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity)
                return true;
            Game1.currentMinigame = new GameParachute();
            return true;
        }

        public override Item getOne()
        {
            return new MachineParachute(data) { TileLocation = Vector2.Zero };
        }

        public override ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            CustomObjectData data = CustomObjectData.collection[additionalSaveData["id"]];
            return new MachineParachute(CustomObjectData.collection[additionalSaveData["id"]], (replacement as Chest).TileLocation);
        }


    }
}
