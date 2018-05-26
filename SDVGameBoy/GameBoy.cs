using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.CustomElementHandler;
using StardewValley;

namespace SDVGameBoy
{
    class GameBoy : PySObject
    {
        public GBCartridge currentCartridge
        {
            get
            {
                return (GBCartridge)heldObject.Value;
            }
            set
            {
                heldObject.Value = value;
            }
        }
        private GBCartridge temp;
        public static Texture2D attTexture;

        public GameBoy()
        {

        }

        public GameBoy(CustomObjectData data)
            : base(data)
        {
        }

        public override string getDescription()
        {
            return currentCartridge != null ? currentCartridge.Name : "No Cartridge";
        }

        public override int attachmentSlots()
        {
            return 1;
        }

        public override bool canStackWith(Item other)
        {
            if ((other is GBCartridge c))
            {
                currentCartridge = (GBCartridge)c.getOne();
                return false;
            }
            return false;
        }

        public override void drawAttachments(SpriteBatch b, int x, int y)
        {
            Rectangle attachementSourceRectangle = new Rectangle(0, 0, 64, 64);
            b.Draw(attTexture, new Vector2(x, y), new Rectangle?(attachementSourceRectangle), Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);

            if (currentCartridge != null)
                    currentCartridge.drawInMenu(b, new Vector2(x, y), 1f);

        }

        private void startGB()
        {
            if (currentCartridge != null)
                Game1.currentMinigame = new GBMinigame(currentCartridge.rom);
        }

        public override bool performUseAction(GameLocation location)
        {
            Game1.player.CurrentToolIndex = 25;
            startGB();
            return false;
        }

        public override bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            return false;
        }

        public override bool canBeShipped()
        {
            return false;
        }

        public override bool canBeTrashed()
        {
            return true;
        }

        public override Item getOne()
        {
            return new GameBoy(data);
        }

        public override ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            CustomObjectData data = CustomObjectData.collection[additionalSaveData["id"]];
            return new GameBoy(CustomObjectData.collection[additionalSaveData["id"]]);
        }


    }
}
