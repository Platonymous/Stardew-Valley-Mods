using System;
using System.Collections.Generic;
using System.Linq;

using StardewValley;
using StardewValley.Tools;
using StardewValley.TerrainFeatures;

using CustomElementHandler;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley.Objects;

namespace SeedBag
{
    class SeedBagTool : Hoe, ISaveElement
    {

        internal static Texture2D texture;
        private static Texture2D attTexture;
        private bool inUse;
        

        public Dictionary<string, string> getAdditionalSaveData()
        {
            Dictionary<string, string> savedata = new Dictionary<string, string>();
            savedata.Add("name", name);
            return savedata;
        }

        public dynamic getReplacement()
        {
            Chest replacement = new Chest(true);
            if(attachments.Count() > 0 && attachments[0] != null)
            {
                replacement.addItem(attachments[0]);
            }
            
            return replacement;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            build();
            Chest chest = (Chest)replacement;
            if (!chest.isEmpty())
            {
                attachments[0] = (StardewValley.Object) chest.items[0];
            }
            
        }

        public SeedBagTool()
            :base()
        {
            build();
        }

        public override string Name
        {
            get
            {

                return this.name;

            }
        }

        public override bool canBeTrashed()
        {
            return true;
        }

        internal static void loadTextures()
        {
            texture = SeedBagMod.mod.Helper.Content.Load<Texture2D>(@"Assets/seedbag.png");
            attTexture = SeedBagMod.mod.Helper.Content.Load<Texture2D>(@"Assets/seedattachment.png");
        }

        private void build()
        {
            name = "Seed Bag";
            description = "Empty";

            numAttachmentSlots = 1;
            attachments = new StardewValley.Object[numAttachmentSlots];
            initialParentTileIndex = 77;
            currentParentTileIndex = 77;
            indexOfMenuItemView = 0;
            upgradeLevel = 5;
           

            instantUse = false;
            inUse = false;
        }

        public override int attachmentSlots()
        {
            return numAttachmentSlots;
        }
        
        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            spriteBatch.Draw(texture, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), new Rectangle?(Game1.getSquareSourceRectForNonStandardTileSheet(texture, Game1.tileSize / 4, Game1.tileSize / 4, this.indexOfMenuItemView)), Color.White * transparency, 0f, new Vector2((float)(Game1.tileSize / 4 / 2), (float)(Game1.tileSize / 4 / 2)), (float)Game1.pixelZoom * scaleSize, SpriteEffects.None, layerDepth);

            if (inUse)
            {
                StardewValley.Farmer f = Game1.player;
                Vector2 vector = f.getLocalPosition(Game1.viewport) + f.jitter + f.armOffset;
                int num = (int)vector.Y - ((Game1.tileSize * 5)/2);
                Game1.spriteBatch.Draw(texture, new Vector2(vector.X, (float)num), new Rectangle?(Game1.getSquareSourceRectForNonStandardTileSheet(texture, Game1.tileSize / 4, Game1.tileSize / 4, this.indexOfMenuItemView)), Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom * scaleSize, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + Game1.tileSize / 2) / 10000f));

            }
        }

        public override void drawAttachments(SpriteBatch b, int x, int y)
        {
            Rectangle attachementSourceRectangle = new Rectangle(0, 0, 64, 64);
            b.Draw(attTexture, new Vector2((float)x, (float)y), new Microsoft.Xna.Framework.Rectangle?(attachementSourceRectangle), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
            
            if (attachments.Count() > 0 && attachments[0] is StardewValley.Object)
            {
                attachments[0].drawInMenu(b, new Vector2((float)x, (float)y), 1f);
            }
        }

        public override bool onRelease(GameLocation location, int x, int y, StardewValley.Farmer who)
        {
            inUse = false;
            return base.onRelease(location, x, y, who);
        }

        public override bool beginUsing(GameLocation location, int x, int y, StardewValley.Farmer who)
        {
            inUse = true;
            return base.beginUsing(location, x, y, who);
        }

        public override bool canThisBeAttached(StardewValley.Object o)
        {
            if (o == null || o.category == -74 || o.category == -19) { return true; } else { return false; }
        }


        public override StardewValley.Object attach(StardewValley.Object o)
        {
            StardewValley.Object priorAttachement = (StardewValley.Object)null;

            if (attachments.Count() > 0 && attachments[0] != null)
            {
                priorAttachement = (StardewValley.Object) attachments[0];
            }

            if (o == null)
            {
                attachments[0] = (StardewValley.Object)null;
                Game1.playSound("dwop");

                return priorAttachement;
            }

            if (canThisBeAttached(o))
            {
                attachments[0] = o;
                Game1.playSound("button1");

                return priorAttachement;
            }
 
            return (StardewValley.Object)null;
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, StardewValley.Farmer who)
        {
            if(attachments.Count() == 0 || attachments[0] == null)
            {
                Game1.showRedMessage("Out of seeds");
                return;
            }

            who.Stamina -= (float)(2 * power) - (float)who.FarmingLevel * 0.1f;
            power = who.toolPower;
            who.stopJittering();
            Game1.playSound("leafrustle");
            Vector2 vector = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
            List<Vector2> list = base.tilesAffected(vector, power, who);
            foreach (Vector2 current in list)
            {
                if (location.terrainFeatures.ContainsKey(current) && location.terrainFeatures[current] is HoeDirt hd && hd.crop == null)
                {
                    hd.plant(attachments[0].ParentSheetIndex, (int) current.X, (int) current.Y,who);
                    attachments[0].stack--;
                    if (attachments[0].stack == 0)
                    {
                        attachments[0] = null;
                        Game1.showRedMessage("Out of seeds");
                        break;
                    }
                }
            }

        }

       
        public override string getDescription()
        {

            if (attachments.Count() > 0 && attachments[0] != null)
            {
                return attachments[0].name;
            }
            else
            {
                string text = description;
                SpriteFont smallFont = Game1.smallFont;
                int width = Game1.tileSize * 4 + Game1.tileSize / 4;
                return Game1.parseText(text, smallFont, width);
               
            }
           

        }

        protected override string loadDisplayName()
        {
            return name;
        }

        protected override string loadDescription()
        {
            return getDescription();
            
        }



       


    }
}
