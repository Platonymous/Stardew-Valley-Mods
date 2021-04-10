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


#if ANDROID
        private Issue GetIssue => AssetManager.Instance.GetIssue(ComicId);
#else
        private void SetModData(string key, string value, StardewValley.Object b = null)
        {
            if (b == null)
                b = Base;

            if (b?.modDataForSerialization.ContainsKey(key) ?? false)
                b.modDataForSerialization[key] = value;
            else
                b?.modDataForSerialization.Add(key, value);
        }

        private string GetModData(string key, StardewValley.Object b = null)
        {
            if (b == null)
                b = Base;

            return b?.modDataForSerialization.ContainsKey(key) ?? false ? b.modDataForSerialization[key] : "";
        }
#endif

        public string ComicDescription
        {
            get
            {
#if ANDROID
                return Name + " (Comic Book)";
#else
                return GetModData("Description");
#endif
            }
            set
            {
#if ANDROID
#else
                SetModData("Description", value);
#endif
            }
        }

        public string ComicId
        {
            get
            {
#if ANDROID
                return netName.Value.Split(';') is string[] s && s.Length > 1 ? s[1] : "244342";
#else
                return GetModData("Id");
#endif
            }
            set
            {
#if ANDROID
#else           
                SetModData("Id", value);
#endif
            }
        }

        public string Volume
        {
            get
            {
#if ANDROID
                return GetIssue.Volume.Name;
#else
                return GetModData("Volume");
#endif
            }
            set
            {
#if ANDROID
#else   
                SetModData("Volume", value);
#endif
            }
        }

        public string Number
        {
            get
            {
#if ANDROID
                return GetIssue.IssueNumber;
#else
                return GetModData("Number");
#endif
            }
            set
            {
#if ANDROID
#else
                SetModData("Number", value);
#endif
            }
        }

        public string SmallImage
        {
            get
            {
#if ANDROID
                return GetIssue.Image.SmallUrl.ToString();
#else
                return GetModData("SmallImage");
#endif
            }
            set
            {
#if ANDROID
#else
                SetModData("SmallImage", value);
#endif
            }
        }

        public string BigImage
        {
            get
            {
#if ANDROID
                return GetIssue.Image.MediumUrl.ToString();
#else
                return GetModData("BigImage");
#endif
            }
            set
            {
#if ANDROID
#else
                SetModData("bigImage", value);
#endif
            }
        }

        public override string DisplayName
        {
            get
            {
#if ANDROID
                return $"{Volume} #{Number}";
#else
                return GetModData("Name") is string n && n != "" ? n : "Comic Book";
#endif
            }
            set
            {
#if ANDROID
#else
                SetModData("Name", value);
#endif
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
       
        public static StardewValley.Object GetNew(string id)
        {
            SaveIndex.ValidateIndex();

            var newComic = new StardewValley.Object(SaveIndex.Index,1);

            PlatoObject<StardewValley.Object>.SetIdentifier(newComic, typeof(ComicBook));
#if ANDROID
            newComic.netName.Value = newComic.netName.Value.Split(';')[0] + ";" + id;
#else        
            newComic.modDataForSerialization.Add("Id", id);
#endif

            return newComic;
        }
        private void CheckParentSheetIndex()
        {
            if (SaveIndex.Index != Base?.parentSheetIndex.Value)
            {
                SaveIndex.ValidateIndex();
                Base?.parentSheetIndex.Set(SaveIndex.Index);
            }
        }
        public override void OnConstruction(IPlatoHelper helper, object linkedObject)
        {
            base.OnConstruction(helper, linkedObject);
            SaveIndex.ValidateIndex();

            CheckParentSheetIndex();

            if (string.IsNullOrEmpty(ComicId))
                ComicId = "244342";

            if (linkedObject is StardewValley.Object obj)
            {
                Issue issue = AssetManager.Instance.GetIssue(ComicId);
#if ANDROID
                obj.netName.Value = obj.netName.Value.Split(';')[0] + ';' + ComicId;
#else
                SetModData("Volume", issue.Volume.Name,obj);
                SetModData("Number", issue.IssueNumber, obj);
                SetModData("SmallImage", issue.Image.SmallUrl.ToString(), obj);
                SetModData("BigImage", issue.Image.MediumUrl.ToString(),obj);
                SetModData("Name", $"{Volume} #{Number}", obj);
                SetModData("Description", issue.Name + " (Comic Book)",obj);
                SetModData("Id", ComicId);
#endif
                Helper.SetTickDelayedUpdateAction(1, () => checkLoad());
            }
        }
    }
}
