using Artista.Artpieces;
using Artista.Furniture;
using Artista.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Artista.Objects
{

    [XmlType("Mods_platonymous_Artista_Easel")]
    public class Easel : StardewValley.Object
    {

        internal Artpiece Art {
            get
            {
                if (heldObject.Value is PaintingFurniture pf)
                {
                    pf.Restore();
                    return pf.Art;
                }

                return null;
            }

            set
            {
                if(value != null)
                    heldObject.Value = new PaintingFurniture(value);
                else
                    heldObject.Value = null;
            }
        }

        public SavedArtpiece SavedArtpiece { get; set; }

        bool Hover { get; set; } = false;

        private Texture2D Texture { get; set; }

        public Easel(Artpiece art)
            : base(Vector2.Zero,"Platonymous.Artista.Easel",false)
        {
            SetArt(art);
            TileLocation = Vector2.Zero;
            Init();
        }
        public Easel()
            : base(Vector2.Zero, "Platonymous.Artista.Easel", false)
        {
            Restore();
            Init();
        }

        public void SetArt(Artpiece art)
        {
            SavedArtpiece = null;
            if(modData.ContainsKey("SavedArtpiece"))
            modData.Remove("SavedArtpiece");

            Art = art;
        }

        private int GetOffesetX()
        {
            var possibleArt = GetPossibleArt(false);

            if (possibleArt == null && !Hover)
                return 0;

            if(possibleArt == null && !ArtistaMod.Config.FreeCanvas)
                return 0;

            if(possibleArt == null) 
                return 16;

            if(possibleArt.TileHeight == possibleArt.TileWidth)
                return 32;

            return 16;
        }

        public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
        {
            if (Art != null)
                return false;

            if (probe && dropInItem is PaintingFurniture)
                return true;

            if(!probe && dropInItem is PaintingFurniture p)
            {
                SetArt(p.Art);
                return true;
            }

            return base.performObjectDropInAction(dropInItem, probe, who);
        }

        private Artpiece GetPossibleArt(bool regardless)
        {
            if (Art != null)
                return Art;

            if (!regardless && !Hover)
                return null;

            if (Game1.player.CurrentItem is PaintingFurniture p)
                return p.Art;

            if(Game1.player.Items.FirstOrDefault(i => i is PaintingFurniture) is PaintingFurniture i)
                return i.Art;

            return null;
        }

        private bool TryAddAndRemoveArt(Artpiece art)
        {
            if (Art == null && GetPossibleArt(true) is Artpiece)
            {
                if (Game1.player.Items.FirstOrDefault(i => i is PaintingFurniture p && p.Art == art) is PaintingFurniture p)
                {
                    SetArt(p.Art);
                    Game1.player.removeItemFromInventory(p);
                    return true;
                }
            }

            return false;
        }

        private ArtPartsRectangle GetArtRectangle(Rectangle destinationRectangle)
        {
            Artpiece possibleArt = GetPossibleArt(false);

            if (possibleArt == null && !Hover)
                return null;

            if(possibleArt == null && !ArtistaMod.Config.FreeCanvas)
                return null;

            int w = 8;
            int h = 16;
            int x = 4;
            int y = 2;

            if (possibleArt != null && possibleArt.TileHeight == possibleArt.TileWidth) {
                x = 2;
                y = 6;
                w = 12;
                h = w;
            }
            float scale = destinationRectangle.Width / 16f;
            Vector2 vecmid = new Vector2(x * scale,y  * scale);
            Vector2 vectop = new Vector2(x * scale, (y) * scale);
            Vector2 vecbottom = new Vector2(x * scale, (y + h - 1) * scale);

            var main = new Rectangle((int)(destinationRectangle.X + vecmid.X), (int)(destinationRectangle.Y + vecmid.Y), (int)(w * scale), (int)(h * scale));
            var top = new Rectangle((int)(destinationRectangle.X + vectop.X), (int)(destinationRectangle.Y + vectop.Y), (int)(w * scale), (int)(2 * scale));
            var bottom = new Rectangle((int)(destinationRectangle.X + vecbottom.X), (int)(destinationRectangle.Y + vecbottom.Y), (int)(w * scale), (int)(1 * scale));

            return new ArtPartsRectangle() { Top = top, Bottom = bottom, Main = main };
        }

        public override bool performToolAction(Tool t)
        {
            GameLocation location = Location;

            if (t is Pickaxe)
            {
                location.Objects.Remove(TileLocation);
                Game1.createItemDebris(this, tileLocation.Value * 64f, (Game1.player.FacingDirection + 2) % 4);
            }
            return false;
        }

        public override bool canBeDropped()
        {
            return base.canBeDropped();
        }
        public override bool canStackWith(ISalable other)
        {
            return false;
        }
        public void Init()
        {
            Texture = ArtistaMod.Singleton.Helper.ModContent.Load<Texture2D>("Assets/easel.png");
            bigCraftable.Value = true;
            canBeSetDown.Value = true;
            setOutdoors.Value = true;
            setIndoors.Value = true;
            isRecipe.Value = false;
            category.Value = -8;
            name = "Easel";
            type.Value = "Crafting";
            parentSheetIndex.Value = -1;
            boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
        }

        protected override string loadDisplayName()
        {
            return "Easel";
        }

        public override bool canBePlacedHere(GameLocation l, Vector2 tile, CollisionMask collisionMask = CollisionMask.All, bool showError = false)
        {
            return l.CanItemBePlacedHere(tile, itemIsPassable: false, collisionMask);
        }
        public override bool performDropDownAction(Farmer who)
        {
            return false;
        }

        public override void dropItem(GameLocation location, Vector2 origin, Vector2 destination)
        {
            base.dropItem(location, origin, destination);
        }

        public override string Name { get => "Easel"; set => base.Name = value; }

        public override string getDescription()
        {
            return "Paint!";
        }

        public void Save()
        {
            SavedArtpiece = Art?.Save();
        }


        public void Restore()
        {
            if (!Context.IsMainPlayer)
                return;

            if (Art == null && SavedArtpiece == null && modData.ContainsKey("SavedArtpiece"))
            {
                if (string.IsNullOrEmpty(modData["SavedArtpiece"]))
                {
                    SetArt(null);
                }
                else
                {
                    SetArt(new Painting(SavedArtpiece.FromJson(modData["SavedArtpiece"])));
                }

                modData.Remove("SavedArtpiece");
            }
            else if (Art == null && SavedArtpiece != null)
            {
                SetArt(new Painting(SavedArtpiece));
                SavedArtpiece = null;
            }
        }

        public override void hoverAction()
        {
            Hover = true;
            base.hoverAction();
        }

        public override bool checkForAction(StardewValley.Farmer who, bool justCheckingForActivity = false)
        {
            Artpiece possibleArt = GetPossibleArt(true);

            if (possibleArt == null && !ArtistaMod.Config.FreeCanvas)
                return false;

            if (justCheckingForActivity)
                return true;

            if (possibleArt != null)
            {
                if (Art == null && TryAddAndRemoveArt(possibleArt))
                    Game1.activeClickableMenu = new PaintMenu(Art, this, ArtistaMod.Singleton.Helper, ArtistaMod.Singleton.Monitor);
                else if (Art != null)
                    Game1.activeClickableMenu = new PaintMenu(Art, this, ArtistaMod.Singleton.Helper, ArtistaMod.Singleton.Monitor);

                return false;
            }
            else if (ArtistaMod.Config.FreeCanvas)
                Game1.activeClickableMenu = new SelectCanvasMenu(this, ArtistaMod.Singleton.Helper, ArtistaMod.Singleton.Monitor);

            return true;
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            Restore();
            Vector2 vector = Vector2.Zero;
            vector *= 4f;
            Vector2 vector2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
            Microsoft.Xna.Framework.Rectangle destinationRectangle = new Microsoft.Xna.Framework.Rectangle((int)(vector2.X - vector.X / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(vector2.Y - vector.Y / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + vector.X), (int)(128f + vector.Y / 2f));
            float num = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
            spriteBatch.Draw(Texture, destinationRectangle, new Rectangle(GetOffesetX(),0,16,32), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, num);

            var artRect = GetArtRectangle(destinationRectangle);
            if (artRect != null)
            {
                if(ArtistaMod.White == null)
                {
                    ArtistaMod.White = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
                    Color[] white = new Color[1] { Color.White };
                    ArtistaMod.White.SetData(white);
                }
                Artpiece possibleArt = GetPossibleArt(false);

                if (possibleArt != null)
                {
                    Texture2D tex = possibleArt.GetTexture();
                    spriteBatch.Draw(tex, artRect.Main, new Rectangle(0, 0, tex.Width, tex.Height), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, num + 0.000002f);
                }
                spriteBatch.Draw(ArtistaMod.White, artRect.Top, new Rectangle(0, 0, 1, 1), Color.White * alpha * 0.3f, 0f, Vector2.Zero, SpriteEffects.None, num + 0.000003f);
                spriteBatch.Draw(ArtistaMod.White, artRect.Bottom, new Rectangle(0, 0, 1, 1), Color.Black * alpha * 0.1f, 0f, Vector2.Zero, SpriteEffects.None, num + 0.000004f);
            }


            Hover = false;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            Restore();
            spriteBatch.Draw(Texture, location + new Vector2(32f, 32f), new Rectangle(0, 0, 16, 32), color * transparency, 0f, new Vector2(8f, 16f), 4f * (((double)scaleSize < 0.2) ? scaleSize : (scaleSize / 2f)), SpriteEffects.None, layerDepth);
        }

        protected override Item GetOneNew()
        {
            return new Easel(Art);
        }

        protected override void GetOneCopyFrom(Item source)
        {
            return;
        }
    }
}
