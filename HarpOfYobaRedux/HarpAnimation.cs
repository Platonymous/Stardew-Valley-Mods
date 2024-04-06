using StardewValley;
using System.Collections.Generic;
using static StardewValley.FarmerSprite;

namespace HarpOfYobaRedux
{
    internal class HarpAnimation : IInstrumentAnimation
    {
        public HarpAnimation()
        {

        }

        public void preAnimation()
        {
            Game1.player.canMove = false;
            Game1.playSound("dwop");
            Game1.player.faceDirection(2);
            Game1.player.showFrame(98);
        }

        public void animate()
        {
            List<int> frames = new List<int> { 99, 98, 98, 99, 100, 100 };
            List<AnimationFrame> animation = new List<AnimationFrame>();

            foreach(int frame in frames)
                animation.Add(new AnimationFrame(frame, 308, false, false));

            Game1.player.FarmerSprite.setCurrentAnimation(animation.ToArray());
            Game1.player.FarmerSprite.loopThisAnimation = true;
            Game1.player.FarmerSprite.PauseForSingleAnimation = true;
            Game1.player.FarmerSprite.currentAnimationIndex = 1;
        }

        public void stop()
        {
            Game1.player.FarmerSprite.PauseForSingleAnimation = false;
            Game1.player.completelyStopAnimatingOrDoingAction();
            Game1.player.canMove = true;
        }
    }
}
