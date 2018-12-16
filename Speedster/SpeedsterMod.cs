using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Objects;

using StardewValley.Menus;

namespace Speedster
{
    public class SpeedsterMod : Mod
    {

        internal static IModHelper ModHelper;
        internal static SConfig config;

        internal Game1 gamePtr;
        private TimeSpan oldTS;
        private bool isSpeeding;

        public override void Entry(IModHelper helper)
        {
            ModHelper = Helper;
            config = helper.ReadConfig<SConfig>();

            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        }

        private void OnReturnedToTitle(object sender, EventArgs e)
        {
            Helper.Events.Input.ButtonPressed -= OnButtonPressed;
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            Helper.Events.Display.MenuChanged -= OnMenuChanged;
            Helper.Events.GameLoop.Saving -= OnSaving;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            start();
        }

        private void start()
        {
            gamePtr = Program.gamePtr;

            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Helper.Events.Display.MenuChanged += OnMenuChanged;
            Helper.Events.GameLoop.Saving += OnSaving;

          isSpeeding = false;
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            SpeedsterMask.takeOffCostume();
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (Game1.player.hat is SpeedsterMask && SpeedsterMask.hyperdrive)
            {
                int index = (Game1.player.hat as SpeedsterMask).index;
                SpeedsterMask.hyperdrive = false;
                SpeedsterMask.takeOffCostume();
                SpeedsterMask.putOnCostume(index);
            }

            if(e.NewMenu is ShopMenu)
            {
                ShopMenu shop = (ShopMenu)Game1.activeClickableMenu;
                Dictionary<Item, int[]> items = Helper.Reflection.GetField<Dictionary<Item, int[]>>(shop, "itemPriceAndStock").GetValue();
                List<Item> selling = Helper.Reflection.GetField<List<Item>>(shop, "forSale").GetValue();

                if (items.Keys.FirstOrDefault<Item>() is Hat)
                {
                    Dictionary<Item, int> newItemsToSell = new Dictionary<Item, int>();

                    newItemsToSell.Add(new SpeedsterMask(0), 10000);
                    newItemsToSell.Add(new SpeedsterMask(1), 20000);

                    foreach (Item item in newItemsToSell.Keys)
                    {
                        items.Add(item, new int[] { newItemsToSell[item], int.MaxValue });
                        selling.Add(item);
                    }
                }

            }
        }

        

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (e.IsMultipleOf(4))
            {
                if (Game1.player.hat != null && Game1.player.hat is SpeedsterMask)
                {
                    SpeedsterMask.putOnCostume((Game1.player.hat as SpeedsterMask).index);
                }
                else
                {
                    SpeedsterMask.takeOffCostume();
                }
            }
        }

        private void timeSpeed()
        {

            if (!isSpeeding && Game1.timeOfDay < 2300)
            {
                Game1.playSound("stardrop");
                
                oldTS = new TimeSpan(gamePtr.TargetElapsedTime.Ticks);
                gamePtr.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 1);
                isSpeeding = true;
                speedUpTime();
                Utility.drawLightningBolt(Game1.player.position, Game1.currentLocation);

            }
            else
            {
                Game1.playSound("thunder");
                gamePtr.TargetElapsedTime = oldTS;

                Game1.player.forceTimePass = false;
                isSpeeding = false;
            }
        }

        private void speedUp()
        {
            if (SpeedsterMask.hyperdrive)
            {
                SpeedsterMask.hyperdrive = false;
            }
            else
            {
                SpeedsterMask.hyperdrive = true;
            }

            if (Game1.player.hat is SpeedsterMask)
            {
                int index = (Game1.player.hat as SpeedsterMask).index;
                SpeedsterMask.takeOffCostume();
                SpeedsterMask.putOnCostume(index);
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {

            if (e.Button == config.timeKey && Game1.player.hat is SpeedsterMask)
            {
                if (isSpeeding)
                {
                    timeSpeed();
                }

                speedUp();
            }

            if (e.Button == config.timeKey && (SpeedsterMask.hyperdrive || isSpeeding) && Game1.player.hat is SpeedsterMask)
            {
                if (SpeedsterMask.hyperdrive)
                {
                    speedUp();
                }

                timeSpeed();
            }
        }

        private void speedUpTime()
        {
            if (isSpeeding)
            {
                Game1.playSound("parry");
                Game1.performTenMinuteClockUpdate();

                foreach(NPC npc in Game1.currentLocation.characters)
                {
                    npc.isCharging = true;
                }

                Game1.player.forceTimePass = true;
                Game1.player.freezePause = 1000;
                Game1.player.jitterStrength = 20f;

                DelayedAction timeAction = new DelayedAction(500);
                timeAction.behavior = new DelayedAction.delayedBehavior(speedUpTime);

                Game1.delayedActions.Add(timeAction);
                if(Game1.timeOfDay > 2300 ||  !(Game1.player.hat is SpeedsterMask))
                {
                    gamePtr.TargetElapsedTime = oldTS;
                    isSpeeding = false;
                }
            }
        }

    }
}
