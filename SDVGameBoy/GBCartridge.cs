using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using StardewValley;

namespace SDVGameBoy
{
    class GBCartridge : PySObject
    {

        internal static Dictionary<string, byte[]> roms;
        public byte[] rom
        {
            get
            {
                return roms[Name];
            }
        }

        public override string getDescription()
        {
            return "A Game for your GameBoy.";
        }

        public GBCartridge()
        {

        }

        public GBCartridge(CustomObjectData data)
            : base(data)
        {
        }

        public override bool canStackWith(Item other)
        {
            return false;
        }

        public override bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            return false;
        }

        public override bool canBeShipped()
        {
            return false;
        }

        public override bool canBeTrashed()
        {
            return true;
        }

        public override Item getOne()
        {
            return new GBCartridge(data);
        }

        public override ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            CustomObjectData data = CustomObjectData.collection[additionalSaveData["id"]];
            return new GBCartridge(CustomObjectData.collection[additionalSaveData["id"]]);
        }


    }
}
