using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System.Linq;
using System.Xml.Serialization;

namespace Portraiture2
{
    [XmlType("Mods_platonymous_SeedBag")]

    public class SeedBagTool : GenericTool
    {
        internal static Texture2D Texture;

        public bool InUse = false;
        public static Texture2D AttTexture;
        public static Texture2D Att2Texture;

        const int attSlots = 2;

        internal static void LoadTextures(IModHelper modHelper)
        {
            if (Texture == null)
            {
                Texture = modHelper.ModContent.Load<Texture2D>("Assets/seedbag.png");
                AttTexture = modHelper.ModContent.Load<Texture2D>("Assets/seedattachment.png");
                Att2Texture = modHelper.ModContent.Load<Texture2D>("Assets/fertilizerattachment.png");
            }
        }


        public SeedBagTool()
            : base()
        {
            this.Category = StardewValley.Object.toolCategory;
            this.Name = "Seed Bag";
            this.ItemId = MeleeWeapon.scytheId;

            this.Stack = 1;
            attachments.SetCount(attSlots);
            numAttachmentSlots.Value = attSlots;
            InstantUse = false;
            UpgradeLevel = 4;
            description = GetDescriptor(this);
        }

        protected override Item GetOneNew()
        {
            var tool = new SeedBagTool();
            tool.attachments.SetCount(attachments.Count);

            for (int i = 0; i < attachments.Count; i++)
            {
                tool.attachments[i] = attachments[i];
            }

            return tool;
        }

        protected override void GetOneCopyFrom(Item source)
        {
        }

        public override string DisplayName { get => SeedBagMod.i18n.Get("Name"); }

        protected override string loadDisplayName()
        {
            return SeedBagMod.i18n.Get("Name");
        }
        protected override string loadDescription()
        {
            return GetDescriptor(this);
        }

        public override bool canBeTrashed()
        {
            return true;
        }

        public override int salePrice(bool ignoreProfitMargins = false)
        {
            return SeedBagMod.config.Price;
        }

        public override void SetSpriteIndex(int spriteIndex)
        {
        }

        public override bool canThisBeAttached(StardewValley.Object o)
        {
            bool cba = (o == null || ((o.Category == -74 || o.Category == -19) && !(o is Furniture)));
            return cba;
        }

        public override StardewValley.Object attach(StardewValley.Object o)
        {
            StardewValley.Object priorAttachement = null;

            if (o != null && o.Category == -74 && attachments[0] != null)
                priorAttachement = new StardewValley.Object(attachments[0].ItemId, attachments[0].Stack);

            if (o != null && o.Category == -19 && attachments[1] != null)
                priorAttachement = new StardewValley.Object(attachments[1].ItemId, attachments[1].Stack);

            if (o == null)
            {
                if (attachments[0] != null)
                {
                    priorAttachement = new StardewValley.Object(attachments[0].ItemId, attachments[0].Stack);
                    attachments[0] = null;
                }
                else if (attachments[1] != null)
                {
                    priorAttachement = new StardewValley.Object(attachments[1].ItemId, attachments[1].Stack);
                    attachments[1] = null;
                }

                Game1.playSound("dwop");
                description = GetDescriptor(this);
                return priorAttachement;
            }

            if (canThisBeAttached(o))
            {
                if (o.Category == -74)
                    attachments[0] = o;

                if (o.Category == -19)
                    attachments[1] = o;

                Game1.playSound("button1");
                description = GetDescriptor(this);
                return priorAttachement;
            }
            else
            {
                return o;
            }
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            float num1 = 0.0f;
            if (MeleeWeapon.defenseCooldown > 0)
                num1 = MeleeWeapon.defenseCooldown / 1500f;
            float num2 = SeedBagMod._helper.Reflection.GetField<float>(typeof(MeleeWeapon), "addedSwordScale").GetValue();
            if (!drawShadow)
                num2 = 0;
            spriteBatch.Draw(Texture, location + new Vector2(Game1.tileSize / 2, Game1.tileSize / 2), new Rectangle(0, 0, 16, 16), Color.White * transparency, 0.0f, new Vector2(8f, 8f), Game1.pixelZoom * (scaleSize + num2), SpriteEffects.None, layerDepth);
            if (num1 <= 0.0 || drawShadow)
                return;
            spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X, (int)location.Y + (Game1.tileSize - (int)(num1 * (double)Game1.tileSize)), Game1.tileSize, (int)(num1 * (double)Game1.tileSize)), Color.Red * 0.66f);
        }


        internal float GetSquaredDistance(Vector2 point1, Vector2 point2)
        {
            float a = (point1.X - point2.X);
            float b = (point1.Y - point2.Y);
            return (a * a) + (b * b);
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, StardewValley.Farmer who)
        {
            if (attachments.Count == 0 || (attachments[0] == null && attachments[1] == null))
            {
                Game1.showRedMessage(SeedBagMod._helper.Translation.Get("Out_Of_Seeds"));
                return;
            }

            who.Stamina -= (float)(2 * power) - (float)who.FarmingLevel * 0.1f;
            power = who.toolPower.Value;
            who.stopJittering();
            Game1.playSound("leafrustle");
            Vector2 vector = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
            var list = tilesAffected(vector, power, who).OrderBy(v => GetSquaredDistance(who.Tile, v));

            foreach (Vector2 current in list)
            {
                if (location.terrainFeatures.ContainsKey(current) && location.terrainFeatures[current] is HoeDirt hd && hd.crop == null && !location.objects.ContainsKey(current))
                {
                    if (attachments[1] != null && string.IsNullOrEmpty(hd.fertilizer.Value))
                    {
                        if (hd.plant(attachments[1].ItemId, who, true))
                        {
                            attachments[1].Stack--;
                            if (attachments[1].Stack == 0)
                            {
                                attachments[1] = null;
                                Game1.showRedMessage(SeedBagMod._helper.Translation.Get("Out_Of_Fertilizer"));
                                break;
                            }
                        }
                    }

                    if (attachments[0] != null)
                    {
                        if (hd.plant(attachments[0].ItemId, who, false))
                        {
                            attachments[0].Stack--;
                            if (attachments[0].Stack == 0)
                            {
                                attachments[0] = null;
                                Game1.showRedMessage(SeedBagMod._helper.Translation.Get("Out_Of_Seeds"));
                                break;
                            }
                        }
                    }
                }
            }
        }

        internal static string GetDescriptor(Tool tool)
        {
            if (tool.attachments.Count > 0)
            {
                if (tool.attachments[0] != null)
                {
                    return tool.attachments[0].name;
                }

                if (tool.attachments[1] != null)
                {
                    return tool.attachments[1].name;
                }
            }

            return SeedBagMod.i18n.Get("Empty");
        }

        public override string getDescription()
        {
            return GetDescriptor(this);
        }

        public override void drawAttachments(SpriteBatch b, int x, int y)
        {
            int offset = 65;
            b.Draw(AttTexture, new Vector2(x, y), null, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
            b.Draw(Att2Texture, new Vector2(x, y + offset), null, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);

            if (attachments.Count() > 0)
            {
                if (attachments[0] is StardewValley.Object)
                    attachments[0].drawInMenu(b, new Vector2(x, y), 1f);

                if (attachments[1] is StardewValley.Object)
                    attachments[1].drawInMenu(b, new Vector2(x, y + offset), 1f);
            }

        }

        public void Draw(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f, Rectangle sourceRect, int type, bool isOnSpecial)
        {
        }
    }
}
