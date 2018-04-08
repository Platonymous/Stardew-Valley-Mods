using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using StardewValley;
using System.Collections.Generic;

namespace CustomFarmingRedux
{
    class WaterItem : PySObject
    {
        public WaterItem()
        {

        }

        public WaterItem(CustomObjectData data)
            : base(data)
        {

        }

        public override string DisplayName { get => (stack / 10f).ToString() + "l " + base.DisplayName.ToLower(); set => base.DisplayName = value; }

        public override Item getOne()
        {
            return new WaterItem(data) { tileLocation = Vector2.Zero, name = name, price = price, quality = quality };
        }

        public override ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            CustomObjectData data = CustomObjectData.collection[additionalSaveData["id"]];
            return new WaterItem(CustomObjectData.collection[additionalSaveData["id"]]);
        }
    }
}
