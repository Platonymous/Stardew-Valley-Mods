using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using Microsoft.Xna.Framework.Graphics;


using System.IO;
using StardewValley.Menus;

namespace Speedster
{
    public class SpeedsterMod : Mod
    {

        internal static IModHelper ModHelper;
   
        internal Game1 gamePtr;
        private TimeSpan oldTS;
        private bool isSpeeding;
        private int phase = 0;
 

        public override void Entry(IModHelper helper)
        {
            ModHelper = Helper;
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;


           
        }

        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
 
            ControlEvents.KeyPressed -= ControlEvents_KeyPressed;
            GameEvents.FourthUpdateTick -= GameEvents_FourthUpdateTick;
            MenuEvents.MenuChanged -= MenuEvents_MenuChanged;
          

        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            start();
        }

        private void start()
        {
            Type type = typeof(Game1).Assembly.GetType("StardewValley.Program", true);
            gamePtr = (Game1)type.GetField("gamePtr").GetValue(null);

            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            GameEvents.FourthUpdateTick += GameEvents_FourthUpdateTick;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;

          isSpeeding = false;
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            SpeedsterMask.takeOffCostume();
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if(Game1.activeClickableMenu is ShopMenu)
            {
                ShopMenu shop = (ShopMenu)Game1.activeClickableMenu;
                Dictionary<Item, int[]> items = Helper.Reflection.GetPrivateValue<Dictionary<Item, int[]>>(shop, "itemPriceAndStock");
                List<Item> selling = Helper.Reflection.GetPrivateValue<List<Item>>(shop, "forSale");

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

        

        private void GameEvents_FourthUpdateTick(object sender, EventArgs e)
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

        private void timeSpeed()
        {

            if (!isSpeeding && Game1.timeOfDay < 2300)
            {
                Game1.playSound("stardrop");
                oldTS = new TimeSpan(gamePtr.TargetElapsedTime.Ticks);
                gamePtr.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 1);
                isSpeeding = true;
                speedUpTime();
            }
            else
            {
                gamePtr.TargetElapsedTime = oldTS;
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

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
   
            if(e.KeyPressed == Microsoft.Xna.Framework.Input.Keys.Space && Game1.player.hat is SpeedsterMask)
            {

                if (phase == 0)
                {
                    speedUp();
                }

                if (phase == 1)
                {
                    timeSpeed();
                }

                if (phase == 2)
                {
                    timeSpeed();
                    speedUp();
                    phase = -1;
                }

                phase++;

                

            }



        }

        private Texture2D loadTexture(string file)
        {
            string path = Path.Combine(Helper.DirectoryPath, "Assets", file);
            Image textureImage = Image.FromFile(path);
            Texture2D texture = Bitmap2Texture(new Bitmap(textureImage));
            return texture;
        }

        private Texture2D Bitmap2Texture(Bitmap bmp)
        {
            MemoryStream s = new MemoryStream();

            bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
            s.Seek(0, SeekOrigin.Begin);
            Texture2D tx = Texture2D.FromStream(Game1.graphics.GraphicsDevice, s);

            return tx;

        }

        private void speedUpTime()
        {
            if (isSpeeding)
            {
                Game1.playSound("parry");
                Game1.performTenMinuteClockUpdate();

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
