
using StardewValley;

namespace TheHarpOfYoba
{
    class BirthdayEvent : HarpEvents
    {

        private HarpOfYoba harp;
        private bool played_before;
        public NPC temp;
        public NPC lastBirthday;

        public BirthdayEvent()
        {



        }

        public override void beforePlaying(bool p, HarpOfYoba h)
        {
            this.harp = h;
            this.played_before = p;


            this.harp.playNewMusic();

            DelayedAction delayedAction1 = new DelayedAction(1000);
            delayedAction1.behavior = new DelayedAction.delayedBehavior(startPlaying);
            Game1.delayedActions.Add(delayedAction1);


            DelayedAction delayedAction2 = new DelayedAction(5000);
            delayedAction2.behavior = new DelayedAction.delayedBehavior(whilePlaying);
            Game1.delayedActions.Add(delayedAction2);



        }

        public override void startPlaying()
        {

            harp.animateHarp();

        }

        public override void whilePlaying()
        {

            GameLocation gl = Game1.currentLocation;

            foreach (NPC ch in gl.characters)
            {

                if (ch.isBirthday(Game1.currentSeason, Game1.dayOfMonth))
                {

                    if (this.lastBirthday != ch)
                    {
                        Game1.player.changeFriendship((int)(40.0 * 8 * 1.5), ch);

                        ch.doEmote(20, true);
                        this.temp = ch;
                        this.lastBirthday = ch;

                        if (ch.name == "Wizard")
                        {
                            HarpOfYobaMod.processIndicators[5] = true;

                        }
                    }

                    DelayedAction delayedActionT = new DelayedAction(1000);
                    delayedActionT.behavior = new DelayedAction.delayedBehavior(stopPlaying);
                    Game1.delayedActions.Add(delayedActionT);



                    break;
                }

            }

            DelayedAction delayedAction2 = new DelayedAction(4500);
            delayedAction2.behavior = new DelayedAction.delayedBehavior(stopPlaying);
            Game1.delayedActions.Add(delayedAction2);

            DelayedAction delayedAction = new DelayedAction(6000);
            delayedAction.behavior = new DelayedAction.delayedBehavior(afterPlaying);
            Game1.delayedActions.Add(delayedAction);
        }

        public override void stopPlaying()
        {
            
            harp.stopHarp();
            Game1.nextMusicTrack = "none";
            DelayedAction.playMusicAfterDelay(harp.oldMusic, 10000);
        }

        public override void afterPlaying()
        {

           

        }
    }
}
