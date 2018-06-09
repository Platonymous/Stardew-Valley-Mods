using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using SObject = StardewValley.Object;
using SFarmer = StardewValley.Farmer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using PyTK.Extensions;
using PyTK.CustomElementHandler;
using StardewValley.Objects;
using PyTK;

namespace CustomFarmingRedux
{
    public class CustomObject : SObject, ISyncableElement, ICustomObject
    {
        public PySync syncObject { get; set; }
        internal IModHelper Helper = CustomFarmingReduxMod._helper;
        internal IMonitor Monitor = CustomFarmingReduxMod._monitor;
        internal string folder = CustomFarmingReduxMod.folder;
        internal List<CustomMachineBlueprint> machines = CustomFarmingReduxMod.machines;

        private RecipeBlueprint blueprint;
        private CustomMachineBlueprint mBlueprint => blueprint.mBlueprint;
        internal Color color;
        private string _name;
        private string cname;
        internal Texture2D texture;
        private Rectangle tilesize = new Rectangle(0, 0, 16, 16);
        internal Rectangle sourceRectangle;
        private SObject input { get => heldObject.Value; set => heldObject.Value = value; }

        public CustomObject()
        {
            syncObject = new PySync(this);
            syncObject.init();
        }

        public CustomObject(int index, int stack, string name, SObject input, RecipeBlueprint blueprint)
            :base(Vector2.Zero, index, stack)
        {
            syncObject = new PySync(this);
            syncObject.init();
            build(name, input, blueprint);
        }

        public CustomObject(string name, SObject input, RecipeBlueprint blueprint)
        {
            build(name, input, blueprint);
        }

        private void calculatePrice()
        {
            int compPrice = (int)(PyUtils.calc(blueprint.price, new KeyValuePair<string, object>("input", input.Price), new KeyValuePair<string, object>("original", Price)));
            price.Value = compPrice;
        }

        private void build(string name, SObject input, RecipeBlueprint blueprint, bool forSync = false)
        {
            this.blueprint = blueprint;
            texture = blueprint.getTexture();
            if (blueprint.colored)
            {
                if (blueprint.color != null)
                    color = new Color(blueprint.color[0], blueprint.color[1], blueprint.color[2], blueprint.color[3]);
                else
                    color = getColor(input);
            }
            else
                color = Color.White;

            this.input = input;

            _name = name;
            cname = name;

            if (forSync)
                return;

            try
            {
                cname = (blueprint.prefix) ? input.name + " " + name : name;
                cname = (blueprint.suffix) ? cname + " " + input.name : cname;

                if (blueprint.insert)
                {
                    string[] namesplit = cname.Split(' ');
                    namesplit[blueprint.insertpos] += " " + input.name;
                    cname = String.Join(" ", namesplit);
                }
            }
            catch
            {

            }

            this.name = cname;
            displayName = cname;

            parentSheetIndex.Value = blueprint.index;

            stack.Value = blueprint.stack;
            quality.Value = (blueprint.quality == -1) ? input.Quality : blueprint.quality;

            sourceRectangle = Game1.getSourceRectForStandardTileSheet(texture, blueprint.tileindex, 16, 16);

            calculatePrice();

            if (syncObject == null)
            {
                syncObject = new PySync(this);
                syncObject.init();
            }
        }

        public void loadBaseObjectInformation(int parentSheetIndex)
        {
            string str;
            Game1.objectInformation.TryGetValue(parentSheetIndex, out str);
            try
            {
                if (str != null)
                {
                    string[] strArray1 = str.Split('/');
                    name = strArray1[0];
                    price.Value = Convert.ToInt32(strArray1[1]);
                    edibility.Value = Convert.ToInt32(strArray1[2]);
                    string[] strArray2 = strArray1[3].Split(' ');
                    type.Value = strArray2[0];
                    if (strArray2.Length > 1)
                        category.Value = Convert.ToInt32(strArray2[1]);
                }
            }
            catch
            {
            }

        }

        public static Color getColor(SObject input)
        {
            Texture2D texture = Game1.objectSpriteSheet;
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            int w = 16;
            int h = 16;
            Rectangle sourceRectangle = Game1.getSourceRectForStandardTileSheet(texture, input.ParentSheetIndex, w, h);
            Color[] data2 = new Color[w * h];

            int x2 = sourceRectangle.X;
            int y2 = sourceRectangle.Y;

            for (int x = x2; x < w + x2; x++)
                for (int y = y2; y < h + y2; y++)
                    data2[(y - y2) * w + (x - x2)] = data[y * texture.Width + x];

            List<Color> colors = data2.ToList();
            colors.RemoveAll(p => p == Color.White || p == Color.Black);

            int R = (int)colors.toList(c => (int)c.R).Average();
            int G = (int)colors.toList(c => (int)c.G).Average(); ;
            int B = (int)colors.toList(c => (int)c.B).Average(); ;

            int nR = R;
            int nG = G;
            int nB = B;

            if (R >= G && R >= B)
                nR = 255;

            if (G >= B && G >= R)
                nG = 255;

            if (B >= R && B >= G)
                nB = 255;

            return new Color(nR, nG, nB);
        }

        public override string getCategoryName()
        {
            return blueprint.category;
        }

        public override Color getCategoryColor()
        {
            return Color.Magenta;
        }

        public override Item getOne()
        {
            return new CustomObject(ParentSheetIndex, Stack, _name, input, blueprint);
        }

        public override string DisplayName { get => cname; set => base.DisplayName = value; }

        public override string Name => cname;

        public override string getDescription()
        {
            return Game1.parseText(blueprint.description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {

            spriteBatch.Draw(Game1.shadowTexture, location + new Vector2((Game1.tileSize / 2), (Game1.tileSize * 3 / 4)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White * 0.5f, 0.0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
            spriteBatch.Draw(texture, location + new Vector2((Game1.tileSize / 2), (Game1.tileSize / 2)), new Rectangle?(sourceRectangle), this.color * transparency, 0.0f, new Vector2(tilesize.Width / 2, tilesize.Height / 2), Game1.pixelZoom * (scaleSize < 0.2 ? scaleSize : scaleSize), SpriteEffects.None, layerDepth);

            if (drawStackNumber && maximumStackSize() > 1 && (scaleSize > 0.3 && Stack != int.MaxValue) && Stack > 1)
                Utility.drawTinyDigits(stack, spriteBatch, location + new Vector2((Game1.tileSize - Utility.getWidthOfTinyDigitString(stack, 3f * scaleSize)) + 3f * scaleSize, (float)(Game1.tileSize - 18.0 * scaleSize + 2.0)), 3f * scaleSize, 1f, Color.White);

            if (drawStackNumber && quality > 0)
            {
                float num = quality < 4 ? 0.0f : (float)((Math.Cos(Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1.0) * 0.0500000007450581);
                spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(12f, (Game1.tileSize - 12) + num), new Microsoft.Xna.Framework.Rectangle?(quality < 4 ? new Rectangle(338 + (quality - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8)), Color.White * transparency, 0.0f, new Vector2(4f, 4f), (float)(3.0 * scaleSize * (1.0 + num)), SpriteEffects.None, layerDepth);
            }

            if (System.Type.GetType("BetterArtisanGoodIcons.ArtisanGoodsManager, BetterArtisanGoodIcons") != null && input is SObject sobj && !sobj.bigCraftable.Value)
                spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(10f * scaleSize, 10f * scaleSize), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, input.ParentSheetIndex, 16, 16)), Color.White * transparency, 0.0f, new Vector2(4f, 4f), 1.5f * scaleSize, SpriteEffects.None, layerDepth);
            
        }


        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, SFarmer f)
        {
            spriteBatch.Draw(texture, objectPosition, sourceRectangle, color, 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, Math.Max(0.0f, (f.getStandingY() + 2) / 10000f));
        }

        public object getReplacement()
        {
            Chest r = new Chest(true);
            r.items.Add(heldObject);
            return r;
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("mid", $"{mBlueprint.folder}.{mBlueprint.file}.{mBlueprint.id}");
            data.Add("id", blueprint.id.ToString());
            data.Add("name", _name);
            data.Add("stack", stack.ToString());
            return data;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            rebuild(additionalSaveData, replacement, false);
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement, bool forSync)
        {
            SObject lastInput = (SObject)replacement;

            if (replacement is Chest c)
                lastInput = (SObject) c.items.ToList().First();

            CustomMachineBlueprint mBlueprint = machines.Find(cmb => additionalSaveData["mid"] == cmb.fullid);
            RecipeBlueprint blueprint = mBlueprint.production.Find(p => p.id.ToString() == additionalSaveData["id"]);
            try
            {
                build(additionalSaveData["name"], lastInput, blueprint, forSync);
            }
            catch
            {

            }
            stack.Value = int.Parse(additionalSaveData["stack"]);
        }

        public Dictionary<string, string> getSyncData()
        {
            return getAdditionalSaveData();
        }

        public void sync(Dictionary<string, string> syncData)
        {
            rebuild(syncData, heldObject, true);
        }

        public ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            SObject lastInput = (SObject)replacement;

            if (replacement is Chest c)
                lastInput = (SObject)c.items.ToList().First();

            CustomMachineBlueprint mBlueprint = machines.Find(cmb => additionalSaveData["mid"] == cmb.fullid);
            RecipeBlueprint blueprint = mBlueprint.production.Find(p => p.id.ToString() == additionalSaveData["id"]);
            build(additionalSaveData["name"], lastInput, blueprint);
            stack.Value = int.Parse(additionalSaveData["stack"]);
            return new CustomObject(blueprint.index, int.Parse(additionalSaveData["stack"]), additionalSaveData["name"], lastInput, blueprint);
        }
    }
}
