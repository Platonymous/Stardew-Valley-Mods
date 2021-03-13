using System;
using System.IO;
using StardewValley;
using StardewValley.Objects;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Locations;
using Netcode;

namespace CustomFurniture
{
    public class CustomFurniture : Furniture, ISaveElement
    {
        public Texture2D texture;
        public static Dictionary<string, string> Textures = new Dictionary<string, string>();
        private int animationFrames;
        private int frame;
        private Rectangle animatedSourceRect;
        private int frameWidth;
        private int skipFrame;
        private int counter;
        private int rotatedHeight;
        private int rotatedWidth;
        private int rotatedBoxWidth;
        private int rotatedBoxHeight;
        public FurnitureRotation fRotation;
        public CustomFurnitureData data;
        public string id;

        public CustomFurniture()
        {

        }

        public CustomFurniture(CustomFurnitureData data, string objectID, Vector2 tile)
        {
            build(data, objectID, tile);
        }

        public void build(CustomFurnitureData data, string objectID, Vector2 tile)
        {
            id = objectID;
            rotatedWidth = data.rotatedWidth == -1 ? data.height : data.rotatedWidth;
            rotatedWidth *= 16;
            rotatedHeight = data.rotatedHeight == -1 ? data.width : data.rotatedHeight;
            rotatedHeight *= 16;
            rotatedBoxWidth = data.rotatedBoxWidth == -1 ? data.boxHeight : data.rotatedBoxWidth;
            rotatedBoxWidth *= Game1.tileSize;
            rotatedBoxHeight = data.rotatedBoxHeight == -1 ? data.boxWidth : data.rotatedBoxHeight;
            rotatedBoxHeight *= Game1.tileSize;
            this.data = data;

            animationFrames = data.animationFrames;
            frameWidth = data.setWidth;
            frame = 0;
            skipFrame = 60 / data.fps;
            counter = 0;
            tileLocation.Value = tile;

            CustomFurnitureMod.helper.Reflection.GetField<string>(this, "_description").SetValue(data.description);
            CustomFurnitureMod.helper.Reflection.GetField<int>(this, "_placementRestriction").SetValue(2);

            parentSheetIndex.Set(data.index);
            setTexture();
            name = data.name;
            List<string> decorTypes = new List<string>();
            decorTypes.Add("chair");
            decorTypes.Add("bench");
            decorTypes.Add("couch");
            decorTypes.Add("armchair");
            decorTypes.Add("dresser");
            decorTypes.Add("bookcase");
            decorTypes.Add("other");
            string typename = data.type.Contains("table") ? "table" : decorTypes.Contains(data.type) ? "decor" : data.type;
            furniture_type.Value = data.type.Contains("table") ? 11 : decorTypes.Contains(data.type) ? 8 : CustomFurnitureMod.helper.Reflection.GetMethod(new Furniture(), "getTypeNumberFromName").Invoke<int>(data.type);
            furniture_type.Value = getTypeFromName(typename);
            defaultSourceRect.Value = new Rectangle(data.index * 16 % texture.Width, data.index * 16 / texture.Width * 16, 1, 1);
            drawHeldObjectLow.Value = false;

            defaultSourceRect.Width = data.width * 16;
            defaultSourceRect.Height = data.height * 16;
            sourceRect.Value = new Rectangle(defaultSourceRect.X, defaultSourceRect.Y, defaultSourceRect.Width, defaultSourceRect.Height);
            animatedSourceRect = sourceRect;
            defaultSourceRect.Value = sourceRect;

            defaultBoundingBox.Value = new Rectangle((int)tileLocation.X, (int)tileLocation.Y, data.boxWidth * Game1.tileSize, data.boxHeight * Game1.tileSize);

            boundingBox.Value = new Rectangle((int)tileLocation.X * Game1.tileSize, (int)tileLocation.Y * Game1.tileSize, defaultBoundingBox.Width, defaultBoundingBox.Height);
            defaultBoundingBox.Value = boundingBox;

            updateDrawPosition();
            rotations.Value = data.rotations;
            price.Value = data.price;

            fRotation = FurnitureRotation.horizontal;
            texture = null;
        }

        public override void DayUpdate(GameLocation location)
        {
            if (data.fromContent)
                texture = CustomFurnitureMod.helper.Content.Load<Texture2D>(data.texture, StardewModdingAPI.ContentSource.GameContent);

            base.DayUpdate(location);
        }

        private void setTexture()
        {
            restore();
            string folder = new DirectoryInfo(data.folderName).Name;
            string tkey = $"{folder}/{data.texture}";
            if (Textures.ContainsKey(tkey))
                texture = CustomFurnitureMod.helper.Content.Load<Texture2D>(Textures[($"{folder}/{data.texture}")],StardewModdingAPI.ContentSource.GameContent);
        }

        protected override string loadDisplayName()
        {
            return name;
        }

        public override string getDescription()
        {
            string text = description;
            SpriteFont smallFont = Game1.smallFont;
            int width = Game1.tileSize * 4 + Game1.tileSize / 4;
            return Game1.parseText(text, smallFont, width);
        }

        private int getTypeFromName(String type)
        {
            switch (type)
            {
                case "chair": return chair;
                case "bench": return bench;
                case "couch": return couch;
                case "armchair": return armchair;
                case "dresser": return dresser;
                case "long table": return longTable;
                case "painting": return painting;
                case "lamp": return lamp;
                case "decor": return decor;
                case "bookcase": return bookcase;
                case "table": return table;
                case "rug": return rug;
                case "window": return window;
                default: return other;
            }
        }

        public override Item getOne()
        {
            return new CustomFurniture(data, id, Vector2.Zero);
        }

        private float getScaleSize()
        {
            int num1 = sourceRect.Width / 16;
            int num2 = sourceRect.Height / 16;
            if (num1 >= 5)
                return 0.75f;
            if (num2 >= 3)
                return 1f;
            if (num1 <= 2)
                return 2f;
            return num1 <= 4 ? 1f : 0.1f;
        }

        public enum FurnitureRotation : int
        {
            horizontal = 0,
            vertical = 1,
            flipped = 3,
            back = 2
        }

        public void customRotate()
        {
            fRotation = (FurnitureRotation) currentRotation.Value;
            flipped.Value = false;

            if (rotations < 2)
                return;

            switch (fRotation)
            {
                case FurnitureRotation.horizontal: fRotation = FurnitureRotation.vertical; break;
                case FurnitureRotation.vertical: fRotation = (rotations > 2) ? FurnitureRotation.back : FurnitureRotation.horizontal; break;
                case FurnitureRotation.back: fRotation = FurnitureRotation.flipped; flipped.Value = true; break;
                case FurnitureRotation.flipped: fRotation = FurnitureRotation.horizontal; break;
            }

            currentRotation.Value = (int)fRotation;
            sourceRect.Value = getCurrentSourceRectangle();
            boundingBox.Value = getCurrentBoundingBox();
        }

        public Rectangle getCurrentSourceRectangle()
        {
            if(fRotation == FurnitureRotation.vertical || fRotation == FurnitureRotation.flipped)
                return new Rectangle(defaultSourceRect.X + defaultSourceRect.Width, defaultSourceRect.Y, rotatedWidth, rotatedHeight);
            if (fRotation == FurnitureRotation.back)
                return new Rectangle(defaultSourceRect.X + defaultSourceRect.Width + rotatedWidth, defaultSourceRect.Y, defaultSourceRect.Width, defaultSourceRect.Height);

            return defaultSourceRect;
        }

        public Rectangle getCurrentBoundingBox()
        {
            if (fRotation == FurnitureRotation.vertical || fRotation == FurnitureRotation.flipped)
                return new Rectangle((int)tileLocation.X * Game1.tileSize, ((int)tileLocation.Y) * Game1.tileSize, rotatedBoxWidth, rotatedBoxHeight);

            return defaultBoundingBox;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if (texture == null)
                setTexture();

            spriteBatch.Draw(texture, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), new Rectangle?(defaultSourceRect), color * transparency, 0.0f, new Vector2((float)(defaultSourceRect.Width / 2), (float)(defaultSourceRect.Height / 2)), 1f * getScaleSize() * scaleSize, SpriteEffects.None, layerDepth);
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (texture == null)
                setTexture();

            counter++;
            counter = counter > skipFrame ? 0 : counter;
            frame = counter == 0 ? frame + 1 : frame;
            frame = frame >= animationFrames ? 0 : frame;
            int offset = frameWidth * 16 * frame;

            animatedSourceRect = frame == 0 ? sourceRect : new Rectangle(offset + sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height);
          
            if (x == -1)
                spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, drawPosition), new Rectangle?(animatedSourceRect), Color.White * alpha, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, furniture_type == 12 ? 0.0f : (float)(boundingBox.Bottom - 8) / 10000f);
            else
                spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize), (float)(y * Game1.tileSize - (sourceRect.Height * Game1.pixelZoom - boundingBox.Height)))), new Rectangle?(sourceRect), Color.White * alpha, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, this.furniture_type.Value == 12 ? 0.0f : (float)(boundingBox.Bottom - 8) / 10000f);
            if (heldObject.Value == null)
                return;
            if (heldObject.Value is CustomFurniture ho)
            {
                customDrawAtNonTileSpot(ho, spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(boundingBox.Center.X - Game1.tileSize / 2), (float)(boundingBox.Center.Y - ho.sourceRect.Height * Game1.pixelZoom - (drawHeldObjectLow ? -Game1.tileSize / 4 : Game1.tileSize / 4)))), (float)(boundingBox.Bottom - 7) / 10000f, alpha);
            }
            else if (heldObject.Value is Furniture)
            {
                (this.heldObject.Value as Furniture).drawAtNonTileSpot(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.boundingBox.Center.X - Game1.tileSize / 2), (float)(this.boundingBox.Center.Y - (this.heldObject.Value as Furniture).sourceRect.Height * Game1.pixelZoom - (this.drawHeldObjectLow.Value ? -Game1.tileSize / 4 : Game1.tileSize / 4)))), (float)(this.boundingBox.Bottom - 7) / 10000f, alpha);
            }
            else
            {
                spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(boundingBox.Center.X - Game1.tileSize / 2), (float)(boundingBox.Center.Y - (drawHeldObjectLow ? Game1.tileSize / 2 : Game1.tileSize * 4 / 3)))) + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize * 5 / 6)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White * alpha, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)boundingBox.Bottom / 10000f);
                spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(boundingBox.Center.X - Game1.tileSize / 2), (float)(boundingBox.Center.Y - (drawHeldObjectLow ? Game1.tileSize / 2 : Game1.tileSize * 4 / 3)))), new Rectangle?(GameLocation.getSourceRectForObject(ParentSheetIndex)), Color.White * alpha, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, (float)(boundingBox.Bottom + 1) / 10000f);
            }
        }

        private void customDrawAtNonTileSpot(CustomFurniture ho, SpriteBatch spriteBatch, Vector2 location, float layerDepth, float alpha = 1f)
        {
            restore();
            spriteBatch.Draw(ho.texture, location, new Rectangle?(ho.sourceRect.Value), Color.White * alpha, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, this.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
        }

        public object getReplacement()
        {
            Furniture replacement = new Furniture(0, tileLocation, currentRotation);
            if (heldObject.Value != null)
                replacement.heldObject.Value = heldObject;

            replacement.sourceRect.Value = this.sourceRect.Value;
            replacement.rotations.Value = rotations;
            replacement.currentRotation.Value = currentRotation;
            return replacement;
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            Dictionary<string, string> savedata = new Dictionary<string, string>();
            savedata.Add("name", name);
            savedata.Add("id", id);
            savedata.Add("rotation", ((int) fRotation).ToString());
            savedata.Add("rotations", rotations.ToString());
            return savedata;
        }

        public void restore()
        {
            if (this.data == null)
           foreach(var f in CustomFurnitureMod.furniturePile)
                if (f.Value.data.name == name)
                {
                    build(f.Value.data, f.Key, tileLocation);
                    break;
                }
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            
            if (replacement is Furniture f)
            {
                string id = additionalSaveData["id"];
                CustomFurnitureMod.log("Rebuild:" + id);
                if (!CustomFurnitureMod.furniture.ContainsKey(id))
                    id = new List<string>(CustomFurnitureMod.furniture.Keys).Find(k => id.Contains(Path.Combine("Furniture",k)));
                if (id == null || !CustomFurnitureMod.furniture.ContainsKey(id))
                    id = CustomFurnitureMod.furniture.Keys.First<string>();

                build(CustomFurnitureMod.furniture[id].data, id, f.TileLocation);

                rotations.Value = additionalSaveData.ContainsKey("rotations") ? int.Parse(additionalSaveData["rotations"]) : (replacement as Furniture).rotations.Value;
                currentRotation.Value = additionalSaveData.ContainsKey("rotation") ? int.Parse(additionalSaveData["rotation"]) : (replacement as Furniture).currentRotation.Value;
                fRotation = (FurnitureRotation)currentRotation.Value;
                tileLocation.Value = (replacement as Furniture).TileLocation;

                rotate();
                rotate();
                rotate();
                rotate();

                if (f.heldObject.Value != null)
                    this.heldObject.Value = f.heldObject.Value;
            }
        }
    }

}
