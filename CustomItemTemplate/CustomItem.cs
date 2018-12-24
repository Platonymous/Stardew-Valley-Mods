using PyTK.CustomElementHandler;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System;
using PyTK.Types;
using StardewModdingAPI;
using PyTK.Extensions;

namespace CustomitemTemplate
{
    class CustomItem : PySObject
    {
        public CustomItem() : base() { }

        public CustomItem(CustomObjectData data) : base(data) { }

        public CustomItem(CustomObjectData data, Vector2 tileLocation) : base(data, tileLocation) { }

        private static IModHelper Helper;
        private static ITranslationHelper i18n;

        public static void init(IModHelper helper)
        {
            Helper = helper;
            i18n = helper.Translation;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        }

        private static void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            string modId = Helper.ModRegistry.ModID;
            var items = Helper.Data.ReadJsonFile<Items>("data.json");
            foreach (Data data in items.Content)
            {
                Texture2D texture = Helper.Content.Load<Texture2D>(data.Texture);
                if (data.ScaleUp)
                {
                    float scale = (float)(Convert.ToDouble(texture.Width) / Convert.ToDouble(data.OriginalWidth));
                    int height = (int)(texture.Height / scale);
                    texture = ScaledTexture2D.FromTexture(texture.getArea(new Rectangle(0, 0, data.OriginalWidth, height)), texture, scale);
                }
                var cod = new CustomObjectData(modId + "." + data.Id, data.DataString.Replace("{Name}", i18n.Get(data.Id + ".Name")).Replace("{Description}", i18n.Get(data.Id + ".Description")), texture, Color.White, data.TileIndex, data.BigCraftable, typeof(CustomItem), data.CraftingRecipe != null ? new CraftingData(modId + "." + data.Id, data.CraftingRecipe, i18n.Get(data.Id + ".Name"), -1, data.BigCraftable) : null);
                if (data.SoldBy != null)
                {
                    var i = new InventoryItem(cod.getObject(), data.Price);
                    i.addToNPCShop(data.SoldBy);
                }
            }

            Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
        }

        public override Item getOne()
        {
            if (!data.bigCraftable)
                return new CustomItem(data);
            else
                return new CustomItem(data, Vector2.Zero);
        }

        public override ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            return (ICustomObject) CustomObjectData.collection[additionalSaveData["id"]].getObject();
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            spriteBatch.Draw(data.texture, objectPosition, data.sourceRectangle, Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + 2) / 10000f));
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            if (data.texture is ScaledTexture2D s)
            {
                Rectangle tilesize = data.bigCraftable ? new Rectangle(0, 0, 16, 32) : new Rectangle(0, 0, 16, 16);
                Vector2 vector2 = getScale() * Game1.pixelZoom;
                Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((x * Game1.tileSize), (y * Game1.tileSize - Game1.tileSize)));
                var r = data.sourceRectangle;
                Rectangle destinationRectangle = new Rectangle((int)(local.X - vector2.X / 2.0) + (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)(local.Y - vector2.Y / 2.0) + (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((tilesize.Width * 4) + (double)vector2.X), (int)((tilesize.Height * 4) + vector2.Y / 2.0));
                var newSR = new Rectangle?(new Rectangle((int)(r.X * s.Scale), (int)(r.Y * s.Scale), (int)(r.Width * s.Scale), (int)(r.Height * s.Scale)));
                spriteBatch.Draw(s.STexture, destinationRectangle, newSR, Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, (float)(Math.Max(0.0f, ((y + 1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + x * 9.99999974737875E-06));
            }
            else
                base.draw(spriteBatch, x, y, alpha);
            }

        public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {
            if(data.texture is ScaledTexture2D)
                draw(spriteBatch, xNonTile, yNonTile, alpha);
            else
                base.draw(spriteBatch, xNonTile, yNonTile, layerDepth, alpha);
        }
    }
}
