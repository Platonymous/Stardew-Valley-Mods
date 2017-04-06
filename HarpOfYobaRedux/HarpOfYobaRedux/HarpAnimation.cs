using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            String anim = "308 99 98 98 99 100 100";
            string[] split = anim.Split(' ');

            int int32 = Convert.ToInt32(split[0]);
            bool flip = false;
            bool flag = true;
            List<FarmerSprite.AnimationFrame> animation = new List<FarmerSprite.AnimationFrame>();
            for (int index = 1; index < split.Length; ++index)
            {
                animation.Add(new FarmerSprite.AnimationFrame(Convert.ToInt32(split[index]), int32, false, flip, (AnimatedSprite.endOfAnimationBehavior)null, false));
            }

            Game1.player.FarmerSprite.setCurrentAnimation(animation.ToArray());
            Game1.player.FarmerSprite.loopThisAnimation = flag;
            Game1.player.FarmerSprite.PauseForSingleAnimation = true;
        }

        public void stop()
        {
            Game1.player.FarmerSprite.PauseForSingleAnimation = false;
            Game1.player.completelyStopAnimatingOrDoingAction();
            Game1.player.canMove = true;
            
        }
    }
}
