using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using PlatoTK;
using PlatoTK.Content;
using PlatoTK.Objects;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;

namespace Comics
{
    public class ComicBook : PlatoSObject<StardewValley.Object>
    {
        public static ISaveIndex SaveIndex { get; set; }

        public Texture2D Texture { get; set; } = null;
        public bool Loaded { get; set; } = false;

        public bool UsePlaceholder { get; set; } = true;

        public string ComicDescription
        {
            get
            {
                return Data.Get("Description");
            }
            set
            {
                Data.Set("Description", value);
            }
        }

        public string ComicId
        {
            get
            {
                return Data.Get("Id");
            }
            set
            {
                Data.Set("Id", value);
            }
        }

        public string Volume
        {
            get
            {
                return Data.Get("Volume") ?? "";
            }
            set
            {
                Data.Set("Volume", value);
            }
        }

        public string Number
        {
            get
            {
                return Data.Get("Number") ?? "";
            }
            set
            {
                Data.Set("Number", value);
            }
        }

        public string SmallImage
        {
            get
            {
                return Data.Get("SmallImage") ?? "";
            }
            set
            {
                Data.Set("SmallImage", value);
            }
        }

        public string BigImage
        {
            get
            {
                return Data.Get("BigImage") ?? "";
            }
            set
            {
                Data.Set("BigImage", value);
            }
        }

        public override string Name
        {

            get
            {
                return Data.DataString ?? "";
            }
            set
            {
            }
        }

        public override string DisplayName
        {
            get {
                return Data.Get("Name") ?? "Comic Book";
            }
            set
            {

            }
        }

        public override string getCategoryName()
        {
            return "Comic Book";
        }

        public ComicBook()
        {

        }


        public ComicBook(Vector2 tileLocation, int parentSheetIndex, int initialStack)
        {

        }
        public ComicBook(Vector2 tileLocation, int parentSheetIndex, bool isRecipe = false)
        {

        }
        public ComicBook(int parentSheetIndex, int initialStack, bool isRecipe = false, int price = -1, int quality = 0)
        {

        }
        public ComicBook(Vector2 tileLocation, int parentSheetIndex, string Givenname, bool canBeSetDown, bool canBeGrabbed, bool isHoedirt, bool isSpawnedObject)
        {

        }



        public override string getDescription()
        {
            SpriteFont smallFont = Game1.smallFont;
            int descriptionWidth = getDescriptionWidth();
            return Game1.parseText(ComicDescription, smallFont, descriptionWidth);
        }

        public new int getDescriptionWidth()
        {
            int val1 = 272;
            if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
                val1 = 384;
            return Math.Max(val1, (int)Game1.dialogueFont.MeasureString(this.ComicDescription ?? "").X);
        }

        public void checkLoad()
        {
            CheckParentSheetIndex();
            if (Loaded || (!AssetManager.LoadImagesInShop && Game1.activeClickableMenu is ShopMenu && !Game1.player.hasItemInInventoryNamed(Name)))
                return;

            Loaded = true;

            Texture = !string.IsNullOrEmpty(SmallImage) && SmallImage != " " ? AssetManager.Instance.LoadImage(SmallImage,ComicId) : AssetManager.Instance.LoadImageForIssue(ComicId);
                   UsePlaceholder = false;
        }
        
        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            if (x == 0 && y == 0)
                return;

            float scale = 11f;
            Vector2 offset = new Vector2(0, -64f);
            var texture = UsePlaceholder ? AssetManager.Instance.Placeholder : Texture;
            var source = new Rectangle(0, 0, texture.Width, texture.Height);

            if (Furniture.isDrawingLocationFurniture)
                spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x,y) + (Base.shakeTimer > 0 ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero)), source, Color.White * alpha, 0.0f, Vector2.Zero, scale, SpriteEffects.None, (float)(Base.boundingBox.Value.Bottom - 48) / 10000f);
            else
                spriteBatch.Draw(texture, offset + Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + (Base.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 - (source.Height * 4 - Base.boundingBox.Height) + (Base.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0)))), source, Color.White * alpha, 0.0f, Vector2.Zero, scale, SpriteEffects.None, (float)(Base.boundingBox.Value.Bottom - 48) / 10000f);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            checkLoad();
            var texture = UsePlaceholder ? AssetManager.Instance.Placeholder : Texture;
            var offset = new Vector2(16f,8f);

            spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), color * 0.5f, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
            spriteBatch.Draw(texture, location + offset, null, color, 0.0f, Vector2.Zero, 3f, SpriteEffects.None, layerDepth + 0.0001f);
            if(Base?.Stack > 1)
            Utility.drawTinyDigits(Base.Stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(Base.Stack, 3f * scaleSize)) + 3f * scaleSize, (float)(64.0 - 18.0 * (double)scaleSize + 1.0)), 3f * scaleSize, 1f, color);
        }
        
        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            checkLoad();
            var texture = UsePlaceholder ? AssetManager.Instance.Placeholder : Texture;
            var offset = new Vector2(-16f, -58f);
            spriteBatch.Draw(texture, objectPosition + offset, null, Color.White, 0.0f, Vector2.Zero, 8f, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 3) / 10000f));
        }
        
        public override Item getOne()
        {
            return GetNew(ComicId);
        }

        public override bool CanLinkWith(object linkedObject)
        {
            return linkedObject is StardewValley.Object obj && obj.netName.Get().Contains("IsComicBookObject");
        }

        public static StardewValley.Object GetNew(string id)
        {
            SaveIndex.ValidateIndex();

            var newComic = new StardewValley.Object(SaveIndex.Index,1);
            
            newComic.netName.Set("Plato:IsComicBookObject=true|Id=" + id);

            return newComic;
        }
        private void CheckParentSheetIndex()
        {
            if (SaveIndex.Index != Base?.parentSheetIndex.Value)
            {
                SaveIndex.ValidateIndex(Base?.parentSheetIndex.Value ?? -1);
                Base?.parentSheetIndex.Set(SaveIndex.Index);
            }
        }
        public override void OnConstruction(IPlatoHelper helper, object linkedObject)
        {
            base.OnConstruction(helper, linkedObject);
            SaveIndex.ValidateIndex();
            Data?.Set("IsComicBookObject", true);

            CheckParentSheetIndex();

            if (!string.IsNullOrEmpty(ComicId))
            {
                Issue issue = AssetManager.Instance.GetIssue(ComicId);
                Data.Set("Volume",issue.Volume.Name);
                Data.Set("Number", issue.IssueNumber);
                Data.Set("SmallImage", issue.Image.SmallUrl.ToString());
                Data.Set("BigImage", issue.Image.MediumUrl.ToString());
                Data.Set("Name", $"{Volume} #{Number}");
                Data.Set("Description", issue.Name + " (Comic Book)");
                Data.Set("IsComicBookObject", true);
                Helper.SetTickDelayedUpdateAction(1,() => checkLoad());
            }
        }

        public override NetString GetDataLink(object linkedObject)
        {
            if (linkedObject is Item item)
                return item.netName;

            return null;
        }
    }
}
