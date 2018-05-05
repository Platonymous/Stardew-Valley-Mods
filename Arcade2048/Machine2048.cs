using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.Objects;

namespace Arcade2048
{
    class Machine2048 : PySObject
    {
        public Machine2048()
        {

        }

        public Machine2048(CustomObjectData data)
            : base(data, Vector2.Zero)
        {
        }

        public Machine2048(CustomObjectData data, Vector2 tileLocation)
            : base(data, tileLocation)
        {
        }

        public override bool checkForAction(StardewValley.Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity)
                return true;
            Game1.currentMinigame = new Game2048();
            return true;
        }

        public override Item getOne()
        {
            return new Machine2048(data) { TileLocation = Vector2.Zero };
        }

        public override ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            CustomObjectData data = CustomObjectData.collection[additionalSaveData["id"]];
            return new Machine2048(CustomObjectData.collection[additionalSaveData["id"]], (replacement as Chest).TileLocation);
        }


    }
}
