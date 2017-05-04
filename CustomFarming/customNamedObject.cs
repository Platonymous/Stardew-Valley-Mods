using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Objects;

using System;
using System.Drawing;
using System.IO;

using CustomElementHandler;
using System.Collections.Generic;

namespace CustomFarming
{
    public class customNamedObject : StardewValley.Object, ISaveElement, ISaveObject
    {
        public string description;
        public Texture2D tilesheet;
        public int tilesheetindex;
        public Microsoft.Xna.Framework.Rectangle sourceRectangle;
        public int tilesheetWidth;
        public Vector2 tileSize;
        public Microsoft.Xna.Framework.Color color;
        public string tilesheetpath;
        private bool inStorage;
        private GameLocation environment;

        public bool InStorage
        {
            get
            {
                return this.inStorage;
            }

            set
            {
                this.inStorage = value;
            }
        }

        public GameLocation Environment
        {
            get
            {
                return this.environment;
            }

            set
            {
                this.environment = value;
            }
        }

        public Vector2 Position
        {
            get
            {
                return this.tileLocation;
            }

            set
            {
                this.tileLocation = value;
            }
        }


        public new string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
            }
        }
        

        public override string getDescription()
        {
            string text = this.description;
            SpriteFont smallFont = Game1.smallFont;
            int width = Game1.tileSize * 4 + Game1.tileSize / 4;
            return Game1.parseText(text, smallFont, width);

        }


  
        public void rebuildFromSave(dynamic additionalSaveData)
        {

            build((int)additionalSaveData.baseID, (string)additionalSaveData.tilesheet, (int)additionalSaveData.tilesheetindex, (int)additionalSaveData.stack, (string)additionalSaveData.name, (string)additionalSaveData.description, (Microsoft.Xna.Framework.Color)additionalSaveData.color);
            if (additionalSaveData.quality != null)
            {
                this.quality = (int)additionalSaveData.quality;
            }
        }

        public override string DisplayName
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
            }
        }

        protected override string loadDisplayName()
        {
            return this.name;
        }

        public void build(int produceBaseIndex, string produceTilesheetPath, int produceTilesheetindex, int initialStack, string name, string description, Microsoft.Xna.Framework.Color color)
        {
            SaveHandler.register(this);
            this.tileLocation = Vector2.Zero;
            this.parentSheetIndex = produceBaseIndex;
            this.loadBaseObjectInformation();

            this.isRecipe = false;
            this.canBeSetDown = true;
            this.canBeGrabbed = true;
            this.isHoedirt = false;
            this.isSpawnedObject = false;
 
            this.stack = initialStack;
            
            this.name = name;
            this.description = description;

            if (produceTilesheetPath != "none")
            {
                this.tilesheetpath = produceTilesheetPath;
                Image produceTilesheetImage = Image.FromFile(produceTilesheetPath);
                Texture2D produceTilesheet = Bitmap2Texture(new Bitmap(produceTilesheetImage));
                this.tilesheet = produceTilesheet;
                this.tilesheetindex = produceTilesheetindex;
            }
            else
            {
                this.tilesheetpath = "none";
                this.tilesheet = Game1.objectSpriteSheet;
                this.tilesheetindex = produceBaseIndex;
            }

            this.tileSize = new Vector2(16, 16);
            this.tilesheetWidth = (int)(tilesheet.Width / tileSize.X);

            if (produceTilesheetPath != "none")
            {
                this.sourceRectangle = new Microsoft.Xna.Framework.Rectangle((this.tilesheetindex % tilesheetWidth) * (int)tileSize.X, (int)Math.Floor(this.tilesheetindex / this.tileSize.Y), (int)this.tileSize.X, (int)this.tileSize.Y);
            }
            else
            {
                this.sourceRectangle = Game1.getFarm().getSourceRectForObject(this.parentSheetIndex);
            }
           
            this.color = color;
           

            this.boundingBox = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * Game1.tileSize, (int)tileLocation.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize);
  
        }

        public void rebuildSoureceRect()
        {
            this.tileSize = new Vector2(16, 16);
            this.tilesheetWidth = (int)(tilesheet.Width / tileSize.X);
            this.sourceRectangle = new Microsoft.Xna.Framework.Rectangle((this.tilesheetindex % tilesheetWidth) * (int)tileSize.X, (int)Math.Floor(this.tilesheetindex / this.tileSize.Y), (int)this.tileSize.X, (int)this.tileSize.Y);
        }

        public void loadBaseObjectInformation()
        {
            string str;
            Game1.objectInformation.TryGetValue(parentSheetIndex, out str);
            try
            {
                if (str != null)
                {
                    string[] strArray1 = str.Split('/');
                    this.name = strArray1[0];
                    this.price = Convert.ToInt32(strArray1[1]);
                    this.edibility = Convert.ToInt32(strArray1[2]);
                    string[] strArray2 = strArray1[3].Split(' ');
                    this.type = strArray2[0];
                    if (strArray2.Length > 1)
                        this.category = Convert.ToInt32(strArray2[1]);
                }
            }
            catch (Exception ex)
            {
            }

        }

        public customNamedObject()
        {

        }

        public customNamedObject(int parentsheetIndex, int stack)
        {

        }

        public customNamedObject(int produceBaseIndex, string produceTilesheetPath, int produceTilesheetindex, int initialStack, string name, string description, Microsoft.Xna.Framework.Color color)
            :base(produceBaseIndex, initialStack)
        {
            this.build(produceBaseIndex, produceTilesheetPath, produceTilesheetindex, initialStack, name, description, color); 

        }

        public Texture2D Bitmap2Texture(Bitmap bmp)
        {

            MemoryStream s = new MemoryStream();

            bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
            s.Seek(0, SeekOrigin.Begin);
            Texture2D tx = Texture2D.FromStream(Game1.graphics.GraphicsDevice, s);

            return tx;

        }


        public void pickColorFromSprite(Texture2D sprite, int x, int y)
        {

            Bitmap b = Texture2Bitmap(sprite);
            System.Drawing.Color c = b.GetPixel(x, y);
            this.color = new Microsoft.Xna.Framework.Color(c.B,c.G,c.R);
        }

        private Bitmap Texture2Bitmap(Texture2D tex)
        {
            Texture2D texture = tex;
            byte[] textureData = new byte[4 * texture.Width * texture.Height];
            texture.GetData<byte>(textureData);

            Bitmap bmp = new Bitmap(
                           texture.Width, texture.Height,
                           System.Drawing.Imaging.PixelFormat.Format32bppArgb
                         );

            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
                           new System.Drawing.Rectangle(0, 0, texture.Width, texture.Height),
                           System.Drawing.Imaging.ImageLockMode.WriteOnly,
                           System.Drawing.Imaging.PixelFormat.Format32bppArgb
                         );

            IntPtr safePtr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
            bmp.UnlockBits(bmpData);

            return bmp;

        }

        public override Item getOne()
            
        {
            return new customNamedObject(parentSheetIndex, tilesheetpath, tilesheetindex, 1, name, description, color);
        }

        public override void drawInMenu(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
     
            spriteBatch.Draw(Game1.shadowTexture, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize * 3 / 4)), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Microsoft.Xna.Framework.Color.White * 0.5f, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
            spriteBatch.Draw(this.tilesheet, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), new Microsoft.Xna.Framework.Rectangle?(this.sourceRectangle), this.color * transparency, 0.0f, new Vector2(tileSize.X / 2, tileSize.Y / 2), (float)Game1.pixelZoom * ((double)scaleSize < 0.2 ? scaleSize : scaleSize), SpriteEffects.None, layerDepth);

            if (drawStackNumber && this.maximumStackSize() > 1 && ((double)scaleSize > 0.3 && this.Stack != int.MaxValue) && this.Stack > 1)
            {
                
                    Utility.drawTinyDigits(this.stack, spriteBatch, location + new Vector2((float)(Game1.tileSize - Utility.getWidthOfTinyDigitString(this.stack, 3f * scaleSize)) + 3f * scaleSize, (float)((double)Game1.tileSize - 18.0 * (double)scaleSize + 2.0)), 3f * scaleSize, 1f, Microsoft.Xna.Framework.Color.White);
             

            }

            if (drawStackNumber && this.quality > 0)
            {
                float num = this.quality < 4 ? 0.0f : (float)((Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1.0) * 0.0500000007450581);
                spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(12f, (float)(Game1.tileSize - 12) + num), new Microsoft.Xna.Framework.Rectangle?(this.quality < 4 ? new Microsoft.Xna.Framework.Rectangle(338 + (this.quality - 1) * 8, 400, 8, 8) : new Microsoft.Xna.Framework.Rectangle(346, 392, 8, 8)), Microsoft.Xna.Framework.Color.White * transparency, 0.0f, new Vector2(4f, 4f), (float)(3.0 * (double)scaleSize * (1.0 + (double)num)), SpriteEffects.None, layerDepth);
            }
            SaveHandler.drawInMenu(this);
        }


        public override void drawWhenHeld(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Vector2 objectPosition, StardewValley.Farmer f)
        {
            spriteBatch.Draw(this.tilesheet, objectPosition, new Microsoft.Xna.Framework.Rectangle?(this.sourceRectangle), this.color, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 2) / 10000f));
        }

        public override int salePrice()
        {
            return base.salePrice();
        }

        dynamic ISaveElement.getReplacement()
        {
            Chest chest = new Chest();
            StardewValley.Object item = new StardewValley.Object(tileLocation, parentSheetIndex);
            item.name = name;
            item.stack = stack;
            item.quality = quality;
            item.parentSheetIndex = parentSheetIndex;
            item.tileLocation = tileLocation;
            chest.items.Add(item);
            return chest;
            
        }

        Dictionary<string, string> ISaveElement.getAdditionalSaveData()
        {
            Dictionary<string, string> savedata = new Dictionary<string, string>();

            savedata.Add("color", color.R+"-"+color.G+"-"+color.B+"-"+color.A);
            savedata.Add("description", description);
            savedata.Add("tilepath", tilesheetpath);
            savedata.Add("tileindex", tilesheetindex.ToString());

            return savedata;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            StardewValley.Object item = (StardewValley.Object) (replacement as Chest).items[0];
            string[] c = additionalSaveData["color"].Split('-');
            dynamic additionalData = new { stack = item.stack, quality = item.quality, baseID = item.parentSheetIndex, color = new Microsoft.Xna.Framework.Color(int.Parse(c[0]), int.Parse(c[1]), int.Parse(c[2]), int.Parse(c[3])), name = item.name, description = additionalSaveData["description"], tilesheet = additionalSaveData["tilepath"], tilesheetindex = int.Parse(additionalSaveData["tileindex"]) };
            rebuildFromSave(additionalData);
        }
    }
}
