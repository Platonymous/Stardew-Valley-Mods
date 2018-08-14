using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using PyTK.Types;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TotSQ
{
    class Spider : NPC
    {
        internal static ScaledTexture2D Texture { get; set; }
        private int readyToJump = -1;
        private readonly NetVector2 facePosition = new NetVector2();

        private readonly NetEvent1Field<Vector2, NetVector2> jumpEvent = new NetEvent1Field<Vector2, NetVector2>();


        public Spider()
            : base(getSprite(), Vector2.Zero, 0, "Spider")
        {
            setup();
        }

        public Spider(Vector2 tileLocation)
            : base(getSprite(),tileLocation, 0 ,"Spider")
        {
            setup();
        }


        public void setup()
        {
        }

        public override void update(GameTime time, GameLocation location)
        {
            base.update(time, location);
            setAnimation(time);
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            return;
            if (!Utility.isOnScreen(this.Position, 128))
                return;
            SpriteBatch spriteBatch = b;
            //b.Draw(this.Sprite.Texture, this.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(48 + this.yJumpOffset)), new Rectangle?(this.Sprite.SourceRect), this.color.Value, this.rotation, new Vector2(8f, 16f), Math.Max(0.2f, (float)((NetFieldBase<float, NetFloat>)this.scale)) * 4f, this.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0.0f, this.drawOnTop ? 0.991f : (float)this.getStandingY() / 10000f));
        }

        public void setAnimation(GameTime time)
        {
            if (facingDirection.Value == 0)
                this.Sprite.Animate(time, 16, 15, 120f);
            else
                this.Sprite.Animate(time, 32, 15, 120f);

            if (facingDirection.Value != 0 && this.readyToJump != -1)
                this.Sprite.Animate(time, 0, 15, 120f);

            this.Speed = 4;
        }
      

        public override void animateInFacingDirection(GameTime time)
        {
            base.animateInFacingDirection(time);
            setAnimation(time);
        }

        public static AnimatedSprite getSprite()
        {
            AnimatedSprite sprite = new AnimatedSprite(@"Characters/Monsters/Spider");
            sprite.SpriteWidth = 16;
            sprite.SpriteHeight = 16;
            sprite.framesPerAnimation = 16;
            sprite.interval = 60f;

            TotSQMod._helper.Reflection.GetField<Texture2D>(sprite, "spriteTexture").SetValue(Texture);

            return sprite;
        }

    }
}
