using SObject = StardewValley.Object;
using PyTK.CustomElementHandler;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using SFarmer = StardewValley.Farmer;
using PyTK.Extensions;
using PyTK.Types;
using StardewValley.Tools;

namespace CustomFarmingRedux
{
    public class CustomMachine : SObject, ISaveElement
    {
        internal IModHelper Helper = CustomFarmingReduxMod._helper;
        internal IMonitor Monitor = CustomFarmingReduxMod._monitor;
        internal Config config = CustomFarmingReduxMod._config;
        internal string folder => blueprint.pack.baseFolder;
        internal List<CustomMachineBlueprint> machines = CustomFarmingReduxMod.machines;
        internal static List<CustomMachine> activeMachines = new List<CustomMachine>();

        private CustomMachineBlueprint blueprint;

        private Texture2D texture;
        private Rectangle sourceRectangle {
            get
            {
               return Game1.getSourceRectForStandardTileSheet(texture, blueprint.tileindex + frame, tilesize.Width, tilesize.Height);
            }
        }
        private bool active = true;
        private bool wasBuild = false;
        private bool isWorking { get => active && !readyForHarvest && ((completionTime != null && activeRecipe != null) || blueprint.production == null); }
        private string id;
        private STime completionTime = null;
        private int tileindex;
        private Rectangle tilesize = new Rectangle(0, 0, 16, 32);
        private int _frame = 0;
        private int skipFrame = 10;
        private int animationFrames = 0;
        private int counter = 0;
        private RecipeBlueprint activeRecipe;
        private RecipeBlueprint starterRecipe;
        private GameLocation location;
        private int frame {
            get
            {
                if (!isWorking)
                    if (readyForHarvest)
                        _frame = blueprint.readyindex - tileindex;
                    else
                        _frame = 0;

                return _frame;
            }
            set
            {
                _frame = value;
            }
        }

        public CustomMachine()
        {

        }

        public CustomMachine(CustomMachineBlueprint blueprint)
        {
            build(blueprint);
            wasBuild = true;
        }

        private void build(CustomMachineBlueprint blueprint)
        {
            name = blueprint.name;
            if (blueprint.readyindex < 0)
                blueprint.readyindex = blueprint.tileindex;
            this.blueprint = blueprint;
            texture = Helper.Content.Load<Texture2D>($"{blueprint.pack.baseFolder}/{blueprint.folder}/{blueprint.texture}");
            id = blueprint.fullid;
            parentSheetIndex = -1;
            bigCraftable = true;
            type = "Crafting";
            tilesize = new Rectangle(0, 0, blueprint.tilewidth, blueprint.tileheight);
            boundingBox = new Rectangle(0, 0, blueprint.tilewidth * 4, blueprint.tileheight * 4);
            skipFrame = 60 / Math.Max(1, blueprint.fps);
            animationFrames = blueprint.frames;
            tileindex = blueprint.tileindex;
            if (blueprint.starter != null)
            {
                starterRecipe = new RecipeBlueprint();
                starterRecipe.index = blueprint.starter.index;
                starterRecipe.mBlueprint = blueprint;
                starterRecipe.stack = blueprint.starter.stack;
                starterRecipe.materials = new List<IngredientBlueprint>();
                starterRecipe.materials.Add(blueprint.starter);
            }                
        }

        public override string getCategoryName()
        {
            return blueprint.category;
        }

        public override Color getCategoryColor()
        {
            return Color.Magenta;
        }

        private SObject maxed(SObject obj)
        {
            SObject o = (SObject) obj.getOne();
            o.stack = int.MaxValue;
            return o;
        }

        private RecipeBlueprint findRecipe(List<List<Item>> items)
        {
            RecipeBlueprint result = null;
            if (blueprint.production != null)
                foreach (RecipeBlueprint r in blueprint.production)
                    if (r.hasIngredients(items))
                        result = r;

            return result;
        }

        private RecipeBlueprint findRecipe(List<Item> items)
        {
            return findRecipe(new List<List<Item>>() { items });
        }

        private RecipeBlueprint findRecipe(Item item)
        {
            return findRecipe(new List<List<Item>>() { new List<Item>() { item } });
        }

        private bool hasStarterMaterials(List<List<Item>> items)
        {
            if (starterRecipe == null)
                return true;

            if (starterRecipe.hasIngredients(items))
                return true;

            return false;
        }

        private bool hasStarterMaterials(List<Item> items)
        {
            return hasStarterMaterials(new List<List<Item>>() { items });
        }

        private List<List<Item>> getItemLists(SFarmer player = null)
        {
            List<List<Item>> items = new List<List<Item>>();

            if (player != null)
                items.Add(player.items);

            if (config.automation)
            {
                List<Vector2> tiles = Utility.getAdjacentTileLocations(tileLocation);
                if (location is GameLocation)
                    foreach (Vector2 tile in tiles)
                        if(location.objects.ContainsKey(tile) && location.objects[tile] is Chest c)
                            items.AddOrReplace(c.items);
            }
            
            return items;
        }

        private bool deliverToNearChest(SObject o)
        {
            List<Vector2> tiles = Utility.getAdjacentTileLocations(tileLocation);
            if (location is GameLocation)
                foreach (Vector2 tile in tiles)
                    if (location.objects.ContainsKey(tile) && location.objects[tile] is Chest c && c.playerChest == true && c.items.Count < 24)
                    {
                        c.addItem(o);
                        clear();
                        return true;
                    }
            return false;
        }

        private void getReadyForHarvest()
        {
            if (blueprint.production == null || activeRecipe == null)
                return;

            minutesUntilReady = -1;
            completionTime = null;
            heldObject = createProduce();
            readyForHarvest = true;

            if(config.automation)
                if (deliverToNearChest(heldObject))
                    startAutomation();
        }

        private void startAutomation()
        {
            List<List<Item>> items = getItemLists(null);
            RecipeBlueprint recipe = findRecipe(items);
            bool hasRecipe = recipe != null;
            bool hasStarter = hasStarterMaterials(items);
            if (hasRecipe && hasStarter && recipe.materials != null)
            {
                foreach (List<Item> list in items)
                    foreach (Item item in list)
                        if (recipe.materials.Find(m => m.index == item.parentSheetIndex || m.index == item.category) != null) {
                            startProduction((SObject)item, recipe, items);
                            return;
                        }
            }
        }

        private void startProduction(SObject obj, RecipeBlueprint recipe, List<List<Item>> items)
        {
            activeRecipe = recipe;
            if (completionTime == null)
            {
                completionTime = STime.CURRENT + recipe.time;
                minutesUntilReady = (completionTime - STime.CURRENT).timestamp;
            }

            if (starterRecipe != null)
                starterRecipe.consumeIngredients(items);

            if (recipe.materials != null)
                recipe.consumeIngredients(items, obj);

            if (obj != null)
            {
                heldObject = (SObject)obj.getOne();
                heldObject.stack = obj.stack;
            }
        }
        private SObject createProduce()
        {
            return activeRecipe.createObject(heldObject);
        }

        public override void updateWhenCurrentLocation(GameTime time)
        {
            if (!wasBuild)
                return;

            if (isWorking && completionTime != null && STime.CURRENT >= completionTime && activeRecipe != null)
                getReadyForHarvest();
            
            if (!(isWorking && completionTime != null))
                startAutoProduction();

            base.updateWhenCurrentLocation(time);
        }

        public override string DisplayName { get => name; set => base.DisplayName = value; }

        public override string getDescription()
        {
            return Game1.parseText(blueprint.description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4);
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            if (blueprint.category == "Mailbox" && Game1.mailbox.Count == 0)
                active = false;
            else
                active = true;

            if (animationFrames != 0)
            {
                counter++;
                counter = counter > skipFrame ? 0 : counter;
                frame = counter == 0 ? frame + 1 : frame;
                frame = frame >= animationFrames + 1 ? 1 : frame;
            }
            else if (animationFrames == 1)
                frame = 1;
            else
                frame = 0;

            if (isWorking && completionTime != null)
                minutesUntilReady = 100;

            Vector2 vector2 = (blueprint.pulsate) ? getScale() * Game1.pixelZoom  : new Vector2(0, 4) * Game1.pixelZoom;
            Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((x * Game1.tileSize), (y * Game1.tileSize - Game1.tileSize)));
            Rectangle destinationRectangle = new Rectangle((int)(local.X - vector2.X / 2.0) + (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)(local.Y - vector2.Y / 2.0) + (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((tilesize.Width * 4) + (double)vector2.X), (int)((tilesize.Height * 4) + vector2.Y / 2.0));
            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, (float)(Math.Max(0.0f, ((y + 1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + x * 9.99999974737875E-06));

            if (readyForHarvest && heldObject != null)
            {
                Texture2D tilesheet = (heldObject is CustomObject co) ? co.texture : Game1.objectSpriteSheet;
                Rectangle csourceRectangle = (heldObject is CustomObject cobj) ? cobj.sourceRectangle : Game1.getSourceRectForStandardTileSheet(tilesheet, heldObject.parentSheetIndex, 16, 16);
                Color color = (heldObject is CustomObject cco) ? cco.color : (heldObject is ColoredObject cvo) ? cvo.color : Color.White;
                float num = (float)(4.0 * Math.Round(Math.Sin(DateTime.Now.TimeOfDay.TotalMilliseconds / 250.0), 2));
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((x * Game1.tileSize - 8), (y * Game1.tileSize - Game1.tileSize * 3 / 2 - 16) + num)), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(((y + 1) * Game1.tileSize) / 10000.0 + 9.99999997475243E-07 + tileLocation.X / 10000.0 + (parentSheetIndex == 105 ? 0.00150000001303852 : 0.0)));
                spriteBatch.Draw(tilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((x * Game1.tileSize + Game1.tileSize / 2), (y * Game1.tileSize - Game1.tileSize - Game1.tileSize / 8) + num)), csourceRectangle, color * 0.75f, 0.0f, new Vector2(8f, 8f), Game1.pixelZoom, SpriteEffects.None, (float)(((y + 1) * Game1.tileSize) / 10000.0 + 9.99999974737875E-06 + tileLocation.X / 10000.0 + 0.0));
            }

            if (blueprint.showitem && heldObject != null)
            {
                Rectangle displayDestinationRectangle = new Rectangle((int)(local.X - vector2.X / 2.0) + blueprint.itempos[0] + (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)(local.Y - vector2.Y / 2.0) + blueprint.itempos[1] + (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)(blueprint.itemzoom * ((int)(Game1.tileSize + vector2.X))), (int)(blueprint.itemzoom * ((int)((Game1.tileSize) + vector2.Y / 2.0))));
                spriteBatch.Draw(heldObject is CustomObject co ?  co.texture : Game1.objectSpriteSheet, displayDestinationRectangle, heldObject is CustomObject cor ? cor.sourceRectangle : Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, heldObject.parentSheetIndex, 16, 16), Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, (float)(Math.Max(0.0f, ((y + 1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + (x + 1) * 9.99999974737875E-06));
            }

            if (blueprint.category == "Mailbox" && Game1.mailbox.Count > 0 && frame == 0 && animationFrames == 0)
            {
                float num = (float)(4.0 * Math.Round(Math.Sin(DateTime.Now.TimeOfDay.TotalMilliseconds / 250.0), 2));
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((tileLocation.X * Game1.tileSize), (tileLocation.Y * Game1.tileSize - Game1.tileSize * 3 / 2 - 48) + num)), new Rectangle?(new Rectangle(141, 465, 20, 24)), Color.White * 0.75f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((17 * Game1.tileSize) / 10000.0 + 9.99999997475243E-07 + 0.00680000009015203));
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((tileLocation.X * Game1.tileSize + Game1.tileSize / 2 + Game1.pixelZoom), (tileLocation.Y * Game1.tileSize - Game1.tileSize - 24 - Game1.tileSize / 8) + num)), new Rectangle(189, 423, 15, 13), Color.White, 0.0f, new Vector2(7f, 6f), 4f, SpriteEffects.None, (float)((17 * Game1.tileSize) / 10000.0 + 9.99999974737875E-06 + 0.00680000009015203));
            }
        }

        public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1)
        {
                draw(spriteBatch, xNonTile, yNonTile, alpha);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            tileindex = 0;
            spriteBatch.Draw(texture, location + new Vector2((Game1.tileSize / 2), (Game1.tileSize / 2)), sourceRectangle, Color.White * transparency, 0.0f, new Vector2(tilesize.Width / 2, tilesize.Width), Game1.pixelZoom * (scaleSize < 0.2 ? scaleSize : scaleSize / 2.00f), SpriteEffects.None, layerDepth);
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, SFarmer f)
        {

        }

        public override Item getOne()
        {
            return new CustomMachine(blueprint);
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("id", id);

            if(location != null)
                data.Add("location", location.name);

            if (activeRecipe != null)
                data.Add("recipe", activeRecipe.id.ToString());

            if (completionTime != null)
                data.Add("completionTime", completionTime.timestamp.ToString());

            return data;
        }

        public object getReplacement()
        {
            Chest replacement = new Chest(true);
            if (heldObject != null)
                replacement.items.Add(heldObject);

            activeMachines.Remove(this);

            replacement.tileLocation = tileLocation;

            return replacement;
        }

        public override bool canBePlacedInWater()
        {
            return blueprint.water;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            if (additionalSaveData.ContainsKey("completionTime"))
                completionTime = new STime(int.Parse(additionalSaveData["completionTime"]));

            build(machines.Find(cmb => additionalSaveData["id"] == cmb.fullid));

            if (additionalSaveData.ContainsKey("location"))
            {
                location = Game1.getLocationFromName(additionalSaveData["location"]);
                activeMachines.AddOrReplace(this);
            }

            if (additionalSaveData.ContainsKey("recipe"))
                activeRecipe = blueprint.production.Find(r => r.id == additionalSaveData["recipe"]);

            if (additionalSaveData.ContainsKey("completionTime"))
                completionTime = new STime(int.Parse(additionalSaveData["completionTime"]));
            else
                completionTime = STime.CURRENT;

            Chest c = (Chest)replacement;
            tileLocation = c.tileLocation;
            if (c.items.Count > 0 && c.items[0] is SObject o)
                heldObject = (SObject) o.getOne();
            updateWhenCurrentLocation(Game1.currentGameTime);
            startAutoProduction();
            wasBuild = true;

        }

        public override void DayUpdate(GameLocation location)
        {

        }

        public override bool checkForAction(SFarmer who, bool justCheckingForActivity = false)
        {
            
            if (blueprint.category == "Mailbox")
            {
                if (justCheckingForActivity)
                    return true;

                shakeTimer = 100;

                if (Game1.mailbox.Count != 0)
                    Game1.getFarm().mailbox();
                else
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8429"));

                return false;
            }

            if ((blueprint.description.Contains("arecrow") || getCategoryName() == "Scarescrow" || getCategoryName() == "Rarescrow"))
            {
                if (justCheckingForActivity)
                    return true;
                
                shakeTimer = 100;

                if (specialVariable == 0)
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12926"));
                else if (specialVariable != 1)
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12929", specialVariable));
                else
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12927"));

                return false;
            }

            if (heldObject == null)
                return (who.ActiveObject is SObject o && findRecipe(maxed(o)) != null && hasStarterMaterials(who.items));
                
            if (!readyForHarvest)
                return false;

            if (justCheckingForActivity)
                return true;

            return deliverProduce(who);
        }

        public bool deliverProduce(SFarmer who, bool toInventory = true)
        {
            if (toInventory && who.IsMainPlayer && !who.addItemToInventoryBool(heldObject, false))
            {
                Game1.showRedMessage("Inventory Full");
                return false;
            }

            if (!toInventory)
                Game1.createItemDebris(heldObject, tileLocation * Game1.tileSize, -1, null);

            clear();

            if (toInventory)
                Game1.playSound("coin");

            startAutoProduction();

            return false;
        }

        private void startAutoProduction()
        {
            if (!isWorking && blueprint.production != null && blueprint.production[0].materials == null)
                startProduction(null, blueprint.production[0], null);
            else if (config.automation)
                startAutomation();
        }

        private void clear()
        {
            heldObject = null;
            readyForHarvest = false;
            activeRecipe = null;
            completionTime = null;
        }

        public override bool performDropDownAction(SFarmer who)
        {
            location = Game1.currentLocation;
            activeMachines.AddOrReplace(this);
            startAutoProduction();
            return false;
        }

        public override bool performObjectDropInAction(SObject dropIn, bool probe, SFarmer who)
        {
            if (heldObject != null)
                return false;

            if (heldObject == null)
                clear();

            List<List<Item>> items = getItemLists(who);
            RecipeBlueprint recipe = findRecipe(maxed(dropIn));
            bool hasRecipe = recipe != null;
            bool hasStarter = hasStarterMaterials(items);
            bool hasIngredients = hasRecipe && recipe.hasIngredients(items);
            bool canProduce = hasIngredients && hasStarter;
            
            if (probe)
                return canProduce;
            
            if (canProduce)
            {
                startProduction(dropIn, findRecipe(dropIn), items);
                Game1.playSound("Ship");
                return false;
            }

            if(!hasStarter)
            {
                Game1.showRedMessage($"Requires {blueprint.starter.stack} {blueprint.starter.name}.");
                return false;
            }

            if (!hasIngredients && hasRecipe)
            {
                int stack = recipe.materials.Find(p => p.index == dropIn.parentSheetIndex).stack;
                Game1.showRedMessage($"Requires {stack} {dropIn.name}.");
                return false;
            }

            return false;
        }

        public override bool performToolAction(Tool t)
        {
            if (heldObject != null)
            {
                if (readyForHarvest)
                {
                    deliverProduce(Game1.player, false);
                    return false;
                }
                else if (t is Pickaxe)
                {
                    deliverProduce(Game1.player, false);
                }
                else
                    return false;
            }


            if (t == null || !t.isHeavyHitter() || t is MeleeWeapon || !(t is Pickaxe))
                return false;           

            clear();
            location = null;
            activeMachines.Remove(this);
            Game1.playSound("hammer");
            Game1.createItemDebris(getOne(), tileLocation * Game1.tileSize, -1, null);
            Game1.currentLocation.objects.Remove(tileLocation);

            return false;
        }

    }
}
