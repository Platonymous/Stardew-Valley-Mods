using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

using StardewValley;
using StardewValley.Tools;
using StardewValley.Objects;
using StardewValley.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json.Linq;
using xTile.Layers;
using xTile.Tiles;
using xTile.Dimensions;
using xTile;

namespace CustomFarming
{
    class simpleMachine : StardewValley.Object , ISaveObject, ICustomFarmingObject
    {

        public Texture2D tilesheet;
        public int tilesheetindex;
        public int tileindex;
        public Vector2 tileSize;
        public string description;
        public int productionTime;
        public Item produce;
        public bool usePrefix;
        public string categoryName;
        public string produceName;
        public new Item heldObject;
        List<StardewValley.Object> materials;
        public Microsoft.Xna.Framework.Rectangle sourceRectangle;
        public int tilesheetWidth;
        public int animationFrames;
        public int animationSpeed;
        public int animationFrame;
        public int workAnimationOffset;
        public int workAnimationFrames;
        public bool animateWork;
        public bool animate;
        public bool isWorking;
        public string prefix;
        public StardewValley.Object lastDropIn;
        public string modFolder;
        public dynamic loadJson;
        public int requiredStack;
        public int starterMaterial;
        public int starterMaterialStack;
        public int produceStack;
        public JArray specialProduce;
        public bool isSpecial;
        public bool specialPrefix;
        public bool specialSuffix;
        public string filename;
        public bool useColor;
        public int mil;
        public bool displayItem;
        public int displayItemX;
        public int displayItemY;
        public bool takeAll;
        public bool useSuffix;
        public string suffix;
        public double displayItemZoom;
        public int produceID;
        public int produceIndex;
        public int tileWidth;
        public int tileHeight;
        public int menuTileIndex;
        public int produceQuality;
        public int readyTile;
        public string crafting;
        public int craftingid;

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

        public Texture2D Texture
        {
            get
            {
                return tilesheet;
            }

        }

        public Microsoft.Xna.Framework.Rectangle SourceRectangle
        {
            get
            {
                return this.sourceRectangle;
            }

        }

        public new string Name
        {
            get
            {
                return base.Name;
            }

            set
            {
                this.name = value;
            }
        }

        public StardewValley.Object getReplacement()
        {
            StardewValley.Object replacement = new Chest(true);
            return replacement;
        }


        public dynamic getAdditionalSaveData()
        {
            int lastDropInPSI = 0;
            int lastDropInQ = 0;
            if (this.lastDropIn != null)
            {
                lastDropInPSI = this.lastDropIn.parentSheetIndex;
                lastDropInQ = this.lastDropIn.quality;
            }
            dynamic additionalData = new { FileName = this.filename, ModFolder = this.modFolder, LastDropIn = lastDropInPSI, LastDropInQuality = lastDropInQ, Ready = this.readyForHarvest, Minutes = this.minutesUntilReady, Working = this.isWorking };
            return additionalData;
        }

        public void rebuildFromSave(dynamic additionalSaveData)
        {
            string m = (string)additionalSaveData.ModFolder;
            string f = (string)additionalSaveData.FileName;
            this.build(m, f);
            if (additionalSaveData.LastDropIn != null && additionalSaveData.Minutes != null && additionalSaveData.Working != null && additionalSaveData.Ready != null)
            {

                if (additionalSaveData.LastDropIn != null)
                {
                    int lastDropInPSI = (int)additionalSaveData.LastDropIn;
                    if (lastDropInPSI != 0)
                    {
                        this.lastDropIn = new StardewValley.Object(lastDropInPSI, this.requiredStack);
                        this.prefix = this.lastDropIn.name;
                        this.suffix = this.lastDropIn.name;
                    }

                    if (additionalSaveData.LastDropInQuality != null && this.lastDropIn != null)
                    {
                        this.lastDropIn.quality = (int)additionalSaveData.LastDropInQuality;
                    }
                }
                

                

                if ((bool)additionalSaveData.Working || (bool) additionalSaveData.Ready)
                {
                    startWorking();
                }

                this.minutesUntilReady = (int)additionalSaveData.Minutes - 480;
                if (this.minutesUntilReady < 1)
                {
                    this.minutesUntilReady = 0;
                }

            }

            
        }

        public void build(string modFolder, string filename)
        {
            SaveHandler.register(this);
            this.tileLocation = Vector2.Zero;
            this.modFolder = modFolder;
            this.filename = filename;
            string path = Path.Combine(modFolder, filename);
            this.loadJson = JObject.Parse(File.ReadAllText(path));
            this.takeAll = false;
            this.tileSize = new Vector2(16, 32);
            this.category = -8;
            this.mil = 0;
            this.bigCraftable = true;
            this.isRecipe = false;
            this.animationFrame = 0;
            this.animationFrames = 1;
            this.animationSpeed = 300;

            this.crafting = "388 30";

            if (loadJson.Crafting != null)
            {
                this.crafting = (string)loadJson.Crafting;
            }

            if(loadJson.AnimationSpeed != null)
            {
                this.animationSpeed = (100 - (int)loadJson.AnimationSpeed) * 10;
                if (this.animationSpeed <= 0)
                {
                    this.animationSpeed = 1;
                }
            }

            this.displayItem = false;
            this.displayItemX = 0;
            this.displayItemY = 0;
            this.displayItemZoom = 1.0;

                if (loadJson.displayItem != null && loadJson.displayItemX != null && loadJson.displayItemY != null && loadJson.displayItemZoom != null)
            {
           
                this.displayItem = (bool)loadJson.displayItem;
                this.displayItemX = (int)loadJson.displayItemX;
                this.displayItemY = (int)loadJson.displayItemY;
                this.displayItemZoom = (double)loadJson.displayItemZoom;

            }

                this.isWorking = false;
            this.parentSheetIndex = -1;
            this.readyForHarvest = false;
            this.prefix = "";
            this.suffix = "";
            this.type = "Crafting";
            this.isSpecial = false;

            string tilesheetFile = Path.Combine(modFolder, (string)loadJson.Tilesheet);
            Image tilesheetImage = Image.FromFile(tilesheetFile);
            this.tilesheet = Bitmap2Texture(new Bitmap(tilesheetImage));

            this.tileWidth = 16;
            

            if (loadJson.TileWidth != null)
            {
                this.tileWidth = (int)loadJson.TileWidth;
            }

            this.tileHeight = tileWidth * 2;

            this.tilesheetWidth = (int)(tilesheet.Width / tileWidth);

            this.boundingBox = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * Game1.tileSize, (int)tileLocation.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize);
            this.workAnimationFrames = 0;
            if (loadJson.WorkAnimationFrames != null)
            {
                this.workAnimationFrames = (int)loadJson.WorkAnimationFrames;
            }

            this.workAnimationOffset = (this.workAnimationFrames > 0) ? 1 : 0;

            this.animate = (this.animationFrames > 1) ? true : false;
            this.animateWork = (this.workAnimationFrames > 1) ? true : false;

            this.tilesheetindex = (int)loadJson.TileIndex;
            this.tileindex = this.tilesheetindex;
            this.readyTile = this.tileindex;
            this.menuTileIndex = this.tileindex;



            if (loadJson.MenuTileIndex != null)
            {

                this.menuTileIndex = (int)loadJson.MenuTileIndex;
            }

            if (loadJson.ReadyTileIndex != null)
            {

                this.readyTile = (int)loadJson.ReadyTileIndex;
            }


            this.name = (string)loadJson.Name;
            this.categoryName = (string)loadJson.CategoryName;

            this.description = (string)loadJson.Description;
            this.produceQuality = 0;

            if (loadJson.Produce != null && loadJson.Produce.Name != "")
            {

                string produceTilesheetFile = "none";
                this.produceIndex = 0;
                if (loadJson.Produce.Tilesheet != null)
                {
                    produceTilesheetFile = Path.Combine(modFolder, (string)loadJson.Produce.Tilesheet);
                    this.produceIndex = (int)loadJson.Produce.TileIndex;
                }
                this.produceID = (int)loadJson.Produce.ProduceID;
                
                this.produceStack = (int)loadJson.Produce.Stack;
                this.produce = new customNamedObject(this.produceID, produceTilesheetFile, this.produceIndex, this.produceStack, (string)loadJson.Produce.Name, (string)loadJson.Produce.Description, Microsoft.Xna.Framework.Color.White);

                if (loadJson.Produce != null && loadJson.Produce.Quality != null)
                {
                    this.produceQuality = (int)loadJson.Produce.Quality;
                }

                (this.produce as StardewValley.Object).quality = this.produceQuality;

                    this.produceName = (string)loadJson.Produce.Name;
            }

            this.usePrefix = false;

            if (loadJson.Produce != null && loadJson.Produce.usePrefix != null)
            {
                this.usePrefix = (bool)loadJson.Produce.usePrefix;
            }



            this.useSuffix = false;

            if (loadJson.Produce != null && loadJson.Produce.useSuffix != null) { 
            this.useSuffix = (bool)loadJson.Produce.useSuffix;
            }

            this.useColor = false;

            if (loadJson.Produce != null && loadJson.Produce.useColor != null)
            {
                this.useColor = (bool)loadJson.Produce.useColor;
            }

            this.productionTime = 0;

            if (loadJson.Produce != null && loadJson.Produce.ProductionTime != null)
            {
                this.productionTime = (int)loadJson.Produce.ProductionTime;
            }

            this.requiredStack = 0;
            if (loadJson.Produce != null && loadJson.RequieredStack != null)
            {
                this.requiredStack = (int)loadJson.RequieredStack;
            }
            

            this.specialProduce = (JArray)loadJson.SpecialProduce;


            this.starterMaterial = 0;
            this.starterMaterialStack = 0;

            if(loadJson.StarterMaterial != null && loadJson.StarterMaterialStack != null) { 
            this.starterMaterial = (int)loadJson.StarterMaterial;
            this.starterMaterialStack = (int)loadJson.StarterMaterialStack;
            }

            updateSourceRectangle();

            

            this.materials = new List<StardewValley.Object>();

            if (loadJson.Materials != null)
            {
                JArray loadMaterials = (JArray)loadJson.Materials;

                if (loadMaterials.Count > 0)
                {
                    foreach (var material in loadMaterials)
                    {
                        int m = (int)material;

                        if (m == -999)
                        {
                            this.takeAll = true;
                        }

                        if (m > 0)
                        {
                            materials.Add(new StardewValley.Object(m, 1));
                        }
                        else
                        {
                            foreach (int keyI in Game1.objectInformation.Keys)
                            {

                                string[] splitData = Game1.objectInformation[keyI].Split('/');

                                string[] splitCategory = splitData[3].Split(' ');
                                if (splitCategory.Length > 1)
                                {
                                    int categoryInt = 0;
                                    int.TryParse(splitCategory[1], out categoryInt);

                                    if (categoryInt == m)
                                    {
                                        materials.Add(new StardewValley.Object(keyI, 1));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
        }

        public simpleMachine()
        {

           
        }

        public simpleMachine(string modFolder, string filename)
        {
            this.build(modFolder, filename);

        }

        public Texture2D Bitmap2Texture(Bitmap bmp)
        {

            MemoryStream s = new MemoryStream();

            bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
            s.Seek(0, SeekOrigin.Begin);
            Texture2D tx = Texture2D.FromStream(Game1.graphics.GraphicsDevice, s);

            return tx;

        }

        public override bool isPassable()
        {
            return false;
        }

        public override Microsoft.Xna.Framework.Rectangle getBoundingBox(Vector2 tileLocation)
        {
            this.boundingBox.X = (int)tileLocation.X * Game1.tileSize;
            this.boundingBox.Y = (int)tileLocation.Y * Game1.tileSize;
           
            return this.boundingBox;
        }

        public override bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            if(!l.objects.ContainsKey(tile))
            {
            
                return !l.isTileOccupiedForPlacement(tile, this);
                
            }
            else
            {
                return false;
            }

        }


        public override bool canBePlacedInWater()
        {
            return false;
        }

        
        public new Vector2 getScale()
        {
            if (this.workAnimationFrames > 1)
            {
                return Vector2.Zero;
            }
            else
            {
 
                if (this.heldObject == null && this.minutesUntilReady <= 0 || this.readyForHarvest)
                    return Vector2.Zero;
               
                this.scale.X -= 0.1f;
                this.scale.Y += 0.1f;
                if ((double)this.scale.X <= 0.0)
                { 
                    this.scale.X = 10f;
                }
                if ((double)this.scale.Y >= 10.0)
                {
                    this.scale.Y = 0.0f;
                }
                    
                return new Vector2(Math.Abs(this.scale.X - 5f), Math.Abs(this.scale.Y - 5f)); ;
            }
           
        }
        

        public bool deliverProduce(StardewValley.Farmer who)
        {

            if(lastDropIn != null)
            {
                (this.heldObject as StardewValley.Object).price += (lastDropIn.price * requiredStack)/(this.heldObject as StardewValley.Object).Stack;
            }

            if (who.IsMainPlayer && !who.addItemToInventoryBool((Item)this.heldObject, false))
            {
                Game1.showRedMessage("Inventory Full");
                return false;
            }

            this.readyForHarvest = false;
            this.lastDropIn = null;
            this.heldObject = null;
            this.isSpecial = false;

            if (this.materials.Count == 0)
            {
                this.startWorking();
            }

            Game1.playSound("coin");
            return true;

        }


        public override bool checkForAction(StardewValley.Farmer who, bool justCheckingForActivity = false)
        {

            if (this.categoryName == "Mailbox")
            {
                if (justCheckingForActivity) {
                                      return true;
                }
                this.shakeTimer = 100;

                if (Game1.mailbox.Count != 0)
                {
                    Game1.getFarm().mailbox();
                    if (Game1.mailbox.Count == 0)
                    {
                        this.isWorking = false;
                    }
                }
                else
                {
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8429"));
                }

                return true;
            }


            if ((this.description.Contains("arecrow") || this.getCategoryName() == "Scarescrow") && this.produce == null)
            {
                if (justCheckingForActivity) { return true; }
                this.shakeTimer = 100;
                
                if (this.specialVariable == 0)
                {
                    Game1.drawObjectDialogue("I haven't encountered any crows yet.");
                }
                else
                {
                    Game1.drawObjectDialogue("I've scared off " + (object)this.specialVariable + " crow" + (this.specialVariable == 1 ? "." : "s."));
                }
                    
                return true;
            }

            if (this.heldObject == null)
            {
              
                bool check = (this.materials.FindIndex(x => (who.ActiveObject is StardewValley.Object) && x.parentSheetIndex == who.ActiveObject.parentSheetIndex) == -1) ? false : true;
                
                return check;
            }

            if (!this.readyForHarvest)
            {
                return false;
            }

            if (justCheckingForActivity)
            {
                return true;
            }

            this.deliverProduce(who);
            return true;
        }
  
        public override void DayUpdate(GameLocation location)
        {


        }

        public override void draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {

            Vector2 vector2 = this.getScale() * (float)Game1.pixelZoom;
            Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize), (float)(y * Game1.tileSize - Game1.tileSize)));
            Microsoft.Xna.Framework.Rectangle destinationRectangle = new Microsoft.Xna.Framework.Rectangle((int)((double)local.X - (double)vector2.X / 2.0) + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((double)local.Y - (double)vector2.Y / 2.0) + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((double)Game1.tileSize + (double)vector2.X), (int)((double)(Game1.tileSize * 2) + (double)vector2.Y / 2.0));

            if (this.readyForHarvest)
            {
                this.tileindex = this.readyTile;
                updateSourceRectangle();
            }

            
            spriteBatch.Draw(this.tilesheet, destinationRectangle, new Microsoft.Xna.Framework.Rectangle?(this.sourceRectangle), Microsoft.Xna.Framework.Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, (float)((double)Math.Max(0.0f, (float)((y + 1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + (double) x * 9.99999974737875E-06));

            if (this.displayItem && this.lastDropIn != null && !this.readyForHarvest)
            {
            
                Microsoft.Xna.Framework.Rectangle displayDestinationRectangle = new Microsoft.Xna.Framework.Rectangle((int)((double)local.X - (double)vector2.X / 2.0)+ this.displayItemX + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((double)local.Y - (double)vector2.Y / 2.0) + this.displayItemY + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)( this.displayItemZoom * ( (int)((double)Game1.tileSize + (double)vector2.X) )), (int) (this.displayItemZoom * ( (int)((double)(Game1.tileSize) + (double)vector2.Y / 2.0))));

                spriteBatch.Draw(Game1.objectSpriteSheet, displayDestinationRectangle, new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.lastDropIn.parentSheetIndex, 16, 16)), Microsoft.Xna.Framework.Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, (float)((double)Math.Max(0.0f, (float)((y+1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + (double) (x+1) * 9.99999974737875E-06));

            }

            if (this.readyForHarvest && this.heldObject != null) {
                    customNamedObject cno = (this.heldObject as customNamedObject);
                    float num = (float)(4.0 * Math.Round(Math.Sin(DateTime.Now.TimeOfDay.TotalMilliseconds / 250.0), 2));
                    spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize - 8), (float)(y * Game1.tileSize - Game1.tileSize * 3 / 2 - 16) + num)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24)), Microsoft.Xna.Framework.Color.White * 0.75f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((double)((y + 1) * Game1.tileSize) / 10000.0 + 9.99999997475243E-07 + (double)this.tileLocation.X / 10000.0 + (this.parentSheetIndex == 105 ? 0.00150000001303852 : 0.0)));
                    spriteBatch.Draw(cno.tilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize + Game1.tileSize / 2), (float)(y * Game1.tileSize - Game1.tileSize - Game1.tileSize / 8) + num)), new Microsoft.Xna.Framework.Rectangle?(cno.sourceRectangle), cno.color * 0.75f, 0.0f, new Vector2(8f, 8f), (float)Game1.pixelZoom, SpriteEffects.None, (float)((double)((y + 1) * Game1.tileSize) / 10000.0 + 9.99999974737875E-06 + (double)this.tileLocation.X / 10000.0 + 0.0));
                }

            if (this.categoryName == "Mailbox" && Game1.mailbox.Count > 0 && this.inStorage == false && this.animateWork == false)
            {
                float num = (float)(4.0 * Math.Round(Math.Sin(DateTime.Now.TimeOfDay.TotalMilliseconds / 250.0), 2));
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileLocation.X * Game1.tileSize), (float)(this.tileLocation.Y * Game1.tileSize - Game1.tileSize * 3 / 2 - 48) + num)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24)), Microsoft.Xna.Framework.Color.White * 0.75f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((double)(17 * Game1.tileSize) / 10000.0 + 9.99999997475243E-07 + 0.00680000009015203));
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileLocation.X * Game1.tileSize + Game1.tileSize / 2 + Game1.pixelZoom), (float)(this.tileLocation.Y * Game1.tileSize - Game1.tileSize - 24 - Game1.tileSize / 8) + num)), new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(189, 423, 15, 13)), Microsoft.Xna.Framework.Color.White, 0.0f, new Vector2(7f, 6f), 4f, SpriteEffects.None, (float)((double)(17 * Game1.tileSize) / 10000.0 + 9.99999974737875E-06 + 0.00680000009015203));
            }

            SaveHandler.draw(this, new Vector2(x,y));
        }


        public override void draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {

            this.draw(spriteBatch, xNonTile, yNonTile, alpha);

        }
        
        public override void drawInMenu(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            this.tileindex = this.menuTileIndex;
            updateSourceRectangle();
            spriteBatch.Draw(this.tilesheet, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), new Microsoft.Xna.Framework.Rectangle?(this.sourceRectangle), Microsoft.Xna.Framework.Color.White * transparency, 0.0f, new Vector2(this.tileWidth/2, this.tileWidth), (float)Game1.pixelZoom * ((double)scaleSize < 0.2 ? scaleSize : scaleSize / 2.00f), SpriteEffects.None, layerDepth);
            SaveHandler.drawInMenu(this);
        }


        public override void drawWhenHeld(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Vector2 objectPosition, StardewValley.Farmer f)
        {
            spriteBatch.Draw(this.tilesheet, objectPosition, new Microsoft.Xna.Framework.Rectangle?(this.sourceRectangle), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (float)(f.getStandingY() + 2) / 10000f));
        }


        public override Microsoft.Xna.Framework.Color getCategoryColor()
        {
            return Microsoft.Xna.Framework.Color.Magenta;
        }

        public override string getCategoryName()
        {

            return this.categoryName;

        }

        public override string getDescription()
        {
            string text = this.description;
            SpriteFont smallFont = Game1.smallFont;
            int width = Game1.tileSize * 4 + Game1.tileSize / 4;
            return Game1.parseText(text, smallFont, width);

        }

        public override bool isActionable(StardewValley.Farmer who)
        {
            return this.checkForAction(who, true);
        }

        public override bool isPlaceable()
        {
            return true;
        }
       
        public override bool minutesElapsed(int minutes, GameLocation environment)
        {
            this.minutesUntilReady = this.minutesUntilReady - minutes;

            if (this.minutesUntilReady <= 0 && this.heldObject != null && !this.readyForHarvest && Game1.currentLocation.Equals((object)environment))
            {
                    Game1.playSound("dwop");
                    this.readyForHarvest = true;
                    this.minutesUntilReady = 0;
                    this.isWorking = false;
            }

            if (!this.readyForHarvest && this.materials.Count == 0 && !this.isWorking && this.categoryName != "Mailbox")
            {

                this.startWorking();
            }

            if(this.categoryName == "Mailbox" && Game1.mailbox.Count > 0)
            {
                this.isWorking = true;
            }
            else if(this.categoryName == "Mailbox" && Game1.mailbox.Count == 0)
            {
                this.isWorking = false;
            }

                return false;
        }
       
        public override bool performDropDownAction(StardewValley.Farmer who)
        {
          if(this.materials.Count == 0)
            {
                startWorking();
            }

            return false;
        }


        public override bool performObjectDropInAction(StardewValley.Object dropIn, bool probe, StardewValley.Farmer who)
        {

            if (dropIn != null && dropIn.bigCraftable || this.heldObject != null)
            {
                return false;
            }
            
            if ((dropIn == null || (dropIn != null & this.materials.FindIndex(x => x.parentSheetIndex == dropIn.parentSheetIndex) == -1)) && !this.takeAll)
            {
                return false;
            }

            if (dropIn.stack < this.requiredStack)
            {
                if (!probe) { 
                Game1.showRedMessage("Requieres " + this.requiredStack + " " + dropIn.name);
                }
                return false;
            }

            if (this.starterMaterial != 0 && who.IsMainPlayer && who.getTallyOfObject(this.starterMaterial, false) <= 0)
            {
                if (!probe)
                {
                    Game1.showRedMessage("Requieres " + this.starterMaterialStack + " " + new StardewValley.Object(this.starterMaterial, 1).name);
                }
                return false;
            }

            int sum = this.starterMaterialStack + this.requiredStack;

            if (dropIn.parentSheetIndex == this.starterMaterial && dropIn.Stack < sum)
            {
                if (!probe)
                { 
                    Game1.showRedMessage("Requieres " + sum + " " + new StardewValley.Object(this.starterMaterial, 1).name);
                }
                return false;
            }


            if (!probe)
            {
                this.lastDropIn = dropIn;
                this.prefix = dropIn.name;
                this.suffix = dropIn.name;


                if (this.starterMaterial != 0 && dropIn.parentSheetIndex == this.starterMaterial)
                {
           
                    if (dropIn.Stack <= (sum - 1))
                    {
                        who.removeItemFromInventory((Item)dropIn);
                    }
                    else
                    {
                        dropIn.Stack -= (sum - 1);
                    }
                    startWorking();
                    Game1.playSound("Ship");
                    return true;
                }
                else
                {
                    if (dropIn.Stack <= (this.requiredStack-1))
                    {
                        who.removeItemFromInventory((Item)dropIn);
                    }
                    else
                    {
                        dropIn.Stack -= (this.requiredStack-1);
                    }
                }


                if (this.starterMaterial != 0 && dropIn.parentSheetIndex != this.starterMaterial)
                {
                    int count = 0;
                    while (count < this.starterMaterialStack)
                    {
                        int index = who.items.FindIndex(x => x != null && x is StardewValley.Object && x.parentSheetIndex == this.starterMaterial);
                        if(index == -1)
                        {
                            break;
                        }

                        if (who.items[index].Stack <= 1)
                        {
                            who.Items[index] = (Item)null;
                        }
                        else
                        { 
                        who.items[index].Stack--;
                        }
                        count++;
                    }

                }

                startWorking();
                Game1.playSound("Ship");
                return true;
            }

            return true;
            
        }

        public override Item getOne()
        {
            return new simpleMachine(modFolder, filename);
        }

        public override bool performToolAction(Tool t)
        {

            if (t == null || !t.isHeavyHitter() || t is MeleeWeapon || !(t is Pickaxe))
            {
                return false;
            }

            this.tileindex = this.menuTileIndex;
          
            this.isWorking = false;
            this.lastDropIn = (StardewValley.Object)null;
            this.heldObject = (StardewValley.Object)null;
            this.readyForHarvest = false;
            this.minutesUntilReady = -1;


            Game1.playSound("hammer");
            Game1.currentLocation.objects.Remove(this.tileLocation);
            Game1.createItemDebris(this, this.tileLocation * (float)Game1.tileSize, -1, (GameLocation)null);


            
            return false;

        }

        public void checkForSpecialProduce()
        {


            this.specialPrefix = false;
            this.isSpecial = false;

            if (this.specialProduce != null && this.specialProduce.Count > 0)
            {

                foreach (dynamic p in this.specialProduce)
                {
                    int m = (int)p.Material;
                    if (this.lastDropIn.parentSheetIndex == m || this.lastDropIn.category == m)
                    {
                        if (p.MaterialQuality != null && this.lastDropIn.quality != (int) p.MaterialQuality)
                        {
                            continue;
                        }

                        if (p.Name != null)
                        {
                            (this.heldObject as customNamedObject).Name = (string)p.Name;
                        }

                        if(p.Stack != null)
                        {
                            (this.heldObject as customNamedObject).Stack = (int)p.Stack;
                        }

                        if (p.usePrefix != null) { 
                        this.specialPrefix = (bool)p.usePrefix;
                        }

                        if (p.useSuffix != null)
                        {
                            this.specialSuffix = (bool)p.useSuffix;
                        }

                        if (p.TileIndex != null)
                        {
                            (this.heldObject as customNamedObject).tilesheetindex = p.TileIndex;
                            (this.heldObject as customNamedObject).rebuildSoureceRect();
                        }

                        if (p.ProduceID != null)
                        {
                            (this.heldObject as customNamedObject).parentSheetIndex = p.ProduceID;
                            this.heldObject = new customNamedObject((this.heldObject as customNamedObject).parentSheetIndex, (this.heldObject as customNamedObject).tilesheetpath, (this.heldObject as customNamedObject).tilesheetindex, (this.heldObject as customNamedObject).Stack, (this.heldObject as customNamedObject).name, (this.heldObject as customNamedObject).description, (this.heldObject as customNamedObject).color);

                        }

                        if (p.Quality != null)
                        {
                            (this.heldObject as StardewValley.Object).quality = (int)p.Quality;
                        }

                        this.isSpecial = true;
                      
                    }

                }
            }

        

        }

        public void buildProduce()
        {

            this.heldObject = produce.getOne();
            this.heldObject.Stack = produce.Stack;
            (this.heldObject as customNamedObject).Name = this.produceName;
            (this.heldObject as StardewValley.Object).quality = this.produceQuality;

            if (this.materials.Count == 0)
            {
                return;
            }

            checkForSpecialProduce();

            if((this.heldObject as StardewValley.Object).quality == -1)
            {
                (this.heldObject as StardewValley.Object).quality = this.lastDropIn.quality;
            }

            if ((this.usePrefix && !this.isSpecial) || (this.isSpecial && this.specialPrefix))
            {
                (this.heldObject as customNamedObject).Name = this.prefix + " " + this.heldObject.Name;
            }

            if ((this.useSuffix && !this.isSpecial) || (this.isSpecial && this.specialSuffix))
            {
                (this.heldObject as customNamedObject).Name = this.heldObject.Name + " " + this.suffix;
            }
        }

        public void startWorking()
        {

            if (this.produce == null) { this.isWorking = true; return; }

            buildProduce();

            this.minutesUntilReady = this.productionTime;

            
            this.isWorking = true;

            if (this.materials.Count == 0)
            {
                return;
            }


            if (this.heldObject != null && this.lastDropIn != null)
            {             
                int x = (this.lastDropIn.parentSheetIndex % (Game1.objectSpriteSheet.Width / 16)) * 16 + 7;
                int y = (int) Math.Floor((decimal)(this.lastDropIn.parentSheetIndex / (Game1.objectSpriteSheet.Width/16))) * 16 + 7;

                if (this.useColor)
                {
                    (this.heldObject as customNamedObject).pickColorFromSprite(Game1.objectSpriteSheet, x, y);
                }

                if (this.specialProduce != null && this.specialProduce.Count > 0)
                {

                    foreach (dynamic p in this.specialProduce)
                    {
                        int m = (int)p.Material;
                        if (this.lastDropIn.parentSheetIndex == m || this.lastDropIn.category == m)
                        {
                            if(p.ProductionTime != null)
                            {
                                this.minutesUntilReady = (int)p.ProductionTime;
                            }
                        }
                    }
                }

            }

            
        }
      
        public override bool placementAction(GameLocation location, int x, int y, StardewValley.Farmer who = null)
        {
            Vector2 index1 = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
            this.tileindex = this.tilesheetindex;
            if (location.objects.ContainsKey(index1))
            {
                return false;
            }

            StardewValley.Object placeObject = (StardewValley.Object)this.getOne();
            placeObject.tileLocation = index1;

            location.objects[index1] = placeObject;

           

            if (this.materials.Count == 0 && this.categoryName != "Mailbox")
            {
                (placeObject as simpleMachine).startWorking();
            }
           
            
            return true;
        }

        public void updateSourceRectangle()
        {
            this.sourceRectangle = new Microsoft.Xna.Framework.Rectangle((this.tileindex % this.tilesheetWidth) * this.tileWidth, (int)Math.Floor((double)this.tilesheetindex / this.tileHeight), (int)this.tileWidth, (int)this.tileHeight);

        }


        public override void updateWhenCurrentLocation(GameTime time)
        {

            if (this.shakeTimer > 0)
            {
                this.shakeTimer = this.shakeTimer - time.ElapsedGameTime.Milliseconds;
                if (this.shakeTimer <= 0)
                    this.health = 10;
            }

       
            if (!(time.TotalGameTime.TotalMilliseconds % this.animationSpeed <= 10.0))
            {
                return;
            }


            if (this.isWorking && this.animateWork)
            {
                this.animationFrame++;
                if (this.animationFrame >= this.workAnimationFrames)
                {
                    this.animationFrame = 0;
                }

            }
            else if (!this.isWorking || !this.animateWork)
            {
                this.animationFrame = 0;
                this.tileindex = this.tilesheetindex;
            }
            
            if (!this.isWorking)
            {
                this.tileindex = this.tilesheetindex + this.animationFrame;
            }
            else
            {
                this.tileindex = this.tilesheetindex + this.workAnimationOffset + this.animationFrame;
            }

            updateSourceRectangle();

        }


    }
}
