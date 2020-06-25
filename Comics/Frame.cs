using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comics
{
    public class Frame : Furniture, ICustomObject
    {
        public Frame(ComicBook comic, Vector2 tileLocation)
            : base(1602, tileLocation)
        {
            this.heldObject.Value = comic;
            this.Stack = comic.Stack;
            this.Type = "painting";
        }

        public Frame()
            : base(1602, Vector2.Zero)
        {

        }

        public override string Name => heldObject.Value?.Name ?? "Frame";

        public override string DisplayName { 
            get => Name; 
            set { return; } 
        }

        public override string getCategoryName()
        {
            return "Comic Book";
        }

        public override Color getCategoryColor()
        {
            return Color.DarkCyan;
        }

        public override string getDescription()
        {
            return this.heldObject.Value?.getDescription() ?? "Empty";
        }

        public override Item getOne()
        {
            return new Frame(this.heldObject.Value as ComicBook, Vector2.Zero);
        }

        public override bool canStackWith(ISalable other)
        {
            return other is Frame f && f.heldObject.Value is ComicBook cb && cb.Id == (heldObject.Value as ComicBook).Id;
        }

        public override bool canBeRemoved(Farmer who)
        {
            return true;
        }

        public override bool clicked(Farmer who)
        {
            Game1.haltAfterCheck = false;
            return false;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            this.heldObject.Value.Stack = Stack;
            this.heldObject.Value.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            if (!(Game1.currentLocation is DecoratableLocation))
                heldObject.Value.drawWhenHeld(spriteBatch, objectPosition, f);
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            float scale = 11f;
            Vector2 offset = new Vector2(0, -64f);

            var cb = (this.heldObject.Value as ComicBook);
            cb.checkLoad();

            var texture = cb.UsePlaceholder ? AssetManager.Instance.Placeholder : (this.heldObject.Value as ComicBook).Texture;
            var source = new Rectangle(0, 0, texture.Width, texture.Height);
            if (Furniture.isDrawingLocationFurniture)
                spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, (Vector2)this.drawPosition.Value + (this.shakeTimer > 0 ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero)), source, Color.White * alpha, 0.0f, Vector2.Zero, scale, SpriteEffects.None, (float)(this.boundingBox.Value.Bottom - 48) / 10000f);
            else
                spriteBatch.Draw(texture, offset + Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 - (source.Height * 4 - this.boundingBox.Height) + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0)))), source, Color.White * alpha, 0.0f, Vector2.Zero, scale, SpriteEffects.None, (float)(this.boundingBox.Value.Bottom - 48) / 10000f);
        }

        public ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            var r = (replacement as Furniture);
            var f = new Frame(r.heldObject.Value as ComicBook,r.TileLocation);

            return f;
        }

        public object getReplacement()
        {
            Furniture f = new Furniture(1602, this.TileLocation);
            f.heldObject.Value = heldObject.Value;
            return f;
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            return new Dictionary<string, string>();
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            
        }
    }
}
