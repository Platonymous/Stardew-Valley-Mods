using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comics
{
    public class ComicBook : StardewValley.Object, ICustomObject
    {
        public Texture2D Texture { get; set; } = null;
        public bool Loaded { get; set; } = false;

        public bool UsePlaceholder { get; set; } = true;

        public string Description
        {
            get
            {
                return netName.Value.Split('>')[1];
            }
            set
            {
                var data = netName.Value.Split('>');
                data[1] = value;
                netName.Value = string.Join(">", data);
            }
        }

        public string Id
        {
            get
            {
                return netName.Value.Split('>')[2];
            }
            set
            {
                var data = netName.Value.Split('>');
                data[2] = value;
                netName.Value = string.Join(">", data);
            }
        }

        public string Volume
        {
            get
            {
                return netName.Value.Split('>')[3];
            }
            set
            {
                var data = netName.Value.Split('>');
                data[3] = value;
                netName.Value = string.Join(">", data);
            }
        }

        public string Number
        {
            get
            {
                return netName.Value.Split('>')[4];
            }
            set
            {
                var data = netName.Value.Split('>');
                data[4] = value;
                netName.Value = string.Join(">", data);
            }
        }

        public string SmallImage
        {
            get
            {
                return netName.Value.Split('>')[5];
            }
            set
            {
                var data = netName.Value.Split('>');
                data[5] = value;
                netName.Value = string.Join(">", data);
            }
        }

        public string BigImage
        {
            get
            {
                return netName.Value.Split('>')[6];
            }
            set
            {
                var data = netName.Value.Split('>');
                data[6] = value;
                netName.Value = string.Join(">", data);
            }
        }

        public override string Name
        {

            get
            {
                return netName.Value.Split('>')[0];
            }
            set
            {
                var data = netName.Value.Split('>');
                data[0] = value;
                netName.Value = string.Join(">", data);
            }
        }

        public override string DisplayName
        {
            get => Name;
            set => Name = value;
        }

        public ComicBook()
            : base()
        {
            if(string.IsNullOrEmpty(netName.Value))
                netName.Value = " > > > > > > ";
        }

        public ComicBook(string id)
            : this()
        {
            Issue issue = AssetManager.Instance.GetIssue(id);
            Id = id;
            Description = issue.Name + " (Comic Book)";
            Volume = issue.Volume.Name;
            Number = issue.IssueNumber;
            SmallImage = issue.Image.SmallUrl.ToString();
            BigImage = issue.Image.MediumUrl.ToString();
            Name = $"{Volume} #{Number}";
        }

        public override string getDescription()
        {
            SpriteFont smallFont = Game1.smallFont;
            int descriptionWidth = this.getDescriptionWidth();
            return Game1.parseText(Description, smallFont, descriptionWidth);
        }

        public virtual Dictionary<string, string> getAdditionalSaveData()
        {
            return new Dictionary<string, string>() { { "nameData", netName}, { "stack", Stack.ToString() } };
        }

        public virtual object getReplacement()
        {
            Chest c = new Chest(true);
            c.playerChoiceColor.Value = Color.Magenta;
            c.TileLocation = TileLocation;
            return c;
        }

        public void checkLoad()
        {
            if (Loaded || (!AssetManager.LoadImagesInShop && Game1.activeClickableMenu is ShopMenu && !Game1.player.hasItemInInventoryNamed(Name)))
                return;

            Loaded = true;

            Texture = !string.IsNullOrEmpty(SmallImage) && SmallImage != " " ? AssetManager.Instance.LoadImage(SmallImage,Id) : AssetManager.Instance.LoadImageForIssue(Id);
                   UsePlaceholder = false;
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            checkLoad();
            var texture = UsePlaceholder ? AssetManager.Instance.Placeholder : Texture;
            var offset = new Vector2(16f,8f);

            spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), color * 0.5f, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
            spriteBatch.Draw(texture, location + offset, null, color, 0.0f, Vector2.Zero, 3f, SpriteEffects.None, layerDepth + 0.0001f);
            if(Stack > 1)
            Utility.drawTinyDigits(Stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(Stack, 3f * scaleSize)) + 3f * scaleSize, (float)(64.0 - 18.0 * (double)scaleSize + 1.0)), 3f * scaleSize, 1f, color);
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
            var c = new ComicBook();
            c.netName.Value = netName.Value;
            return c;
        }

        public virtual void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
           
        }

        public virtual ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            var c = new ComicBook();
            c.netName.Value = additionalSaveData["nameData"];
            if (int.TryParse(additionalSaveData["stack"], out int nstack))
                c.Stack = nstack;
            return c;
        }
    }
}
