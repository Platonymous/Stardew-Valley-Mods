using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.Objects;

namespace ArcadePong
{
    class PongMachine : PySObject
    {
        internal static float zoom = 1.0f;

        public PongMachine()
        {

        }

        public PongMachine(CustomObjectData data)
            : base(data, Vector2.Zero)
        {
        }

        public PongMachine(CustomObjectData data, Vector2 tileLocation)
            : base(data, tileLocation)
        {
        }

        public override bool checkForAction(StardewValley.Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity)
                return true;
            PongMinigame.quit = false;
            zoom = Game1.options.zoomLevel;
#if ANDROID
            if (Game1.options.GetType().GetField("baseZoomLevel") is FieldInfo finfo3)
                finfo3.SetValue(Game1.options, 1f);
            else if (Game1.options.GetType().GetField("zoomLevel") is FieldInfo finfo4)
                finfo4.SetValue(Game1.options, 1f);
#else
            Game1.options.baseZoomLevel = 1f;
#endif
            Game1.currentMinigame = new PongMinigame();
            ArcadePongMod.runPong = true;
            return true;
        }

        public override Item getOne()
        {
            return new PongMachine(data) { TileLocation = Vector2.Zero };
        }

        public override ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            CustomObjectData data = CustomObjectData.collection[additionalSaveData["id"]];
            return new PongMachine(CustomObjectData.collection[additionalSaveData["id"]], (replacement as Chest).TileLocation);
        }


    }
}
