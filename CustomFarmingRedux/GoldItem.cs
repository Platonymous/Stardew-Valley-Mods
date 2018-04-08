using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using StardewValley;
using System.Collections.Generic;

namespace CustomFarmingRedux
{
    class GoldItem : PySObject
    {
        public GoldItem()
        {

        }

        public GoldItem(CustomObjectData data)
            : base(data)
        {

        }

        public override string DisplayName { get => stack + base.DisplayName.ToLower(); set => base.DisplayName = value; }

        public override Item getOne()
        {
                return new GoldItem(data) { tileLocation = Vector2.Zero, name = name, price = price, quality = quality };
        }

        public override ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            CustomObjectData data = CustomObjectData.collection[additionalSaveData["id"]];
            return new GoldItem(CustomObjectData.collection[additionalSaveData["id"]]);
        }

    }
}
