using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Linq;

using StardewValley;
using StardewValley.Tools;

using StardewValley.Objects;
using StardewValley.Locations;
using StardewValley.Buildings;

using Microsoft.Xna.Framework.Graphics;

namespace TheJunimoExpress
{
    class JunimoHelper : StardewValley.Object
    {
        public int helperID = 0;
        private int frame = 0;

        private int offset = 32;
        private int mainoffset = 0;
        public Color color = Color.White;
        public double animspeed = 300.00;
        public bool onTracks = false;
        public int direction = 0;
        public Vector2 runFrame = Vector2.Zero;
        public int moved = 0;
        public bool readyStop = false;
        public bool stop = false;
        public Chest itemChest = new Chest();
        public bool nearPlayer = false;

        public GameLocation location;
        public BuildableGameLocation bgl;
        public int objectID = 0;
        public List<Item> inList = new List<Item>();
        public int index = -1;
        public int building = -1;
        public bool inventory = false;



        public static Color[] colorList = { Color.Aquamarine, Color.Chocolate, Color.DarkCyan, Color.DeepPink, Color.Olive, Color.Orange, Color.Peru, Color.MediumSpringGreen, Color.Magenta, Color.Gold, Color.Maroon, Color.Salmon, Color.MistyRose };
        public int emote = 0;

        public LinkedChest targetChest;


        public string saveObject()
        {
            checkIfBuilding();
            if(this.targetChest != null)
            {
                this.objectID = this.targetChest.objectID;

            }

            string locationName = "";
            if (this.building == -1)
            {
                locationName = this.location.name;
            }
            else
            {
                locationName = this.bgl.name;
            }
            return "JunimoHelper;;;" + locationName + ";;;" + this.building.ToString() + ";;;" + this.index.ToString() + ";;;" + this.tileLocation.X.ToString() + ";;;" + this.tileLocation.Y.ToString() + ";;;" + this.objectID.ToString() + ";;;" + this.direction.ToString();

        }

        public void removeObject()
        {
            Chest chest = getChest();
            chest.playerChoiceColor = this.color;
            if (this.index >= 0)
            {

                this.inList[this.index] = chest;
            }
            else
            {
                this.location.objects[new Vector2(this.tileLocation.X, this.tileLocation.Y)] = chest;
            }
        }

        public Chest getChest()
        {
            Chest newChest = new Chest(true);
            Item[] itemList = new Item[this.itemChest.items.Count()];
            this.itemChest.items.CopyTo(itemList);
            newChest.items = new List<Item>(itemList);
            return newChest;
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




        public JunimoHelper()
            : base()
        {


        }

        public void checkForDrop()
        {
           
            if (this.location.objects.ContainsKey(this.tileLocation + new Vector2(0, -1f)) && this.location.objects[this.tileLocation + new Vector2(0, -1f)] is Chest)
            {
                if(!this.itemChest.isEmpty())
                {
                    Chest nextChest = (Chest) this.location.objects[this.tileLocation + new Vector2(0, -1f)];
                    for (int i = 0; i < this.itemChest.items.Count(); i++)
                    {
                        if (nextChest.items.Count >= 36)
                        {
                            break;
                        }

                        nextChest.addItem(this.itemChest.items[i]);
                        this.itemChest.items.Remove(this.itemChest.items[i]);
                    }
                  
                }
                
            }
            if (this.location.objects.ContainsKey(this.tileLocation + new Vector2(0, 1f)) && this.location.objects[this.tileLocation + new Vector2(0, 1f)] is Chest)
            {
                if (!this.itemChest.isEmpty())
                {
                    Chest nextChest = (Chest)this.location.objects[this.tileLocation + new Vector2(0, 1f)];
                    for (int i = 0; i < this.itemChest.items.Count(); i++)
                    {
                        if (nextChest.items.Count >= 36)
                        {
                            break;
                        }

                        nextChest.addItem(this.itemChest.items[i]);
                        this.itemChest.items.Remove(this.itemChest.items[i]);
                    }

                }
            } 

        }


        public void checkForDropIn()
        {
            List<Vector2> vList = new List<Vector2>();
            vList.Add(this.tileLocation + new Vector2(0, 1f));
            vList.Add(this.tileLocation + new Vector2(-1f,0));
            vList.Add(this.tileLocation + new Vector2(1f, 0));
            vList.Add(this.tileLocation + new Vector2(0, -1f));
            vList.Add(this.tileLocation + new Vector2(1f, 1f));
            vList.Add(this.tileLocation + new Vector2(-1, -1f));
            vList.Add(this.tileLocation + new Vector2(-1, 1f));
            vList.Add(this.tileLocation + new Vector2(1, -1f));

            for (int d = 0; d < vList.Count(); d++)
            {
                if (this.location.objects.ContainsKey(vList[d]))
                {
                    if (!this.itemChest.isEmpty() && this.location.objects[vList[d]].heldObject == null)
                    {
                        StardewValley.Object dropObject = this.location.objects[vList[d]];

                        for (int i = 0; i < this.itemChest.items.Count(); i++)
                        {

                            bool tryDrop = dropObject.performObjectDropInAction((StardewValley.Object)this.itemChest.items[i], false, Game1.player);
                            if (tryDrop)
                            {
                                int stack = this.itemChest.items[i].getStack();
                                if (stack < 2)
                                {
                                    this.itemChest.items.Remove(this.itemChest.items[i]);
                                }
                                else
                                {
                                    this.itemChest.items[i].Stack--;
                                }
                                break;
                            }

                        }

                    }

                }
            }

        }

        public void checkForDropOut()
        {
            List<Vector2> vList = new List<Vector2>();
            vList.Add(this.tileLocation + new Vector2(0, 1f));
            vList.Add(this.tileLocation + new Vector2(-1f, 0));
            vList.Add(this.tileLocation + new Vector2(1f, 0));
            vList.Add(this.tileLocation + new Vector2(0, -1f));

            for (int d = 0; d < vList.Count(); d++)
            {
                if (this.location.objects.ContainsKey(vList[d]))
                {
                    if (this.itemChest.isEmpty() && !(this.location.objects[vList[d]].Equals("Crystalarium")) && !(this.location.objects[vList[d]].Equals("Slime Egg-Press")) && !(this.location.objects[vList[d]].Equals("Charcoal Kiln")) && !(this.location.objects[vList[d]].Equals("Furnace")) && this.location.objects[vList[d]].readyForHarvest && this.location.objects[vList[d]].heldObject != null)
                    {
                        StardewValley.Object dropObject = this.location.objects[vList[d]];
                        this.itemChest.addItem(dropObject.heldObject.getOne());
                        dropObject.heldObject = null;
                        dropObject.readyForHarvest = false;
                        

                    }

                }
            }

        }

        public void pickFromChest(Chest nextChest)
        {

            if (!nextChest.isEmpty())
            {
                List<Item> itemList = new List<Item>();
                int j = 0;
                if (nextChest is LinkedChest && (nextChest as LinkedChest).objectID == -1)
                {
                    j = 1;
                }
                if(nextChest is LinkedChest && (nextChest as LinkedChest).objectID == -1 && nextChest.items.Count() < 2)
                {
                    return;
                }
                for (int i = j; i < nextChest.items.Count(); i++)
                {
                    if (nextChest is LinkedChest && (nextChest as LinkedChest).objectID == -1 && !this.itemChest.isEmpty() && this.itemChest.items[0].canStackWith(nextChest.items[i]))
                    {
                        itemList.Add(nextChest.items[i]);
                        this.itemChest.addItem(nextChest.items[i]);
                    }
                    else if (!this.itemChest.isEmpty() && this.itemChest.items[0].canStackWith(nextChest.items[i]))
                    {
                        itemList.Add(nextChest.items[i]);
                        this.itemChest.addItem(nextChest.items[i]);
                    }
                    else if (this.itemChest.isEmpty() && (nextChest.items[i] is StardewValley.Object) && !(nextChest.items[i] is Tool) && !(nextChest.items[i] as StardewValley.Object).bigCraftable)
                    {
                        itemList.Add(nextChest.items[i]);
                        this.itemChest.addItem(nextChest.items[i]);
                    }

                }

                for (int i = 0; i < itemList.Count(); i++)
                {
                    nextChest.items.Remove(itemList[i]);
                }

            }

        }

        public void checkForTake()
        {
            
            if (this.location.objects.ContainsKey(this.tileLocation + new Vector2(-1f, 0)) && this.location.objects[this.tileLocation + new Vector2(-1f, 0)] is Chest)
            {
                Chest nextChest = (Chest)this.location.objects[this.tileLocation + new Vector2(-1f, 0)];
                this.pickFromChest(nextChest);
            }
            if (this.location.objects.ContainsKey(this.tileLocation + new Vector2(1f, 0)) && this.location.objects[this.tileLocation + new Vector2(1f, 0)] is Chest)
            {
                Chest nextChest = (Chest)this.location.objects[this.tileLocation + new Vector2(1f, 0)];
                this.pickFromChest(nextChest);
            }
               
            
        }





        public override void updateWhenCurrentLocation(GameTime time)
        {
            if (!Game1.isDarkOut() && this.stop && this.onTracks)
            {
                this.stop = false;
                setDirection();
            }

            if (time.TotalGameTime.TotalMilliseconds % this.animspeed <= 10.0)
            {
                
                this.frame++;
                if (this.frame > 7)
                {
                    this.frame = 0;
                }
            }
            if (this.emote > 0)
            {
              
                if (this.frame > 2)
                {
                    this.emote = 0;
                    Random rnd = new Random();
                    this.frame = rnd.Next(7);
                    this.animspeed = 200.00;
                }
            }

    
        }

        public void start()
        {

            
            this.onTracks = false;
            if (this.location.terrainFeatures.ContainsKey(tileLocation) && this.location.terrainFeatures[this.tileLocation] is RailroadTrack)
            {
                this.onTracks = true;
                this.direction = 0;
                this.mainoffset = 80;
                this.setDirection();
            }
            else
            {
                this.offset = 32;
                this.mainoffset = 0;
            }

  

            this.type = location.name;


            if (this.type == "Town" && this.tileLocation.Y > 50)
            {
                this.offset = 40;
            }

            if (this.type == "Desert" && this.tileLocation.Y < 25)
            {
                this.offset = 40;
            }

            if (this.type == "Sewer")
            {
                this.offset = 40;
            }


            if (this.type == "WitchSwamp")
            {
                this.offset = 40;
            }

            if (this.type == "BugLand")
            {
                this.offset = 40;
            }

            if (this.type == "Beach" && this.tileLocation.Y > 30)
            {
                this.offset = 40;
            }

            if (this.type == "Mountain" && this.tileLocation.X < 100)
            {
                this.offset = 40;
            }

            if (this.type == "Woods" && this.tileLocation.X < 25)
            {
                this.offset = 40;
            }


            this.name = "Little Helper";
            this.emote = 3;

        }


        public JunimoHelper(GameLocation l, Vector2 tl, int parentSheetIndex, bool isRecipe = false)
            : base(tl, parentSheetIndex, isRecipe)
        {

            this.tileLocation = tl;
            this.location = l;
            this.start();

        }

        public JunimoHelper(Vector2 tl, int parentSheetIndex, bool isRecipe = false)
            : base(tl, parentSheetIndex, isRecipe)
        {
           
            this.tileLocation = tl;
            this.location = Game1.currentLocation;
            this.start();
            Random rnd = new Random();
            
            this.helperID = rnd.Next(colorList.Length);
            this.color = colorList[helperID];

        }


        public override bool performToolAction(Tool t)
        {

            GameLocation location = Game1.currentLocation;
            if (t == null || !(t.GetType() == typeof(Pickaxe)))
            {
                this.emote = 2;
                this.frame = 0;
                return false;
            }
            Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, 4, false, -1, false, -1);

            Game1.playSound("hammer");

            location.objects.Remove(tileLocation);
            location.debris.Add(new Debris((Item)new StardewValley.Object(268, 50, false, -1, 0), tileLocation * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2))));
            LoadData.objectlist.Remove(this);
            if(this.targetChest != null) { 
            LoadData.objectlist.Remove(this.targetChest);
            this.targetChest.removeObject();
            }
            return false;
        }

        public void moveNext()
        {
            this.location.objects.Remove(this.tileLocation);
            if (direction == 0)
            {
                this.tileLocation += new Vector2(1f, 0);
            }
            if (direction == 1)
            {
                this.tileLocation += new Vector2(-1f, 0);
            }
            if (direction == 2)
            {
                this.tileLocation += new Vector2(0, 1f);
            }
            if (direction == 3)
            {
                this.tileLocation += new Vector2(0, -1f);
            }
            
            this.tryPlacingJunimo();

        }

        public void tryPlacingJunimo()
        {
            if (!this.location.objects.ContainsKey(this.tileLocation))
            {
                
                this.location.objects.Add(this.tileLocation, this);
               
                checkForDropOut();
                this.checkForDropIn();
                this.checkForDrop();
                this.checkForTake();
            }
            else
            {
              
                DelayedAction delayedAction1 = new DelayedAction(250);
                delayedAction1.behavior = new DelayedAction.delayedBehavior(this.tryPlacingJunimo);
                Game1.delayedActions.Add(delayedAction1);
            }

        }

        public List<int> availableDirections()
        {
            List<int> directions = new List<int>();

            if ((!this.location.objects.ContainsKey(this.tileLocation + new Vector2(1f, 0)) || (this.location.objects.ContainsKey(this.tileLocation + new Vector2(1f, 0)) && this.location.objects[this.tileLocation + new Vector2(1f, 0)] is JunimoHelper)) && this.location.terrainFeatures.ContainsKey(this.tileLocation + new Vector2(1f, 0)) && this.location.terrainFeatures[this.tileLocation + new Vector2(1f, 0)] is RailroadTrack){
                directions.Add(0);
            }
            if ((!this.location.objects.ContainsKey(this.tileLocation + new Vector2(-1f, 0)) || (this.location.objects.ContainsKey(this.tileLocation + new Vector2(-1f, 0)) && this.location.objects[this.tileLocation + new Vector2(-1f, 0)] is JunimoHelper)) && this.location.terrainFeatures.ContainsKey(this.tileLocation + new Vector2(-1f, 0)) && this.location.terrainFeatures[this.tileLocation + new Vector2(-1f, 0)] is RailroadTrack)
            {
                directions.Add(1);
            }
            if ((!this.location.objects.ContainsKey(this.tileLocation + new Vector2(0, 1f)) || (this.location.objects.ContainsKey(this.tileLocation + new Vector2(0, 1f)) && this.location.objects[this.tileLocation + new Vector2(0, 1f)] is JunimoHelper)) && this.location.terrainFeatures.ContainsKey(this.tileLocation + new Vector2(0, 1f)) && this.location.terrainFeatures[this.tileLocation + new Vector2(0, 1f)] is RailroadTrack)
            {
                directions.Add(2);
            }
            if ((!this.location.objects.ContainsKey(this.tileLocation + new Vector2(0, -1f)) || (this.location.objects.ContainsKey(this.tileLocation + new Vector2(0, -1f)) && this.location.objects[this.tileLocation + new Vector2(0, -1f)] is JunimoHelper)) && this.location.terrainFeatures.ContainsKey(this.tileLocation + new Vector2(0, -1f)) && this.location.terrainFeatures[this.tileLocation + new Vector2(0, -1f)] is RailroadTrack)
            {
                directions.Add(3);
            }

            return directions;


        }

        public void setDirection()
        {
            List<int> directions = availableDirections();
            
            if (this.direction == 0)
            {

                if (directions.Contains(0))
                {
                    this.direction = 0;
                }
                else if (directions.Contains(2))
                {
                    this.direction = 2;
                }
                else if (directions.Contains(3))
                {
                    this.direction = 3;
                }
                else if (directions.Contains(1))
                {
                    this.direction = 1;
                }
                else
                {
                    this.stop = true;
                }
               
            }

            if (this.direction == 1)
            {

                if (directions.Contains(1))
                {
                    this.direction = 1;
                }
                else if (directions.Contains(3))
                {
                    this.direction = 3;
                }
                else if (directions.Contains(2))
                {
                    this.direction = 2;
                }
                else if (directions.Contains(0))
                {
                    this.direction = 0;
                }
                else
                {
                    this.stop = true;
                }

            }

            if (this.direction == 2)
            {

                if (directions.Contains(2))
                {
                    this.direction = 2;
                }
                else if (directions.Contains(1))
                {
                    this.direction = 1;
                }
                else if (directions.Contains(0))
                {
                    this.direction = 0;
                }
                else if (directions.Contains(3))
                {
                    this.direction = 3;
                }
                else
                {
                    this.stop = true;
                }

            }

            if (this.direction == 3)
            {

                if (directions.Contains(3))
                {
                    this.direction = 3;
                }
                else if (directions.Contains(0))
                {
                    this.direction = 0;
                }
                else if (directions.Contains(1))
                {
                    this.direction = 1;
                }
                else if (directions.Contains(2))
                {
                    this.direction = 2;
                }
                else
                {
                    this.stop = true;
                }

            }

            if (this.direction == 0)
            {
                this.mainoffset = 80;
            }

            if (this.direction == 1)
            {
                this.mainoffset = 88;
            }

            if (this.direction == 2)
            {
                this.mainoffset = 0;
            }

            if (this.direction == 2 && !this.itemChest.isEmpty())
            {
                this.mainoffset = 72;
            }

            if (this.direction == 3)
            {
                this.mainoffset = 104;
            }

            if (this.direction == 3 && !this.itemChest.isEmpty())
            {
                this.mainoffset = 96;
            }
        }

        public void run()
        {
            if (this.emote == 0 && this.onTracks == true)
            {
                if (Math.Abs(this.runFrame.X) + Math.Abs(this.runFrame.Y) >= 64)
                {
                    this.runFrame = Vector2.Zero;
                    this.moveNext();
                    this.moved++;
                   

                    this.setDirection();

                }


                if (!this.stop )
                {

                    if (this.direction == 0)
                    {
                        this.runFrame += new Vector2(2f, 0);

                    }
                    else if (this.direction == 1)
                    {
                        this.runFrame += new Vector2(-2f, 0);
                    }
                    else if (this.direction == 2)
                    {
                        this.runFrame += new Vector2(0, 2f);
                    }
                    else if (this.direction == 3)
                    {
                        this.runFrame += new Vector2(0, -2f);
                    }
                    this.readyStop = false;
                }
            }
        }

        public void checkForChest()
        {

            if (this.targetChest == null && !this.onTracks)
            {
                Dictionary<Vector2, StardewValley.Object> allObjects = this.location.objects;
                Vector2 check = new Vector2(0, 0);
                Vector2 pPosition = new Vector2(this.tileLocation.X, this.tileLocation.Y);
                LinkedChest newChest = new LinkedChest(true);

                for (float i = -1; i <= 1.0; i++)
                {
                    if (this.targetChest != null)
                    {
                        break;
                    }
                    for (float j = -1; j <= 1.0; j++)
                    {
                        check = (new Vector2(i, j) + pPosition);
                        if (allObjects.ContainsKey(check) && allObjects[check] is Chest && (allObjects[check] as Chest).isEmpty() && !(allObjects[check] is LinkedChest))
                        {
                            newChest.items = (allObjects[check] as Chest).items;
                            allObjects.Remove(check);
                            allObjects.Add(check, newChest);
                            newChest.linkedJunimo = this;
                            newChest.tileLocation = check; 
                            newChest.playerChoiceColor = this.color;
                            newChest.objectID = LoadData.objectlist.Count();
                            this.targetChest = newChest;
                            LoadData.objectlist.Add(newChest);
                            break;
                        }

                    }
                }

            }
            else if(this.targetChest != null)
            {
                this.targetChest.playerChoiceColor = this.color;

            }
        }


        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {

            this.tileLocation.X = x;
            this.tileLocation.Y = y;
           

            this.nearPlayer = false;

            if (this.location == Game1.currentLocation && Math.Abs(x - Game1.player.getTileLocation().X) <= 2 && Math.Abs(y - Game1.player.getTileLocation().Y) <= 2)
            {
                this.nearPlayer = true;
            }



            int shift = 0;
            int mainoffsetTiles = this.mainoffset;
            if (Game1.isDarkOut() || this.stop == true) { shift = 8; mainoffsetTiles = 0; this.stop = true;}
            
            
            int offsetTiles = this.offset;
               
            if (this.emote > 0)
            {
                offsetTiles = 56 + 4 * (this.emote - 1);
            }



            Vector2 vector2 = this.getScale() * (float)Game1.pixelZoom;
            Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize), (float)(y * Game1.tileSize - Game1.tileSize)));
            if (this.onTracks)
            {
                local = local +this.runFrame;
                if (this.direction < 2)
                {
                    local = new Vector2(local.X, local.Y-16f);

                }
            }
            Microsoft.Xna.Framework.Rectangle destinationRectangle = new Microsoft.Xna.Framework.Rectangle((int)((double)local.X - (double)vector2.X / 2.0) + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((double)local.Y - (double)vector2.Y / 2.0) + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((double)Game1.tileSize + (double)vector2.X), (int)((double)(Game1.tileSize * 2) + (double)vector2.Y / 2.0));

           
            if (this.emote != 3) { 
            spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destinationRectangle, new Microsoft.Xna.Framework.Rectangle?(StardewValley.Object.getSourceRectForBigCraftable(this.showNextIndex ? this.ParentSheetIndex + 1 : this.ParentSheetIndex + this.frame + mainoffsetTiles + shift)), this.color * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, (float)((double)Math.Max(0.0f, (float)((y + 1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + (this.parentSheetIndex == 105 ? 0.00350000010803342 : 0.0) + (double)x * 9.99999974737875E-06));
            }
            if ((shift == 0 || this.emote > 0) && nearPlayer && (!this.onTracks || this.emote == 3))
            {
                if (this.emote == 0) { alpha = 0.5f; }
                if (this.emote == 3)
                {
                    spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destinationRectangle, new Microsoft.Xna.Framework.Rectangle?(StardewValley.Object.getSourceRectForBigCraftable(this.showNextIndex ? this.ParentSheetIndex + 1 : this.ParentSheetIndex +  offsetTiles + this.frame)), this.color * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, (float)((double)Math.Max(0.0f, (float)((y + 1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + (this.parentSheetIndex == 105 ? 0.00350000010803342 : 0.0) + (double)x * 9.99999974737875E-06));
                }
                else
                {
                    spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destinationRectangle, new Microsoft.Xna.Framework.Rectangle?(StardewValley.Object.getSourceRectForBigCraftable(this.showNextIndex ? this.ParentSheetIndex + 1 : this.ParentSheetIndex + offsetTiles + this.frame)), Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, (float)((double)Math.Max(0.0f, (float)((y + 1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + (this.parentSheetIndex == 105 ? 0.00350000010803342 : 0.0) + (double)x * 9.99999974737875E-06));
                }
            }

            if (!this.itemChest.isEmpty())
            {
                StardewValley.Item item = this.itemChest.items[0];
                Microsoft.Xna.Framework.Rectangle itemDestinationRectangle = new Microsoft.Xna.Framework.Rectangle((int)((double)local.X - (double)vector2.X / 2.0) + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((double)local.Y - (double)vector2.Y / 2.0) + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((double)Game1.tileSize + (double)vector2.X), (int)((double)(Game1.tileSize) + (double)vector2.Y / 2.0));
                spriteBatch.Draw(Game1.objectSpriteSheet, itemDestinationRectangle, new Microsoft.Xna.Framework.Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, item.parentSheetIndex, 16, 16)), Color.White * 1f, 0.0f, Vector2.Zero, SpriteEffects.None, (float)((double)Math.Max(0.0f, (float)((y + 1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + (this.parentSheetIndex == 105 ? 0.00350000010803342 : 0.0) + (double)x * 9.99999974737875E-06));
            }
            this.checkIfBuilding();
        }


        public void helperAction()
        {
            checkIfBuilding();
            if (this.targetChest != null)
            {
                if (this.offset == 40)
                {
                    StardewValley.Object fish = this.location.getFish(500, 774, 5, Game1.player, (double)15);
                    if (fish == null || fish.ParentSheetIndex <= 0) { 
                        fish = new StardewValley.Object(Game1.random.Next(167, 173), 1, false, -1, 0);
                    }
                    if (this.nearPlayer == true)
                        {
                            Game1.playSound("coin");
                        }
                    if (this.targetChest.items.Count < 36)
                    {
                        this.targetChest.addItem(fish);
                    }
                }

                if ((this.location is AnimalHouse) && this.location.name.Contains("Barn") && this.offset == 32)
                {
                    
                    SerializableDictionary<long,FarmAnimal> animalDict = (this.location as AnimalHouse).animals;
                    List<FarmAnimal> animals = new List<FarmAnimal>();
                    foreach (long keyL in animalDict.Keys)
                    {

                        animals.Add(animalDict[keyL]);

                    }

                    if (animals.Count() > 0)
                    {
                        for (int i = 0; i < animals.Count(); i++)
                        {

                            FarmAnimal animal = animals[i];

                            if (animal != null && animal.currentProduce > 0 && (animal.age >= (int)animal.ageWhenMature) && this.targetChest.items.Count < 36)
                            {

                                StardewValley.Object product = new StardewValley.Object(Vector2.Zero, animal.currentProduce, (string)null, false, true, false, false);
                                product.quality = animal.produceQuality;

                                this.targetChest.addItem(product);

                                if (this.nearPlayer == true)
                                {
                                    Game1.playSound("coin");
                                }
                                animal.currentProduce = -1;
                                if (animal.showDifferentTextureWhenReadyForHarvest)
                                    animal.sprite.Texture = Game1.content.Load<Texture2D>("Animals\\Sheared" + animal.type);
                                Game1.player.gainExperience(0, 5);
                                break;

                            }
                        }
                    }

                }


                if (this.offset == 32)
                {


                    List<Vector2> pickTiles = new List<Vector2>();
                    SerializableDictionary<Vector2, StardewValley.Object> allobjects;

                    allobjects = this.location.objects;

                    pickTiles = new List<Vector2>();

                    Random rnd = new Random();


                    foreach (var keyV in allobjects.Keys)
                    {


                        if (allobjects[keyV].canBeShipped() && !allobjects[keyV].hasBeenInInventory && !allobjects[keyV].bigCraftable && allobjects[keyV].CanBeGrabbed && !allobjects[keyV].canBePlacedInWater() && allobjects[keyV].canBeGivenAsGift() && !allobjects[keyV].hasBeenPickedUpByFarmer && allobjects[keyV].salePrice() >= 1 && (allobjects[keyV].isForage(this.location) || allobjects[keyV].isAnimalProduct()))
                        {
                            pickTiles.Add(keyV);

                        }

                    }

                    int i = rnd.Next(pickTiles.Count());


                    if (this.targetChest.items.Count < 36 && pickTiles.Count() > 0)
                    {
                        if (this.nearPlayer == true)
                        {
                            Game1.playSound("coin");
                        }

                        this.targetChest.addItem(allobjects[pickTiles[i]]);
                        allobjects.Remove(pickTiles[i]);
                    }


                }
            }
        }

    }
}
           

