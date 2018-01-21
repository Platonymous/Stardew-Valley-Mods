using System;
using System.Collections.Generic;
using StardewValley;
using SObject = StardewValley.Object;
using StardewValley.Objects;
using Microsoft.Xna.Framework;
using PyTK.Extensions;
using StardewModdingAPI;
using Microsoft.Xna.Framework.Graphics;

namespace PyTK.CustomElementHandler
{
    public class PySObject : SObject, ICustomObject
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        public CustomObjectData data { get; set; }

        public PySObject()
        {

        }

        public PySObject(CustomObjectData data)
            : base(data.sdvId,1)
        {
            this.data = data;
        }

        public PySObject(CustomObjectData data, Vector2 tileLocation)
            : base(tileLocation,data.sdvId)
        {
            this.data = data;
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            return new Dictionary<string, string>() { { "id", data.id }, { "tileLocation", tileLocation.X + "," + tileLocation.Y }, { "name", name }, { "quality", quality.ToString() }, { "price", price.ToString() }, { "stack", stack.ToString() } };
        }

        public object getReplacement()
        {
            return new Chest(true) { playerChoiceColor = Color.Magenta };
        }

        public override Item getOne()
        {
            if(data.bigCraftable)
                return new PySObject(data, Vector2.Zero) { name = name, price = price, quality = quality };
            else
                return new PySObject(data) { tileLocation = Vector2.Zero, name = name, price = price, quality = quality };
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            tileLocation = additionalSaveData["tileLocation"].Split(',').toList(i => i.toInt()).toVector<Vector2>();
            price = additionalSaveData["price"].toInt();
            name = additionalSaveData["name"];
            stack = additionalSaveData["stack"].toInt();
            quality = additionalSaveData["quality"].toInt();
        }

        public ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            CustomObjectData data = CustomObjectData.collection[additionalSaveData["id"]];

            if (data.bigCraftable)
                return new PySObject(data, additionalSaveData["tileLocation"].Split(',').toList(i => i.toInt()).toVector<Vector2>());
            else
                return new PySObject(CustomObjectData.collection[additionalSaveData["id"]]);
        }

        public override void updateWhenCurrentLocation(GameTime time)
        {
            parentSheetIndex = data.sdvId;
            base.updateWhenCurrentLocation(time);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            parentSheetIndex = data.sdvId;
            base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber);
        }
    }
}
