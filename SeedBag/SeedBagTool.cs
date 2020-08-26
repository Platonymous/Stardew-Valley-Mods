using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using PlatoTK;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Linq;

namespace SeedBag
{
    public class SeedBagTool : PlatoTK.Objects.PlatoTool<StardewValley.Tools.GenericTool>
    {
        public static int TileIndex { get; set; }
        public bool InUse = false;
        public static Texture2D Texture;
        public static Texture2D AttTexture;
        public static Texture2D Att2Texture;

        const int attSlots = 2;

        internal static void LoadTextures(IPlatoHelper helper)
        {
            if (Texture == null)
            {
                Texture = helper.ModHelper.Content.Load<Texture2D>(@"Assets/seedbag.png");
                AttTexture = helper.ModHelper.Content.Load<Texture2D>(@"Assets/seedattachment.png");
                Att2Texture = helper.ModHelper.Content.Load<Texture2D>(@"Assets/fertilizerattachment.png");
            }
        }

        public override string DisplayName {
            get => Helper.ModHelper.Translation.Get("Name");
            set {

            }
        }

        public override string Name { get
            {
                return DisplayName;
            }
            set
            {

            }
        }

        public SeedBagTool()
            : base()
        {

        }

        public override bool CanLinkWith(object linkedObject)
        {
            return linkedObject is StardewValley.Tools.GenericTool obj && obj.netName.Get().Contains("IsSeedBagTool");
        }

        public override NetString GetDataLink(object linkedObject)
        {
            if (linkedObject is Tool t)
                return t.netName;
            return null;
        }

        public override Item getOne()
        {
            if(Base.attachments.Length > 1)
                return GetNew(Helper, Base.attachments[0], Base.attachments[1]);

            return GetNew(Helper);
        }
        public override bool canBeTrashed()
        {
            return true;
        }

        public override bool actionWhenPurchased()
        {
            Game1.playSound("parry");
            Game1.exitActiveMenu();
            var newBag = GetNew(Helper);
            Helper.SetTickDelayedUpdateAction(5, () =>
            {
                if (!(Game1.player.hasItemInInventoryNamed(newBag.Name) || Game1.player.hasItemInInventoryNamed(DisplayName)))
                {
                    Game1.player.holdUpItemThenMessage(newBag, true);
                    Game1.player.addItemByMenuIfNecessary(newBag);
                }
            });
            return true;
        }

        public override void setNewTileIndexForUpgradeLevel()
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

            if (o != null && o.Category == -74 && Base.attachments[0] != null)
                priorAttachement = new StardewValley.Object(Vector2.Zero, Base.attachments[0].ParentSheetIndex, Base.attachments[0].Stack);

            if (o != null && o.Category == -19 && Base.attachments[1] != null)
                priorAttachement = new StardewValley.Object(Vector2.Zero, Base.attachments[1].ParentSheetIndex, Base.attachments[1].Stack);

            if (o == null)
            {
                if (Base.attachments[0] != null)
                {
                    priorAttachement = new StardewValley.Object(Vector2.Zero, Base.attachments[0].ParentSheetIndex, Base.attachments[0].Stack);
                    Base.attachments[0] = null;
                }
                else if (Base.attachments[1] != null)
                {
                    priorAttachement = new StardewValley.Object(Vector2.Zero, Base.attachments[1].ParentSheetIndex, Base.attachments[1].Stack);
                    Base.attachments[1] = null;
                }

                Game1.playSound("dwop");
                Base.description = GetDescriptor(Helper, Base);
                return priorAttachement;
            }

            if (canThisBeAttached(o))
            {
                if (o.Category == -74)
                    Base.attachments[0] = o;

                if (o.Category == -19)
                    Base.attachments[1] = o;

                Game1.playSound("button1");
                Base.description = GetDescriptor(Helper, Base);
                return priorAttachement;
            }
            else
            {
                return o;
            }
        }

        internal float GetSquaredDistance(Vector2 point1, Vector2 point2)
        {
            float a = (point1.X - point2.X);
            float b = (point1.Y - point2.Y);
            return (a * a) + (b * b);
        }

        public override void DoFunction(GameLocation location, int x, int y, int power, StardewValley.Farmer who)
        {
            if (Base == null)
                return;

            if (Base.attachments.Count == 0 || (Base.attachments[0] == null && Base.attachments[1] == null))
            {
                Game1.showRedMessage(Helper.ModHelper.Translation.Get("Out_Of_Seeds"));
                return;
            }

            who.Stamina -= (float)(2 * power) - (float)who.FarmingLevel * 0.1f;
            power = who.toolPower;
            who.stopJittering();
            Game1.playSound("leafrustle");
            Vector2 vector = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
            var list = tilesAffected(vector, power, who).OrderBy(v => GetSquaredDistance(who.getTileLocation(),v));

            foreach (Vector2 current in list)
            {
                if (location.terrainFeatures.ContainsKey(current) && location.terrainFeatures[current] is HoeDirt hd && hd.crop == null && !location.objects.ContainsKey(current))
                {
                    if (Base.attachments[1] != null && hd.fertilizer.Value <= 0)
                    {
                        if (hd.plant(Base.attachments[1].ParentSheetIndex, (int)current.X, (int)current.Y, who, true, location))
                        {
                            Base.attachments[1].Stack--;
                            if (Base.attachments[1].Stack == 0)
                            {
                                Base.attachments[1] = null;
                                Game1.showRedMessage(Helper.ModHelper.Translation.Get("Out_Of_Fertilizer"));
                                break;
                            }
                        }
                    }

                    if (Base.attachments[0] != null)
                    {
                        if (hd.plant(Base.attachments[0].ParentSheetIndex, (int)current.X, (int)current.Y, who, false, location))
                        {
                            Base.attachments[0].Stack--;
                            if (Base.attachments[0].Stack == 0)
                            {
                                Base.attachments[0] = null;
                                Game1.showRedMessage(Helper.ModHelper.Translation.Get("Out_Of_Seeds"));
                                break;
                            }
                        }
                    }
                }
            }
        }
       

        public static Tool GetNew(IPlatoHelper helper, StardewValley.Object seed = null, StardewValley.Object fertilizer = null)
        {
            var newTool = new GenericTool();
            newTool.attachments.SetCount(2);
            newTool.numAttachmentSlots.Value = attSlots;
            if (seed != null)
                newTool.attachments[0] = seed;

            if(fertilizer != null)
                newTool.attachments[1] = fertilizer;

            newTool.initialParentTileIndex.Value = TileIndex;
            newTool.currentParentTileIndex.Value = TileIndex;
            newTool.instantUse.Value = false;
            newTool.indexOfMenuItemView.Value = TileIndex;
            newTool.upgradeLevel.Value = 4;
            newTool.description = GetDescriptor(helper,newTool);
            newTool.netName.Set("Plato:IsSeedBagTool=true");
            return newTool;
        }

        internal static string GetDescriptor(IPlatoHelper helper, Tool tool)
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

            return helper.ModHelper.Translation.Get("Empty");
        }

        public override string getDescription()
        {
            return GetDescriptor(Helper, Base);
        }

        protected override string loadDescription()
        {
            return getDescription();
        }

        protected override string loadDisplayName()
        {
            return DisplayName;
        }

        public override void drawAttachments(SpriteBatch b, int x, int y)
        {
            int offset = 65;
            b.Draw(AttTexture, new Vector2(x, y), null, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
            b.Draw(Att2Texture, new Vector2(x, y + offset), null, Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);

            if (Base == null)
                return;

            if (Base.attachments.Count() > 0)
            {
                if (Base.attachments[0] is StardewValley.Object)
                    Base.attachments[0].drawInMenu(b, new Vector2(x, y), 1f);

                if (Base.attachments[1] is StardewValley.Object)
                    Base.attachments[1].drawInMenu(b, new Vector2(x, y + offset), 1f);
            }

        }
        
    }
}
