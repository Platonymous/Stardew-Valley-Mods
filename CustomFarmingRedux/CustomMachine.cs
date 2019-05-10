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
using PyTK;
using System.Reflection;
using StardewValley.Menus;

namespace CustomFarmingRedux
{
    public class CustomMachine : SObject, ICustomObject, ISyncableElement
    {
        internal IModHelper Helper = CustomFarmingReduxMod._helper;
        internal IMonitor Monitor = CustomFarmingReduxMod._monitor;
        internal Config config = CustomFarmingReduxMod._config;
        public virtual string folder => blueprint.pack.baseFolder;
        internal List<CustomMachineBlueprint> machines = CustomFarmingReduxMod.machines;
        internal static List<CustomMachine> activeMachines = new List<CustomMachine>();
        public virtual Texture2D texture { get; private set; }
        public virtual Rectangle sourceRectangle => Game1.getSourceRectForStandardTileSheet(texture, blueprint.tileindex + frame, tilesize.Width, tilesize.Height);
        internal MachineHandler machineHandler = null;

        public CustomMachineBlueprint blueprint;

        private bool active = true;
        private bool wasBuild = false;
        public virtual bool isWorking { get => active && !readyForHarvest && ((completionTime != null && activeRecipe != null) || blueprint.production == null); }
        public string id { get; private set; }
        public string conditions = null;
        public virtual STime completionTime { get; set; }
        private int tileindex;
        private Rectangle tilesize = new Rectangle(0, 0, 16, 32);
        private int _frame = 0;
        private int skipFrame = 10;
        private int animationFrames = 0;
        private int counter = 0;
        public virtual RecipeBlueprint activeRecipe { get; set; }
        public RecipeBlueprint starterRecipe;
        public GameLocation location;
        private CustomObjectData data;
        public bool meetsConditions = true;
        public bool checkedToday = false;
        private bool skipDrop = false;
        private int identifier => (int)((double)tileLocation.X * 2000.0 + (double)tileLocation.Y);
        public PySync syncObject { get; set; }


        public virtual int frame {
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
            syncObject = new PySync(this);
            syncObject.init();
        }

        public CustomMachine(CustomObjectData data)
        {
            this.data = data;
            build(machines.Find(m => m.fullid == data.id));
            wasBuild = true;
        }

        public CustomMachine(CustomMachineBlueprint blueprint)
        {
            build(blueprint);
            wasBuild = true;
        }

        public virtual void build(CustomMachineBlueprint blueprint)
        {
            if (syncObject == null)
            {
                syncObject = new PySync(this);
                syncObject.init();
            }
            CustomFarmingReduxMod._helper.Events.GameLoop.DayStarted += (s, e) => checkedToday = false;
            CustomFarmingReduxMod.machineHandlers.TryGetValue(blueprint.fullid, out machineHandler);

            if (blueprint.worklight)
                this.IsOn = false;

            if (blueprint.category == "Chest" && !(heldObject.Value is Chest))
                heldObject.Value = new Chest(true);

            if (data == null)
                data = CustomObjectData.collection.ContainsKey(blueprint.fullid) ? CustomObjectData.collection[blueprint.fullid] : new CustomObjectData(blueprint.fullid, $"{blueprint.name}/{blueprint.price}/-300/Crafting -9/{blueprint.description}/true/true/0/{blueprint.name}", blueprint.getTexture(), Color.White, blueprint.tileindex, true, typeof(CustomMachine), (blueprint.crafting == null || blueprint.crafting == "") ? null : new CraftingData(blueprint.name, blueprint.crafting));

            name = blueprint.name;
            if (blueprint.readyindex < 0)
                blueprint.readyindex = blueprint.tileindex;
            this.blueprint = blueprint;

            removeLightSource();

            texture = blueprint.getTexture();
            id = blueprint.fullid;
            conditions = blueprint.workconditions;
            ParentSheetIndex = data.sdvId;
            bigCraftable.Value = true;
            type.Value = "Crafting";
            tilesize = new Rectangle(0, 0, blueprint.tilewidth, blueprint.tileheight);
            boundingBox.Value = new Rectangle(0, 0, blueprint.tilewidth * 4, blueprint.tileheight * 4);
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
            o.Stack = int.MaxValue;
            return o;
        }

        public override void initializeLightSource(Vector2 tileLocation, bool mineShaft = false)
        {
            if (blueprint.lightsource)
                this.lightSource = new LightSource(4, new Vector2((float)((double)tileLocation.X * 64.0 + 32.0), tileLocation.Y * 64f), blueprint.lightradius, new Color(blueprint.lightcolor[0], blueprint.lightcolor[1], blueprint.lightcolor[2], blueprint.lightcolor[3]), identifier);
            else
                this.lightSource = null;
        }
        
        public virtual RecipeBlueprint findRecipe(List<IList<Item>> items)
        {
            RecipeBlueprint result = null;
            if (blueprint.production != null)
                foreach (RecipeBlueprint r in blueprint.production)
                    if (r.hasIngredients(items))
                        result = r;

            return result;
        }

        public virtual RecipeBlueprint findRecipeFor(Item item)
        {
            if (blueprint.production == null)
                return null;

            return blueprint.production.Find(rec => rec.materials != null && rec.materials.Count > 0 && rec.fitsIngredient(item, rec.materials));
        }

        public virtual RecipeBlueprint findRecipe(List<Item> items)
        {
            return findRecipe(new List<IList<Item>>() { items });
        }

        public virtual RecipeBlueprint findRecipe(Item item)
        {
            return findRecipe(new List<IList<Item>>() { new List<Item>() { item } });
        }

        public virtual bool hasStarterMaterials(List<IList<Item>> items)
        {
            if (starterRecipe == null)
                return true;

            if (starterRecipe.hasIngredients(items))
                return true;

            return false;
        }

        public virtual bool hasStarterMaterials(List<Item> items)
        {
            return hasStarterMaterials(new List<IList<Item>>() { items });
        }

        public virtual List<IList<Item>> getItemLists(SFarmer player = null)
        {
            List<IList<Item>> items = new List<IList<Item>>();

            if (player != null)
                items.Add(player.Items);

            return items;
        }

        public virtual void getReadyForHarvest()
        {
            if (blueprint.worklight)
                removeLightSource();

            if (blueprint.production == null || activeRecipe == null)
                return;

            minutesUntilReady.Set(-1);
            completionTime = null;
            heldObject.Value = createProduce();
            readyForHarvest.Value = true;
            activeRecipe = null;
            syncObject.MarkDirty();
        }

        public virtual void setReadyForHarvest()
        {
            minutesUntilReady.Set(-1);
            completionTime = null;
            readyForHarvest.Value = true;
            activeRecipe = null;
            syncObject.MarkDirty();

        }

        public virtual void startProduction(SObject obj, RecipeBlueprint recipe, List<IList<Item>> items)
        {
            activeRecipe = recipe;
            if (completionTime == null)
                completionTime = STime.CURRENT + recipe.time;

            if (location == null)
                location = new List<GameLocation>(Game1.locations).Find(g => new List<SObject>(g.Objects.Values).Contains(this));

            if (!checkedToday)
                meetsConditions = PyUtils.CheckEventConditions(conditions, this);

            checkedToday = true;

            if (!meetsConditions)
            {
                completionTime = STime.CURRENT + STime.DAY;
                completionTime.hour = 6;
                completionTime.minute = 0;
                completionTime += recipe.time;
            }

            minutesUntilReady.Set((completionTime + 1 - STime.CURRENT).timestamp);
            
            if (starterRecipe != null)
                starterRecipe.consumeIngredients(items);

            if (recipe.materials != null)
                recipe.consumeIngredients(items, obj);

            if (obj != null)
            {
                heldObject.Value = (SObject)obj.getOne();
                heldObject.Value.Stack = obj.Stack;
            }

            syncObject.MarkDirty();

        }
        public virtual SObject createProduce()
        {
            if (!(heldObject.Value is Chest))
            {
                CustomFarmingReduxMod.machineHandlers.TryGetValue(blueprint.fullid, out machineHandler);
                if (machineHandler is MachineHandler handler && handler.GetOutput != null && handler.GetOutput(heldObject.Value, this, blueprint.fullid, activeRecipe.name) is SObject output)
                    return output;

                return activeRecipe.createObject(heldObject.Value);
            }
            else
                return (SObject)SaveHandler.rebuildElement(heldObject.Value.name, heldObject.Value);
        }

        public override bool minutesElapsed(int minutes, GameLocation environment)
        {
            if (isWorking && completionTime != null && STime.CURRENT >= completionTime && activeRecipe != null)
                getReadyForHarvest();

            return false;
        }

        public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
        {
            location = environment;
            bool isWorking = this.isWorking;

            if (tileLocation == Vector2.Zero)
                if (!Game1.currentLocation.objects.ContainsKey(tileLocation) || Game1.currentLocation.objects[tileLocation] != this)
                    if (new List<SObject>(Game1.currentLocation.objects.Values).Contains(this))
                        tileLocation.Value = new List<Vector2>(Game1.currentLocation.objects.Keys).Find(k => Game1.currentLocation.objects[k] == this);

            if (blueprint.worklight)
                this.IsOn = isWorking;
            else
                this.IsOn = true;

            if (!IsOn && lightSource != null)
            {
                removeLightSource();
                this.IsOn = false;
            }

            if (IsOn && lightSource == null)
                addLightSource();

            if (!wasBuild || blueprint.asdisplay)
            {
                shakeTimer = 0;
                return;
            }

            if (isWorking && completionTime != null && STime.CURRENT >= completionTime && activeRecipe != null)
                getReadyForHarvest();

            if (!(isWorking && completionTime != null))
                startAutoProduction();

            base.updateWhenCurrentLocation(time, environment);

            if (completionTime != null)
                minutesUntilReady.Set((completionTime + 1 - STime.CURRENT).timestamp);
        }

        public override string DisplayName { get => name; set => base.DisplayName = value; }

        public override string getDescription()
        {
            return Game1.parseText(blueprint.description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4);
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            if (blueprint.category == "Dresser" || blueprint.category == "Chest")
                active = false;

            if (blueprint.category == "Mailbox" && Game1.mailbox is IList<string> mb && mb.Count == 0)
                active = false;
            else
                active = true;

            if (animationFrames != 0)
            {
                if (!blueprint.conditionalanimation || meetsConditions)
                {
                    counter++;
                    counter = counter > skipFrame ? 0 : counter;
                    frame = counter == 0 ? frame + 1 : frame;
                    frame = frame >= animationFrames + 1 ? 1 : frame;
                }
            }
            else if (animationFrames == 1)
                frame = 1;
            else
                frame = 0;

            Vector2 vector2 = (blueprint.pulsate) ? getScale() * Game1.pixelZoom  : new Vector2(0, 4) * Game1.pixelZoom;
            Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((x * Game1.tileSize), (y * Game1.tileSize - Game1.tileSize)));
            Rectangle destinationRectangle = new Rectangle((int)(local.X - vector2.X / 2.0) + (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)(local.Y - vector2.Y / 2.0) + (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)((tilesize.Width * 4) + (double)vector2.X), (int)((tilesize.Height * 4) + vector2.Y / 2.0));

            if (texture is ScaledTexture2D s)
                spriteBatch.Draw(s.STexture, destinationRectangle, new Rectangle?(new Rectangle((int)(sourceRectangle.X * s.Scale), (int)(sourceRectangle.Y * s.Scale), (int)(sourceRectangle.Width * s.Scale), (int)(sourceRectangle.Height * s.Scale))), Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, (float)(Math.Max(0.0f, ((y + 1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + x * 9.99999974737875E-06));
            else
                spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, (float)(Math.Max(0.0f, ((y + 1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + x * 9.99999974737875E-06));

            if (readyForHarvest && heldObject.Value != null)
            {
                Texture2D tilesheet = null;
                Rectangle csourceRectangle = new Rectangle();
                bool cbig = false;
                if(heldObject.Value.bigCraftable.Value && CustomObjectData.collection.Exists(c => c.Value.sdvId == heldObject.Value.ParentSheetIndex) && CustomObjectData.collection.Find(c => c.Value.sdvId == heldObject.Value.ParentSheetIndex) is KeyValuePair<string, CustomObjectData> cod)
                {
                    cbig = true;
                    tilesheet = cod.Value.texture;
                    csourceRectangle = cod.Value.sourceRectangle;
                }

                if (!cbig)
                {
                    tilesheet = (heldObject.Value is CustomObject co) ? co.texture : !heldObject.Value.bigCraftable.Value ? Game1.objectSpriteSheet : Game1.bigCraftableSpriteSheet;
                    csourceRectangle = (heldObject.Value is CustomObject cobj) ? cobj.sourceRectangle : Game1.getSourceRectForStandardTileSheet(tilesheet, heldObject.Value.ParentSheetIndex, 16, heldObject.Value.bigCraftable.Value ? 32 : 16);
                }

                Color color = (heldObject.Value is CustomObject cco) ? cco.color : (heldObject.Value is ColoredObject cvo) ? cvo.color.Value : Color.White;
                float num = (float)(4.0 * Math.Round(Math.Sin(DateTime.Now.TimeOfDay.TotalMilliseconds / 250.0), 2));
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((x * Game1.tileSize - 8), (y * Game1.tileSize - Game1.tileSize * 3 / 2 - 16) + num)), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(((y + 1) * Game1.tileSize) / 10000.0 + 9.99999997475243E-07 + tileLocation.X / 10000.0 + (parentSheetIndex == 105 ? 0.00150000001303852 : 0.0)));
                spriteBatch.Draw(tilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((x * Game1.tileSize + Game1.tileSize / 2), (y * Game1.tileSize - Game1.tileSize - Game1.tileSize / 8 - (heldObject.Value.bigCraftable.Value ? 64 : 0)) + num)), csourceRectangle, color * 0.75f, 0.0f, new Vector2(8f, 8f), Game1.pixelZoom, SpriteEffects.None, (float)(((y + 1) * Game1.tileSize) / 10000.0 + 9.99999974737875E-06 + tileLocation.X / 10000.0 + 0.0));
            }

            if (blueprint.showitem && heldObject.Value != null)
            {
                Rectangle displayDestinationRectangle = new Rectangle((int)(local.X - vector2.X / 2.0) + blueprint.itempos[0] + (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)(local.Y - vector2.Y / 2.0) + blueprint.itempos[1] + (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)(blueprint.itemzoom * ((int)(Game1.tileSize + vector2.X))), (int)(blueprint.itemzoom * ((int)((Game1.tileSize) + vector2.Y / 2.0))));
                spriteBatch.Draw(heldObject.Value is CustomObject co ?  co.texture : Game1.objectSpriteSheet, displayDestinationRectangle, heldObject.Value is CustomObject cor ? cor.sourceRectangle : Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, heldObject.Value.ParentSheetIndex, 16, 16), Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, (float)(Math.Max(0.0f, ((y + 1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + (x + 1) * 9.99999974737875E-06));
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

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {
            tileindex = 0;
            spriteBatch.Draw(texture, location + new Vector2((Game1.tileSize / 2), (Game1.tileSize / 2)), sourceRectangle, Color.White * transparency, 0.0f, new Vector2(tilesize.Width / 2, tilesize.Width), Game1.pixelZoom * (scaleSize < 0.2 ? scaleSize : scaleSize / 2.00f), SpriteEffects.None, layerDepth);
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, SFarmer f)
        {
            spriteBatch.Draw(texture, objectPosition, sourceRectangle, Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + 2) / 10000f));
        }

        public override Item getOne()
        {
            return new CustomMachine(blueprint);
        }

        public virtual Dictionary<string, string> getAdditionalSaveData()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("id", id);

            if (location != null)
                data.Add("location", location.Name);

            if (activeRecipe != null && !blueprint.asdisplay)
                data.Add("recipe", activeRecipe.id.ToString());

            if (readyForHarvest)
                data.Add("ready", "true");

            if (completionTime != null)
                data.Add("completionTime", completionTime.timestamp.ToString());

            data.Add("tileLocation", tileLocation.X + "," + tileLocation.Y);

            return data;
        }

        public virtual object getReplacement()
        {
            Chest replacement = new Chest(true);
            if (heldObject.Value != null)
                replacement.items.Add(heldObject);

            replacement.TileLocation = tileLocation;

            return replacement;
        }

        public override bool canBePlacedInWater()
        {
            return blueprint.water;
        }

        public virtual void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            blueprint = machines.Find(cmb => additionalSaveData["id"] == cmb.fullid);

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

            checkedToday = false;
            meetsConditions = PyUtils.CheckEventConditions(conditions, this);

            if (completionTime != null && completionTime > STime.CURRENT && !meetsConditions)
            {
                int minutesToCompletion = (completionTime - STime.CURRENT).timestamp;
                completionTime = STime.CURRENT + STime.DAY;
                completionTime.hour = 6;
                completionTime.minute = 0;
                completionTime += minutesToCompletion < 0 ? 0 : minutesToCompletion;
                minutesUntilReady.Set((completionTime + 1 - STime.CURRENT).timestamp);
            }

            Chest c = (Chest)replacement;
            tileLocation.Value = c.TileLocation;

            if(additionalSaveData.ContainsKey("tileLocation"))
                tileLocation.Value = additionalSaveData["tileLocation"].Split(',').toList(s => int.Parse(s)).toVector<Vector2>();
            if (c.items.Count > 0 && c.items[0] is SObject o)
                heldObject.Value = o;

            if (location == null)
                location = new List<GameLocation>(Game1.locations).Find(g => new List<SObject>(g.Objects.Values).Contains(this));

            updateWhenCurrentLocation(Game1.currentGameTime, location);
            startAutoProduction();
            wasBuild = true;

            if (additionalSaveData.ContainsKey("ready") && additionalSaveData["ready"] == "true")
                setReadyForHarvest();
        }

        public override void DayUpdate(GameLocation location)
        {
            checkedToday = false;
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

            if (blueprint.category == "Chest")
            {
                if (justCheckingForActivity)
                    return true;
                Game1.activeClickableMenu = (IClickableMenu)new ItemGrabMenu((IList<Item>)(heldObject.Value as Chest).items, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems), new ItemGrabMenu.behaviorOnItemSelect((heldObject.Value as Chest).grabItemFromInventory), (string)null, new ItemGrabMenu.behaviorOnItemSelect((heldObject.Value as Chest).grabItemFromChest), false, true, true, true, true, 1, null, -1, (object)null);
            }

            if (machineHandler is MachineHandler handler && handler.ClickAction != null)
            {
                if (justCheckingForActivity)
                    return true;
                handler.ClickAction(this);
            }

            if (blueprint.category == "Dresser")
            {
                if (justCheckingForActivity)
                    return true;

                shakeTimer = 100;


                if (CustomFarmingReduxMod.hasKisekae && CustomFarmingReduxMod.kisekae is Mod ks)
                    ks.GetType().GetMethod("OpenMenu", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(ks, null);
                else
                    Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.Wizard);

                return false;
            }

            if ((blueprint.description.Contains("arecrow") || getCategoryName() == "Scarescrow" || getCategoryName() == "Rarescrow"))
            {
                if (justCheckingForActivity)
                    return true;

                shakeTimer = 100;

                if (SpecialVariable == 0)
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12926"));
                else if (SpecialVariable != 1)
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12929", SpecialVariable));
                else
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12927"));

                return false;
            }

            if (heldObject.Value == null)
                return (who.ActiveObject is SObject o && findRecipe(maxed(o)) != null && hasStarterMaterials(new List<IList<Item>>() { who.Items }));

            if (!readyForHarvest)
                return false;

            if (justCheckingForActivity)
                return true;

            return deliverProduce(who);
        }

        public virtual bool deliverProduce(SFarmer who, bool toInventory = true)
        {
            skipDrop = true;

            if (toInventory && !who.addItemToInventoryBool(heldObject, false))
            {
                Game1.showRedMessage("Inventory Full");
                return false;
            }

            if (!toInventory)
                Game1.createItemDebris(heldObject, tileLocation.Value * Game1.tileSize, -1, null);

            clear();

            if (toInventory)
                Game1.playSound("coin");

            startAutoProduction();

            return false;
        }

        public virtual void startAutoProduction()
        {
            if (!isWorking && blueprint.production != null && blueprint.production[0].materials == null)
                startProduction(null, blueprint.production[0], null);
        }

        public virtual void clear()
        {
            heldObject.Value = null;
            readyForHarvest.Value = false;
            activeRecipe = null;
            completionTime = null;
        }

        public virtual void removeLightSource()
        {
            try
            {
                if (location != null)
                {
                    //location.sharedLights.Filter((Func<LightSource, bool>)(l => l.Identifier != identifier));

                    var f = (Func<LightSource, bool>)(l => l.Identifier != identifier);
                    object sl = location.GetType().GetField("sharedLights").GetValue(location);
                    sl.GetType().GetMethod("Filter").Invoke(sl, new[] { f });

                    lightSource = null;
                }
            }
            catch
            {

            }
        }

        public virtual void addLightSource()
        {
            if (location != null)
            {
                try
                {
                    initializeLightSource(tileLocation, false);
                    //location.sharedLights.Filter((Func<LightSource, bool>)(l => l.Identifier != identifier));

                    var f = (Func<LightSource, bool>)(l => l.Identifier != identifier);
                    object sl = location.GetType().GetField("sharedLights").GetValue(location);
                    sl.GetType().GetMethod("Filter").Invoke(sl, new[] { f });

                    if (lightSource != null)
                    {
                        //location.sharedLights.Add(lightSource.Clone());
                        sl.GetType().GetMethod("Add").Invoke(sl, new[] { lightSource.Clone() });
                    }
                }
                catch
                {

                }
            }
        }

        public override bool performDropDownAction(SFarmer who)
        {
            location = Game1.currentLocation;
            activeMachines.AddOrReplace(this);
            startAutoProduction();
            return false;
        }

        public virtual string getCatName(int cat)
        {
            SObject s = new SObject(Vector2.Zero, 399);
            s.Category = cat;
            return s.getCategoryName();
        }

        public bool performAutomatedDropInAction(Item dropInItem, List<Item> itemlist, bool probe)
        {
            if (!(dropInItem is SObject))
                return false;

            var items = new List<IList<Item>>() { itemlist };            

            if (heldObject.Value != null)
                return false;

            if (heldObject.Value == null)
                clear();

            RecipeBlueprint recipe = findRecipeFor(maxed((SObject)dropInItem));
            bool hasRecipe = recipe != null;
            bool hasStarter = hasStarterMaterials(items);
            bool hasIngredients = hasRecipe && recipe.hasIngredients(items, dropInItem);
            bool canProduce = hasIngredients && hasStarter;


            if (!canProduce)
                return false;

            if (probe)
            {
                if (canProduce)
                    heldObject.Value = (SObject)dropInItem;

                return canProduce;
            }

            if (canProduce && blueprint.conditionaldropin)
            {
                if (!checkedToday)
                    meetsConditions = PyUtils.CheckEventConditions(conditions, this);

                checkedToday = true;

                if (!meetsConditions)
                    return false;
            }

            if (canProduce)
            {
                startProduction((SObject)dropInItem, findRecipeFor(maxed((SObject)dropInItem)), items);
                return false;
            }

            if (!hasStarter)
                return false;

            if (!hasIngredients && hasRecipe)
            {
                string ingredients = String.Join(",", recipe.materials.toList(m => m.stack + "x " + (m.index > 0 ? Game1.objectInformation[m.index].Split('/')[4] : "Category " + getCatName(m.index))));
                Game1.showRedMessage($"Missing Ingredients. ({ingredients})");
                return false;
            }

            return false;
        }

        public override bool performObjectDropInAction(Item dropInItem, bool probe, SFarmer who)
        {
            return performDropInAction(dropInItem, probe, getItemLists(who));
        }

        public virtual RecipeBlueprint findRecipeForItemType(SObject item)
        {
            CustomFarmingReduxMod.machineHandlers.TryGetValue(blueprint.fullid, out machineHandler);
            if (machineHandler is MachineHandler handler && handler.CheckInput != null && !handler.CheckInput(item, this, blueprint.fullid))
                return null;

            return findRecipeFor(maxed((SObject)item));
        }

        public bool performDropInAction(Item dropInItem,bool probe, List<IList<Item>> items)
        {
            if (skipDrop || heldObject.Value != null)
            {
                skipDrop = false;
                return false;
            }

            if (!(dropInItem is SObject))
                return false;

            if (heldObject.Value != null)
                return false;

            if (heldObject.Value == null)
                clear();

            if (blueprint.asdisplay && dropInItem is SObject d)
            {
                heldObject.Value = (SObject)d.getOne();
                return false;
            }

            RecipeBlueprint recipe = findRecipeForItemType((SObject)dropInItem);
            bool hasRecipe = recipe != null;
            bool hasStarter = hasStarterMaterials(items);
            bool hasIngredients = hasRecipe && recipe.hasIngredients(items, dropInItem);
            bool canProduce = hasIngredients && hasStarter;

            if (probe)
            {
                if (canProduce)
                    heldObject.Value = (SObject)dropInItem;

                return canProduce;
            }

            if (canProduce && blueprint.conditionaldropin)
            {
                if (!checkedToday)
                    meetsConditions = PyUtils.CheckEventConditions(conditions, this);

                checkedToday = true;

                if (!meetsConditions)
                {
                    Game1.showRedMessage($"This machine is not working under the current conditions");
                    return false;
                }
            }

            if (canProduce)
            {
                startProduction((SObject)dropInItem, findRecipeFor(maxed((SObject)dropInItem)), items);
                Game1.playSound("Ship");
                return false;
            }

            if (!hasStarter)
            {
                Game1.showRedMessage($"Requires {blueprint.starter.stack}x {(blueprint.starter.index > 0 ? Game1.objectInformation[blueprint.starter.index].Split('/')[4] : "Category " + getCatName(blueprint.starter.index))}.");
                return false;
            }

            if (!hasIngredients && hasRecipe)
            {
                string ingredients = String.Join(",", recipe.materials.toList(m => m.stack + "x " + (m.index > 0 ? Game1.objectInformation[m.index].Split('/')[4] : "Category " + getCatName(m.index))));
                Game1.showRedMessage($"Missing Ingredients. ({ingredients})");
                return false;
            }

            return false;
        }

        public override bool performToolAction(Tool t, GameLocation location)
        {
            if (blueprint.category == "Chest" && (heldObject.Value as Chest).items.Count != 0)
                return false;

            Farmer farmer = t.getLastFarmerToUse();

            if (heldObject.Value != null && !blueprint.asdisplay && readyForHarvest)
            {
                    deliverProduce(farmer, false);
                    return false;
            }

            if (t == null || !t.isHeavyHitter() || t is MeleeWeapon || !(t is Pickaxe))
                return false;

            if (blueprint.asdisplay && heldObject.Value != null)
            {
                heldObject.Value = null;
                return false;
            }

            clear();
            location = null;
            activeMachines.Remove(this);
            Game1.playSound("hammer");

            if (!Game1.currentLocation.objects.ContainsKey(tileLocation) || Game1.currentLocation.objects[tileLocation] != this)
                tileLocation.Value = new List<Vector2>(Game1.currentLocation.objects.Keys).Find(k => Game1.currentLocation.objects[k] == this);

            removeLightSource();
            Game1.createItemDebris(getOne(), tileLocation.Value * Game1.tileSize, -1, null);
            Game1.currentLocation.objects.Remove(tileLocation);

            return false;
        }

        public virtual ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            return new CustomMachine();
        }

        public Dictionary<string, string> getSyncData()
        {
            var data = new Dictionary<string, string>() { { "completionTime", completionTime != null ? completionTime.timestamp.ToString() : "-1" } };

            if (activeRecipe != null && !blueprint.asdisplay)
                data.Add("recipe", activeRecipe.id.ToString());

            return data;
        }

        public void sync(Dictionary<string, string> syncData)
        {
            if (syncData.ContainsKey("completionTime"))
                if (int.Parse(syncData["completionTime"]) is int i && i != -1)
                    completionTime = new STime(i);

            if (syncData.ContainsKey("recipe"))
                activeRecipe = blueprint.production.Find(r => r.id == syncData["recipe"]);
        }
    }
}
