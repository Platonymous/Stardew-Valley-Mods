using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;
using System;
using System.Reflection;

namespace ArcadePong
{
    public class PongMinigame : IMinigame
    {
        internal static bool quit = false;

        public PongMinigame()
        {
        }

        public void changeScreenSize()
        {
        }

        public void draw(SpriteBatch b)
        {
            b.Begin();
            if (ArcadePongMod.pong != null)
            {
                var cmenu = Type.GetType("Pong.ModEntry, Pong").GetField("currentMenu", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ArcadePongMod.pong);
                if (cmenu != null)
                {
                    cmenu.GetType().GetMethod("Update").Invoke(cmenu,null);
                    cmenu.GetType().GetMethod("Draw").Invoke(cmenu, new[] { Game1.spriteBatch });
                }
            }
            b.End();
        }

        public void leftClickHeld(int x, int y)
        {
            
        }

        public string minigameId()
        {
            return "Cat.Pong";
        }

        public bool overrideFreeMouseMovement()
        {
            return false;
        }

        public void receiveEventPoke(int data)
        {
            
        }

        public void receiveKeyPress(Keys k)
        {
            
        }

        public void receiveKeyRelease(Keys k)
        {
            
        }

        public void receiveLeftClick(int x, int y, bool playSound = true)
        {
           
        }

        public void receiveRightClick(int x, int y, bool playSound = true)
        {
            
        }

        public void releaseLeftClick(int x, int y)
        {
           
        }

        public void releaseRightClick(int x, int y)
        {
            
        }

        public bool tick(GameTime time)
        {
            return quit;
        }

        public void unload()
        {
            
        }

        public bool doMainGameUpdates()
        {
            return false;
        }
    }
}
