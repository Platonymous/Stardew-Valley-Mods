using SObject = StardewValley.Object;
using PyTK.CustomElementHandler;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using StardewModdingAPI.Utilities;
using SFarmer = StardewValley.Farmer;
using PyTK.Extensions;

namespace CustomFarmingRedux
{
    public class CustomMachine : SObject, ISaveElement
    {
        internal IModHelper Helper = CustomFarmingReduxMod._helper;
        internal IMonitor Monitor = CustomFarmingReduxMod._monitor;
        internal string folder = CustomFarmingReduxMod.folder;
        internal List<CustomMachineBlueprint> machines = CustomFarmingReduxMod.machines;

        private CustomMachineBlueprint blueprint;

        private Texture2D texture;
        private Rectangle sourceRectangle {
            get
            {
               return Game1.getSourceRectForStandardTileSheet(texture, blueprint.tileindex + frame, tilesize.Width, tilesize.Height);
            }
        }
        private bool isWorking = false;
        private string id;
        private SDate completionTime;
        private int tileindex;
        private Rectangle tilesize = new Rectangle(0, 0, 16, 32);
        private int _frame = 0;
        private int skipFrame = 10;
        private int animationFrames = 1;
        private int counter = 0;
        private RecipeBlueprint activeRecipe;
        private RecipeBlueprint starterRecipe;
        private GameLocation location;
        private int frame {
            get
            {
                if (!isWorking)
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
        }

        private void build(CustomMachineBlueprint blueprint)
        {
            name = blueprint.name;
            Helper.Reflection.GetField<string>(this, "description").SetValue(blueprint.description);
            this.blueprint = blueprint;
            texture = Helper.Content.Load<Texture2D>($"{folder}/{blueprint.folder}/{blueprint.texture}");
            id = $"{blueprint.folder}.{blueprint.file}.{blueprint.id}";
            parentSheetIndex = -1;
            skipFrame = 60 / blueprint.fps;
            animationFrames = blueprint.frames;
            tileindex = blueprint.tileindex;
            if (blueprint.starter != null)
            {
                starterRecipe = new RecipeBlueprint();
                starterRecipe.materials = new List<IngredientBlueprint>();
                starterRecipe.materials.Add(blueprint.starter);
            }
                
        }

        private RecipeBlueprint findRecipe(List<List<Item>> items)
        {
            activeRecipe = null;

            foreach (RecipeBlueprint r in blueprint.production)
                if (r.hasIngredients(items))
                    activeRecipe = r;

            return activeRecipe;
        }

        private bool hasStarterMaterials(List<List<Item>> items)
        {
            if (starterRecipe == null)
                return true;

            if (starterRecipe.hasIngredients(items))
                return true;

            return false;
        }

        private List<List<Item>> getItemLists(SFarmer player = null)
        {
            List<List<Item>> items = new List<List<Item>>();

            if (player != null)
                items.Add(player.items);

            if (CustomFarmingReduxMod._config.automation)
            {
                List<Vector2> tiles = Utility.getAdjacentTileLocations(tileLocation);
                if (location is GameLocation)
                    foreach (SObject obj in location.objects.Values)
                        if (obj is Chest c && !items.Contains(c.items))
                            items.Add(c.items);
            }
            
            return items;
        }

        private Color getColor(SObject obj)
        {
            return Color.White;
        }

        private Item createItem()
        {
            if (activeRecipe.colored)
            {
                if (activeRecipe.color != null)
                    return activeRecipe.createObject(new Color(activeRecipe.color.toVector<Vector4>()));
                else
                    return activeRecipe.createObject(getColor(null));
            }
            else
                return activeRecipe.createObject();
        }

        public override string DisplayName { get => name; set => base.DisplayName = value; }

        public override string getDescription()
        {
            return Game1.parseText(blueprint.description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4);
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            counter++;
            counter = counter > skipFrame ? 0 : counter;
            frame = counter == 0 ? frame + 1 : frame;
            frame = frame >= animationFrames ? 0 : frame;

            Vector2 vector2 = getScale() * Game1.pixelZoom;
            Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((x * Game1.tileSize), (y * Game1.tileSize - Game1.tileSize)));
            Rectangle destinationRectangle = new Rectangle((int)(local.X - vector2.X / 2.0) + (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)(local.Y - vector2.Y / 2.0) + (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0), (int)(tilesize.Width + (double)vector2.X), (int)((tilesize.Height) + vector2.Y / 2.0));
            spriteBatch.Draw(texture, destinationRectangle, sourceRectangle, Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, (float)(Math.Max(0.0f, ((y + 1) * Game1.tileSize - Game1.pixelZoom * 6) / 10000f) + x * 9.99999974737875E-06));
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

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, StardewValley.Farmer f)
        {
            spriteBatch.Draw(texture, objectPosition, sourceRectangle, Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + 2) / 10000f));
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("id", id);
            return data;
        }

        public object getReplacement()
        {
            Chest replacement = new Chest(true);
            if (heldObject != null)
                replacement.items.Add(heldObject);

            return replacement;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            build(machines.Find(cmb => additionalSaveData["id"] == cmb.fullid));
        }

        

    }
}
