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

namespace CustomFarmingRedux
{
    public class CustomObject : SObject, ISaveElement
    {
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
        private SObject input { get => heldObject; set => heldObject = value; }

        public CustomObject()
        {

        }

        public CustomObject(string name, SObject input, RecipeBlueprint blueprint)
        {
            build(name, input, blueprint);
        }

        private void build(string name, SObject input, RecipeBlueprint blueprint)
        {
            loadBaseObjectInformation(blueprint.index);
            _name = name;
            cname = (blueprint.prefix) ? input.name + " " + name : name;
            cname = (blueprint.suffix) ? cname + " " + input.name : cname;
            if (blueprint.insert)
            {
                string[] namesplit = cname.Split(' ');
                namesplit[blueprint.insertpos] += " " + input.name;
                cname = String.Join(" ", namesplit);
            }
            this.name = cname;
            displayName = cname;
            this.blueprint = blueprint;
            parentSheetIndex = blueprint.index;
            this.input = input;
            stack = blueprint.stack;
            quality = (blueprint.quality == -1) ? input.quality : blueprint.quality;
            texture = blueprint.getTexture();
            sourceRectangle = Game1.getSourceRectForStandardTileSheet(texture, blueprint.tileindex, 16, 16);
            if (blueprint.colored)
            {
                if (blueprint.color != null)
                    color = new Color(blueprint.color[0], blueprint.color[1], blueprint.color[2], blueprint.color[3]);
                else
                    color = getColor(input);
            }
            else
            {
                color = Color.White;
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
                    price = Convert.ToInt32(strArray1[1]);
                    edibility = Convert.ToInt32(strArray1[2]);
                    string[] strArray2 = strArray1[3].Split(' ');
                    type = strArray2[0];
                    if (strArray2.Length > 1)
                        category = Convert.ToInt32(strArray2[1]);
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
            Rectangle sourceRectangle = Game1.getSourceRectForStandardTileSheet(texture, input.parentSheetIndex, 16, 16);
            Dictionary<Color, int> colors = new Dictionary<Color, int>();

            for (int x = sourceRectangle.X + 6; x < (sourceRectangle.X + sourceRectangle.Width - 6); x++)
                for (int y = sourceRectangle.Y + 6; y < (sourceRectangle.Y + sourceRectangle.Height - 4); y++)
                    if (data[x * texture.Height + y].R != data[x * texture.Height + y].G && data[x * texture.Height + y].G != data[x * texture.Height + y].B && data[x * texture.Height + y].A == 255)
                            colors.AddOrReplace(data[x * texture.Height + y], 0);
            
            for (int i = 0; i < colors.Keys.Count; i++)
                for (int j = 0; j < colors.Keys.Count; j++)
                    colors[colors.Keys.ElementAt(i)] += colors.Keys.ElementAt(i).getDistanceTo(colors.Keys.ElementAt(j));
                    
            List<int> distances = colors.toList(k => k.Value);
            int mindist = distances.Min();
            return colors.Find(k => k.Value == mindist).Key.setLight(120).setSaturation(150);
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
            return new CustomObject(_name, input, blueprint);
        }

        public override string DisplayName { get => cname; set => base.DisplayName = value; }

        public override string Name => cname;

        public override string getDescription()
        {
            return Game1.parseText(blueprint.description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {

            spriteBatch.Draw(Game1.shadowTexture, location + new Vector2((Game1.tileSize / 2), (Game1.tileSize * 3 / 4)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White * 0.5f, 0.0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
            spriteBatch.Draw(texture, location + new Vector2((Game1.tileSize / 2), (Game1.tileSize / 2)), new Rectangle?(sourceRectangle), color * transparency, 0.0f, new Vector2(tilesize.Width / 2, tilesize.Height / 2), Game1.pixelZoom * (scaleSize < 0.2 ? scaleSize : scaleSize), SpriteEffects.None, layerDepth);

            if (drawStackNumber && maximumStackSize() > 1 && (scaleSize > 0.3 && Stack != int.MaxValue) && Stack > 1)
                Utility.drawTinyDigits(stack, spriteBatch, location + new Vector2((Game1.tileSize - Utility.getWidthOfTinyDigitString(stack, 3f * scaleSize)) + 3f * scaleSize, (float)(Game1.tileSize - 18.0 * scaleSize + 2.0)), 3f * scaleSize, 1f, Color.White);

            if (drawStackNumber && quality > 0)
            {
                float num = quality < 4 ? 0.0f : (float)((Math.Cos(Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1.0) * 0.0500000007450581);
                spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(12f, (Game1.tileSize - 12) + num), new Microsoft.Xna.Framework.Rectangle?(quality < 4 ? new Rectangle(338 + (quality - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8)), Color.White * transparency, 0.0f, new Vector2(4f, 4f), (float)(3.0 * scaleSize * (1.0 + num)), SpriteEffects.None, layerDepth);
            }
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
            CustomMachineBlueprint mBlueprint = machines.Find(cmb => additionalSaveData["mid"] == cmb.fullid);
            RecipeBlueprint blueprint = mBlueprint.production.Find(p => p.id.ToString() == additionalSaveData["id"]);
            build(additionalSaveData["name"], (SObject) (replacement as Chest).items[0], blueprint);
            stack = int.Parse(additionalSaveData["stack"]);
        }
    }
}
