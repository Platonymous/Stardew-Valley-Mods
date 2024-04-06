using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Artista.Artpieces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace Artista.Furniture
{
    [XmlType("Mods_platonymous_Artista_PaintingFurniture")]
    public class PaintingFurniture : StardewValley.Objects.Furniture
    {
        internal Texture2D Texture => Art?.GetFinishedTextureForMenu();

        internal Painting Art { get; set; }


        internal bool First { get; set; } = false;

        public SavedArtpiece SavedArtpiece { get; set; }
        public PaintingFurniture(Artpiece art)
            : base("Platonymous.Artista.PaintingFurniture", Vector2.Zero)
        {
            Art = art is Painting p ? p : null;
            SavedArtpiece = art?.Save();
            modData.Add("SavedArtpiece", SavedArtpiece?.GetJsonData() ?? "");
            Init();
        }

        public void Init()
        {
            Restore();
            tileLocation.Value = Vector2.Zero;
            isOn.Value = false;
            base.ParentSheetIndex = -1;
            furniture_type.Value = 6;
            drawHeldObjectLow.Value = false;
            rotations.Value = 1;
            price.Value = 300;
            currentRotation.Value = 0;
            ArtistaMod.Singleton.Helper.Reflection.GetField<int>(this, "_placementRestriction").SetValue(0);
            UpdateBounds();

        }

        private void UpdateBounds()
        {
            defaultSourceRect.Value = new Rectangle(0, 0, Texture?.Width ?? 16, Texture?.Height ?? 16);
            sourceRect.Value = defaultSourceRect.Value;
            defaultBoundingBox.Value = new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, (int)(Art?.Tilesize.X * 64 ?? 64), (int)(Art?.Tilesize.Y * 64 ?? 64));
            boundingBox.Value = defaultBoundingBox.Value;
            base.name = Art?.Name ?? "Painting";
            updateDrawPosition();
        }

        protected override string loadDisplayName()
        {
            return Art?.Name ?? "Painting";
        }

        public PaintingFurniture()
            : base("Platonymous.Artista.PaintingFurniture", Vector2.Zero)
        {
            Init();
        }

        public override void updateDrawPosition()
        {
            drawPosition.Value = new Vector2(boundingBox.X, boundingBox.Y + 16);
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            Restore();
            if (isTemporarilyInvisible || Game1.player.CurrentItem == this || Texture == null)
                return;
            var value = sourceRect.Value;
            Vector2 pos = Game1.GlobalToLocal(Game1.viewport, drawPosition.Value + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero));

            var dest = new Rectangle((int)pos.X, (int)pos.Y,(int)Art.Tilesize.X * 64, (int)Art.Tilesize.Y * 64);
            var dest2 = new Rectangle((int)pos.X, (int)pos.Y, (int)Art.Tilesize.X * 64, Art.Width == Art.Height ? (int)Art.Tilesize.Y * 64 - 10: ((int)Art.Tilesize.Y * 64) - 20);
            var scalex = Art.Width == Art.Height ? (dest.Width - 24f) / dest.Width : (dest.Width - 16f) / dest.Width;
            var scaley = scalex;

            dest = new Rectangle(dest.X + (int)((dest.Width * (1f - scalex)) / 2f), dest.Y + (int)((dest.Height * (1f - scaley)) / 2f) - (Art.Width == Art.Height ? 5 : 10), (int)(dest.Width * scalex), (int)(dest.Height * scaley));
            spriteBatch.Draw(Art.GetFullTexture(), dest, value, Color.White * alpha, 0f, Vector2.Zero,SpriteEffects.None, ((int)furniture_type == 12) ? (2E-09f + tileLocation.Y / 100000f) : ((float)(boundingBox.Value.Bottom - (((int)furniture_type == 6 || (int)furniture_type == 17 || (int)furniture_type == 13) ? 48 : 8)) / 10000f));
            spriteBatch.Draw(Art.Border, dest2, new Rectangle(0,0,Art.Border.Width,Art.Border.Height), Color.White * alpha *0.9f, 0f, Vector2.Zero, SpriteEffects.None, ((int)furniture_type == 12) ? (2E-09f + tileLocation.Y / 100000f) : ((float)(boundingBox.Value.Bottom - (((int)furniture_type == 6 || (int)furniture_type == 17 || (int)furniture_type == 13) ? 48 : 8)) / 10000f) + 0.000001f);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            Restore();
            if (isTemporarilyInvisible || Texture == null)
                return;
            var pos = location + new Vector2(32f, 32f);
            var sc = 1f * getScaleSize() * scaleSize;
            spriteBatch.Draw(Art.GetFinishedTextureForMenu(),new Rectangle((int)pos.X, (int)pos.Y, (int)(Art.Tilesize.X * 16 * sc), (int)(Art.Tilesize.Y * 16 * sc)), defaultSourceRect.Value, color * transparency, 0f, new Vector2(defaultSourceRect.Width / 2, defaultSourceRect.Height / 2), SpriteEffects.None, layerDepth);
            spriteBatch.Draw(Art.Border, new Rectangle((int)pos.X, (int)pos.Y, (int)(Art.Tilesize.X * 16 * sc), (int)(Art.Tilesize.Y * 16 * sc)), new Rectangle(0,0,Art.Border.Width,Art.Border.Height), color * transparency, 0f, new Vector2(Art.Border.Width / 2, Art.Border.Height / 2), SpriteEffects.None, layerDepth + 0.000001f);
        }

        public override string getDescription()
        {
            Restore();
            return Art?.Description ?? "Painting";
        }

        public override string getCategoryName()
        {
            Restore();
            return Art?.GetCategoryName ?? "Painting";
        }

        protected override Item GetOneNew()
        {
            Restore();
            return new PaintingFurniture(Art);
        }

        protected override void GetOneCopyFrom(Item source)
        {
            return;
        }


        public override bool canStackWith(ISalable other)
        {
            return false;
        }
        public void Restore()
        {
            if (Art != null && SavedArtpiece != null)
                return;
            else if (Art == null && SavedArtpiece != null)
            {
                Art = new Painting(SavedArtpiece);
                if (modData.ContainsKey("SavedArtpiece"))
                    modData["SavedArtpiece"] = SavedArtpiece?.GetJsonData() ?? "";
                else
                    modData.Add("SavedArtpiece", SavedArtpiece?.GetJsonData() ?? "");
                UpdateBounds();
            }
            else if (Art != null && SavedArtpiece == null)
            {
                SavedArtpiece = Art.Save();
                if (modData.ContainsKey("SavedArtpiece"))
                    modData["SavedArtpiece"] = SavedArtpiece?.GetJsonData() ?? "";
                else
                    modData.Add("SavedArtpiece", SavedArtpiece?.GetJsonData() ?? "");
                UpdateBounds();
            }
            else if(Art == null && SavedArtpiece == null)
            {
                if(modData.ContainsKey("SavedArtpiece") && !string.IsNullOrEmpty(modData["SavedArtpiece"]))
                {
                    SavedArtpiece = SavedArtpiece.FromJson(modData["SavedArtpiece"]);
                    Art = new Painting(SavedArtpiece);
                }
            }
        }
    }
}
