using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.Objects;

namespace Snake
{
    class SnakeMachine : PySObject
    {
        public SnakeMachine()
        {

        }

        public SnakeMachine(CustomObjectData data)
            : base(data, Vector2.Zero)
        {
        }

        public SnakeMachine(CustomObjectData data, Vector2 tileLocation)
            : base(data, tileLocation)
        {
        }

        public override bool checkForAction(StardewValley.Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity)
                return true;
            Game1.currentMinigame = new SnakeMinigame(SnakeMod.helper);
            return true;
        }

        public override Item getOne()
        {
            return new SnakeMachine(data) { tileLocation = Vector2.Zero };
        }

        public override ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            CustomObjectData data = CustomObjectData.collection[additionalSaveData["id"]];
            return new SnakeMachine(CustomObjectData.collection[additionalSaveData["id"]], (replacement as Chest).tileLocation);
        }


    }
}
