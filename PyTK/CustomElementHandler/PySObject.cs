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
    public class PySObject : SObject, ICustomObject, IDrawFromCustomObjectData, ISyncableElement
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        public SObject sObject;
        public CustomObjectData data { get; set; }
        public PySync syncObject { get; set; }

        public PySObject()
        {

        }

        public PySObject(CustomObjectData data)
            : base(data.sdvId,1)
        {
            sObject = new SObject(data.sdvId, 1);
            this.data = data;
            checkData();
        }

        public PySObject(CustomObjectData data, Vector2 tileLocation)
            : base(tileLocation,data.sdvId)
        {
            sObject = new SObject(tileLocation, data.sdvId);
            this.data = data;
            checkData();
        }

        public virtual void rebuildData()
        {
            if (CustomObjectData.collection.Find(c => c.Value.getObject().Name == Name) is KeyValuePair<string,CustomObjectData> cd)
                data = cd.Value;
        }

        public virtual Dictionary<string, string> getAdditionalSaveData()
        {
            checkData();
            return new Dictionary<string, string>() { { "id", data != null ? data.id : "na" }, { "tileLocation", TileLocation != null ? TileLocation.X + "," + TileLocation.Y : "0,0" }, { "name", Name != null ? Name : "Error"}, { "quality", Quality.ToString() != null ? Quality.ToString() : "0" }, { "price", Price.ToString() != null ? Price.ToString() : "0"}, { "stack", Stack.ToString() != null ? Stack.ToString() : "1" } };
        }

        public virtual object getReplacement()
        {
            Chest c = new Chest(true);
            c.playerChoiceColor.Value = Color.Magenta;
            c.TileLocation = TileLocation;
            return c;
        }

        public virtual void checkData()
        {
            if (data == null)
                rebuildData();
        }

        public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
        {
            base.updateWhenCurrentLocation(time, environment);
        }

        private void setTags()
        {
            checkData();
            Game1.bigCraftableSpriteSheet.Tag = data.id;
            Game1.objectSpriteSheet.Tag = data.id;
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            setTags();
            base.draw(spriteBatch, x, y, alpha);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            setTags();
            base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            setTags();
            base.drawWhenHeld(spriteBatch, objectPosition, f);
        }

        public override Item getOne()
        {
            if(data.bigCraftable)
                return new PySObject(data, Vector2.Zero) { name = name, Price = price, Quality = quality };
            else
                return new PySObject(data) { TileLocation = Vector2.Zero, name = name, Price = price, Quality = quality };
        }

        public virtual void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            TileLocation = additionalSaveData["tileLocation"].Split(',').toList(i => i.toInt()).toVector<Vector2>();
            Price = additionalSaveData["price"].toInt();
            Name = additionalSaveData["name"];
            Stack = additionalSaveData["stack"].toInt();
            Quality = additionalSaveData["quality"].toInt();
            checkData();
        }

        public virtual ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            CustomObjectData data = CustomObjectData.collection[additionalSaveData["id"]];

            if (data.bigCraftable)
                return new PySObject(data, additionalSaveData["tileLocation"].Split(',').toList(i => i.toInt()).toVector<Vector2>());
            else
                return new PySObject(CustomObjectData.collection[additionalSaveData["id"]]);
        }

        public virtual Dictionary<string, string> getSyncData()
        {
            return null;
        }

        public virtual void sync(Dictionary<string, string> syncData)
        {
            
        }
    }
}
