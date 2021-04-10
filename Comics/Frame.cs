using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using PlatoTK;
using PlatoTK.Content;
using PlatoTK.Objects;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace Comics
{
    public class Frame : PlatoFurniture<Furniture>
    {
        public static ISaveIndex SaveIndex { get; set; }

        public Frame()
        {
            
        }

        public Frame(int which, Vector2 tile)
        {

        }

        public Frame(int which, Vector2 tile, int initialRotations)
        {

        }
        public Frame(Vector2 tileLocation, int parentSheetIndex, int initialStack)
        {

        }
        public Frame(Vector2 tileLocation, int parentSheetIndex, bool isRecipe = false)
        {

        }
        public Frame(int parentSheetIndex, int initialStack, bool isRecipe = false, int price = -1, int quality = 0)
        {

        }
        public Frame(Vector2 tileLocation, int parentSheetIndex, string Givenname, bool canBeSetDown, bool canBeGrabbed, bool isHoedirt, bool isSpawnedObject)
        {

        }

        public override string DisplayName
        {
            get
            {
#if ANDROID
                return Base?.heldObject.Value?.netName.Value.Split(';')[0] ?? "Frame";
#else
                return Base?.heldObject.Value?.DisplayName ?? "Frame";
#endif
            }
            set
            {

            }
        }

        public override string getCategoryName()
        {
            return "Comic Book";
        }

        public override string getDescription()
        {
            return Base?.heldObject.Value?.getDescription() ?? "Empty";
        }

        public override Item getOne()
        {
            var one = GetNew(Base?.heldObject.Value);
            (one as Furniture).updateDrawPosition();
            return one;
        }


        public override bool canStackWith(ISalable other)
        {
            return false;
        }

        public override bool canBeRemoved(Farmer who)
        {
            return true;
        }
        public override bool clicked(Farmer who)
        {
            Game1.haltAfterCheck = false;
            return false;
        }

        public new void resetOnPlayerEntry(GameLocation environment, bool dropDown)
        {
            CheckParentSheetIndex();
            Link?.CallUnlinked<Furniture>((f) => f.resetOnPlayerEntry(environment, dropDown));
        }

        private void CheckParentSheetIndex()
        {
            if (SaveIndex.Index != Base?.parentSheetIndex.Value)
            {
                SaveIndex.ValidateIndex();
                Base?.parentSheetIndex.Set(SaveIndex.Index);
                Base?.defaultSourceRect.Set(new Rectangle(SaveIndex.Index * 16 % Furniture.furnitureTexture.Width, SaveIndex.Index * 16 / Furniture.furnitureTexture.Width * 16, 16, 16));
                Base?.sourceRect.Set(Base.defaultSourceRect.Value);
            }
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            CheckParentSheetIndex();

            if (Base?.heldObject?.Value?.Stack is int stack)
                Base.heldObject.Value.Stack = Base.Stack;

            Base?.heldObject?.Value?.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            if (!(Game1.currentLocation is DecoratableLocation))
                Base?.heldObject?.Value?.drawWhenHeld(spriteBatch, objectPosition, f);
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            CheckParentSheetIndex();

            Vector2 position = Link?.PrivateFields["drawPosition"] is NetVector2 vector ? vector.Get() : Vector2.Zero;

            if (position == Vector2.Zero)
                Base?.updateDrawPosition();

            Base?.heldObject?.Value?.draw(spriteBatch, (int)position.X, (int)position.Y, alpha);
        }

        public override void OnConstruction(IPlatoHelper helper, object linkedObject)
        {
            base.OnConstruction(helper, linkedObject);
            SaveIndex.ValidateIndex();
            CheckParentSheetIndex();

#if ANDROID
            if (Base?.heldObject.Value is StardewValley.Object obj && obj.netName.Value.Split(';') is string[] split && split.Length > 1)
                Base?.heldObject.Set(ComicBook.GetNew(split[1]));
            else
                Base?.heldObject.Set(ComicBook.GetNew("216384")); ;
#else
            if (Base?.heldObject.Value is StardewValley.Object obj && obj.modDataForSerialization.TryGetValue("Id", out string id))
                Base?.heldObject.Set(ComicBook.GetNew(id));
            else
                Base?.heldObject.Set(ComicBook.GetNew("216384"));
#endif

            Base?.updateDrawPosition();
        }


        public static Item GetNew(StardewValley.Object obj)
        {
            SaveIndex.ValidateIndex();
            var newFrame = new Furniture(SaveIndex.Index, Vector2.Zero);

            PlatoObject<Furniture>.SetIdentifier(newFrame, typeof(Frame));

            newFrame.heldObject.Value = (StardewValley.Object)obj?.getOne();

            return newFrame;
        }
    }
}
