using System;
using StardewValley;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace HarpOfYobaRedux
{
    class TeleportMagic : IMagic
    {
        private GameLocation lastLocation;
        private Vector2 lastPosition;
        private GameLocation targetLocation;
        private Vector2 targetPosition;


        public TeleportMagic()
        {

        }

        private void teleport()
        {
            Game1.changeMusicTrack("none");
            Game1.warpFarmer(targetLocation.name, (int)targetPosition.X, (int)targetPosition.Y, false);
            Game1.fadeToBlackAlpha = 0.99f;
            Game1.screenGlow = false;
            Game1.player.temporarilyInvincible = false;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.displayFarmer = true;
        }

        private void start()
        {
            for (int index = 0; index < 12; ++index)
            {
                Game1.player.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(354, (float)Game1.random.Next(25, 75), 6, 1, new Vector2((float)Game1.random.Next((int)Game1.player.position.X - Game1.tileSize * 4, (int)Game1.player.position.X + Game1.tileSize * 3), (float)Game1.random.Next((int)Game1.player.position.Y - Game1.tileSize * 4, (int)Game1.player.position.Y + Game1.tileSize * 3)), false, Game1.random.NextDouble() < 0.5));
            }

            Game1.playSound("wand");
            Game1.displayFarmer = false;
            Game1.player.Halt();
            Game1.player.faceDirection(2);
            Game1.player.freezePause = 1000;
            Game1.flashAlpha = 1f;

            new System.Drawing.Rectangle(Game1.player.GetBoundingBox().X, Game1.player.GetBoundingBox().Y, Game1.tileSize, Game1.tileSize).Inflate(Game1.tileSize * 3, Game1.tileSize * 3);
            int num1 = 0;
            for (int index = Game1.player.getTileX() + 8; index >= Game1.player.getTileX() - 8; --index)
            {
                List<TemporaryAnimatedSprite> temporarySprites = Game1.player.currentLocation.temporarySprites;
                TemporaryAnimatedSprite temporaryAnimatedSprite = new TemporaryAnimatedSprite(6, new Vector2((float)index, (float)Game1.player.getTileY()) * (float)Game1.tileSize, Microsoft.Xna.Framework.Color.White, 8, false, 50f, 0, -1, -1f, -1, 0);
                temporaryAnimatedSprite.layerDepth = 1f;
                int num2 = num1 * 25;
                temporaryAnimatedSprite.delayBeforeAnimationStart = num2;
                Vector2 vector2 = new Vector2(-0.25f, 0.0f);
                temporaryAnimatedSprite.motion = vector2;
                temporarySprites.Add(temporaryAnimatedSprite);
                ++num1;
            }
        }

        public void doMagic(bool playedToday)
        {
            if (!playedToday)
            {
                lastLocation = Game1.getLocationFromName("Town");
                lastPosition = new Vector2(53,24);
            }

            targetLocation = Game1.getLocationFromName(lastLocation.name);
            targetPosition = new Vector2(lastPosition.X, lastPosition.Y);
            lastLocation = Game1.currentLocation;
            lastPosition = new Vector2(Game1.player.getTileX(), Game1.player.getTileY());

            DelayedAction startAction = new DelayedAction(6000);
            startAction.behavior = new DelayedAction.delayedBehavior(start);

            DelayedAction teleportAction = new DelayedAction(7000);
            teleportAction.behavior = new DelayedAction.delayedBehavior(teleport);

            Game1.delayedActions.Add(startAction);
            Game1.delayedActions.Add(teleportAction);
        }
    }
}
