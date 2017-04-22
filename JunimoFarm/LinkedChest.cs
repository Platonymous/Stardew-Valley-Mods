using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Tools;
using StardewValley.Objects;
using StardewValley.Locations;
using StardewValley.Buildings;

namespace JunimoFarm
{
    class LinkedChest : Chest
    {

        public JunimoHelper linkedJunimo;
        public GameLocation location;
        public BuildableGameLocation bgl;
        public int objectID = 0;
        public List<Item> inList = new List<Item>();
        public int index = -1;
        public int building = -1;
        public bool inventory = false;
        public int frame = 0;

        public string saveObject()
        {
            string locationName = "";
            if (this.inventory == true)
            {
                locationName = "Inventory";
            }else if (this.building == -1) { 
            locationName = this.location.name;
            }else
            {
                locationName = this.bgl.name;
            }
            return "LinkedChest;;;" + locationName + ";;;" + this.building.ToString() + ";;;" + this.index.ToString() + ";;;" + this.tileLocation.X.ToString() + ";;;" + this.tileLocation.Y.ToString() + ";;;" + this.objectID.ToString();

        }

        public void removeObject()
        {
            if (this.index >= 0) { 
            this.inList[this.index] = getChest();
            (this.inList[this.index] as Chest).playerChoiceColor = this.playerChoiceColor;
            }
            else
            {
                this.location.objects[new Vector2(this.tileLocation.X, this.tileLocation.Y)] = getChest();
                (this.location.objects[new Vector2(this.tileLocation.X, this.tileLocation.Y)] as Chest).playerChoiceColor = this.playerChoiceColor;
            }
        }

        public Chest getChest()
        {
            Chest newChest = new Chest(true);
            Item[] itemList = new Item[this.items.Count()];
            this.items.CopyTo(itemList);
            newChest.items = new List<Item>(itemList);
            return newChest;
        }

        public LinkedChest()
            :base()
        {
            this.location = Game1.currentLocation;
           
        }

        

        public LinkedChest(Vector2 v)
            :base(v)
        {
            
            this.location = Game1.currentLocation;
            this.tileLocation = v;
            
        }

        public LinkedChest(string type, Vector2 p)
            :base(type,p)
        {
            
            this.location = Game1.currentLocation;
            this.tileLocation = p;
            
        }


        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            
            if (Game1.player.items.Contains(this))
            {
                this.inList = Game1.player.items;
                this.index = Game1.player.items.FindIndex(x => x == this);
                this.inventory = true;
            }
            else
            {
                inventory = false;
                Dictionary<Vector2, StardewValley.Object> allObjects = Game1.currentLocation.objects;
                Vector2 check = new Vector2(0, 0);
                Vector2 pPosition = new Vector2(Game1.player.getTileX(), Game1.player.getTileX());
                LinkedChest newChest = new LinkedChest(true);

                for (float i = -1; i <= 1.0; i++)
                {
                    
                    for (float j = -1; j <= 1.0; j++)
                    {
                        check = (new Vector2(i, j) + pPosition);
                        if (allObjects.ContainsKey(check) && allObjects[check] is Chest && (allObjects[check] as Chest).items.Contains(this))
                        {
                            this.inList = (allObjects[check] as Chest).items;
                            this.index = this.inList.FindIndex(x => x == this);
                            if (allObjects[check] is LinkedChest)
                            {
                                this.linkedJunimo.targetChest = (LinkedChest) null;
                                this.removeObject();
                            }else
                            {
                                this.tileLocation = allObjects[check].tileLocation;
                            }
                            break;
                        }

                    }
                }

             
            }

            base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber);
            if(this.objectID > 0) {
                this.quality = 2;
                float num = this.quality < 4 ? 0.0f : (float)((Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1.0) * 0.0500000007450581);
                spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(12f, (float)(Game1.tileSize - 12) + num), new Microsoft.Xna.Framework.Rectangle?(this.quality < 4 ? new Microsoft.Xna.Framework.Rectangle(338 + (this.quality - 1) * 8, 400, 8, 8) : new Microsoft.Xna.Framework.Rectangle(346, 392, 8, 8)), this.playerChoiceColor * transparency, 0.0f, new Vector2(4f, 4f), (float)(3.0 * (double)scaleSize * (1.0 + (double)num)), SpriteEffects.None, layerDepth);
            }
        }

        public void checkIfBuilding()
        {
            this.building = -1;
            this.bgl = null;
            if (this.index == -1)
            {
               
                if (this.location.GetType().GetMethod("getBuilding") != null)
                {
                    Building building = (this.location as AnimalHouse).getBuilding();

                    this.building = Game1.getFarm().buildings.IndexOf(building);
                    this.bgl = Game1.getFarm();
                    
                }
            }

        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            this.index = -1;
            this.inventory = false;
            inList = new List<Item>();
            this.checkIfBuilding();
            base.draw(spriteBatch, x, y, alpha);
            if (this.objectID == -1 && this.linkedJunimo == null) { 
            Vector2 vector2 = this.getScale() * (float)Game1.pixelZoom;
            Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize), (float)(y * Game1.tileSize - Game1.tileSize)));
            Microsoft.Xna.Framework.Rectangle destinationRectangle = new Microsoft.Xna.Framework.Rectangle((int)((double)local.X - (double)vector2.X / 2.0) + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((double)local.Y - (double)vector2.Y / 2.0) + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((double)Game1.tileSize + (double)vector2.X), (int)((double)(Game1.tileSize * 2) + (double)vector2.Y / 2.0));
            spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destinationRectangle, new Microsoft.Xna.Framework.Rectangle?(StardewValley.Object.getSourceRectForBigCraftable(this.showNextIndex ? this.ParentSheetIndex + 1 : LoadData.craftables[3] + 68 + this.frame)), Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, (float)((double)Math.Max(0.0f, (float)((y + 1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + (this.parentSheetIndex == 105 ? 0.00350000010803342 : 0.0) + (double)x * 9.99999974737875E-06));
            }
            if (!this.isEmpty())
            {
                Vector2 vector2 = this.getScale() * (float)Game1.pixelZoom;
                Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize), (float)(y * Game1.tileSize - Game1.tileSize)));
                StardewValley.Item item = this.items[0];
                Microsoft.Xna.Framework.Rectangle itemDestinationRectangle = new Microsoft.Xna.Framework.Rectangle((int)((double)local.X - (double)vector2.X / 2.0) + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((double)local.Y - (double)vector2.Y / 2.0) + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((double)Game1.tileSize + (double)vector2.X), (int)((double)(Game1.tileSize) + (double)vector2.Y / 2.0));
                spriteBatch.Draw(Game1.objectSpriteSheet, itemDestinationRectangle, new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, item.parentSheetIndex, 16, 16)), Color.White * 0.8f, 0.0f, Vector2.Zero, SpriteEffects.None, (float)((double)Math.Max(0.0f, (float)((y + 1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + (this.parentSheetIndex == 105 ? 0.00350000010803342 : 0.0) + (double)x * 9.99999974737875E-06));
            }
        }

        public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {
            this.index = -1;
            this.inventory = false;
            inList = new List<Item>();
          
            base.draw(spriteBatch, xNonTile, yNonTile, layerDepth, alpha);

        }

        public LinkedChest(GameLocation l, bool i, bool playerchest)
            : base(playerchest)
        {
            this.inventory = i;
            this.location = l;
        }

        public LinkedChest(bool playerchest)
            :base(playerchest)
        {
            this.inventory = false;
            this.location = Game1.currentLocation;
            
        }

        public override bool placementAction(GameLocation l, int x, int y, StardewValley.Farmer who = null)
        {
            Vector2 index1 = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
            this.tileLocation = index1;
            this.location = l;
            this.checkIfBuilding();
            l.objects.Add(index1, this);
            Game1.playSound("axe");
            return true;

        }

        public override void updateWhenCurrentLocation(GameTime time)
        {
            if (this.linkedJunimo != null)
            {

                if (this.playerChoiceColor == Color.Black)
                {
                    this.linkedJunimo.color = Color.White;
                }
                else
                {
                    this.linkedJunimo.color = this.playerChoiceColor;
                }
            }
            else if (this.objectID == -1)
            {
                if (time.TotalGameTime.TotalMilliseconds % 200.00 <= 10.0)
                {

                    this.frame++;
                    if (this.frame > 2)
                    {
                        this.frame = 0;
                    }
                }
            
                List<Vector2> vList = new List<Vector2>();
                vList.Add(this.tileLocation + new Vector2(0, 1f));
                vList.Add(this.tileLocation + new Vector2(-1f, 0));
                vList.Add(this.tileLocation + new Vector2(1f, 0));
                vList.Add(this.tileLocation + new Vector2(0, -1f));
                vList.Add(this.tileLocation + new Vector2(1f, 1f));
                vList.Add(this.tileLocation + new Vector2(-1, -1f));
                vList.Add(this.tileLocation + new Vector2(-1, 1f));
                vList.Add(this.tileLocation + new Vector2(1, -1f));

                if (!this.isEmpty())
                {

                    for (int d = 0; d < vList.Count(); d++)
                    {
                        if (this.location.objects.ContainsKey(vList[d]) && this.location.objects[vList[d]] is Chest)
                        {
                            if(this.location.objects[vList[d]] is LinkedChest && (this.location.objects[vList[d]] as LinkedChest).objectID == -1)
                            {
                                continue;
                            }

                            for (int i = 0; i < (this.location.objects[vList[d]] as Chest).items.Count(); i++)
                            {
                                bool check = false;
                                if (this.items.Count() > 0 && (this.location.objects[vList[d]] as Chest).items[i] != null && this.items[0].canStackWith((this.location.objects[vList[d]] as Chest).items[i]) && (this.items.Count() < 2 || this.items[1].canStackWith((this.location.objects[vList[d]] as Chest).items[i])))
                                {
                                    check = true;
                                }
                                if (check == true)
                                {
                                    if (this.items.Count() >= 2)
                                    {
                                        this.items[1].addToStack((this.location.objects[vList[d]] as Chest).items[i].Stack);
                                    }
                                    else
                                    {
                                        this.items.Insert(1,(this.location.objects[vList[d]] as Chest).items[i]);
                                    }
                                    (this.location.objects[vList[d]] as Chest).items.Remove((this.location.objects[vList[d]] as Chest).items[i]);
                                }

                            }

                        }
                    }
                }
            }

            

            base.updateWhenCurrentLocation(time);
        }

        public override bool performToolAction(Tool t)
        {
            
            if(!(t.GetType() == typeof(Pickaxe)) && !(t.GetType() == typeof(Axe)))
            {
                return false;
            }

            if (this.objectID == -1)
            {
                if (!this.isEmpty()) { return false; }
                LoadData.objectlist.Remove(this);
                Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, 4, false, -1, false, -1);

                Game1.playSound("hammer");

                location.objects.Remove(tileLocation);
                location.debris.Add(new Debris((Item)new StardewValley.Object(388, 50, false, -1, 0), tileLocation * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2))));
                return false;
            }

            this.location = Game1.currentLocation;

           
            Game1.createRadialDebris(location, 0, (int)this.tileLocation.X, (int)this.tileLocation.Y, 4, false, -1, false, -1);

            
            location.objects.Remove(this.tileLocation);

            Game1.playSound("hammer");

            location.debris.Add(new Debris((Item)this, tileLocation * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2))));

            return false;
        }
    }
}
