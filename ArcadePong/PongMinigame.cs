using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Minigames;
using System;

namespace ArcadePong
{
    public class PongMinigame : IMinigame
    {
       internal static object game;
        internal static IModHelper helper => ArcadePongMod.pongHelper;
        internal static bool setEvents = false;
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
            helper.Reflection.GetMethod(game, "Draw").Invoke(b);
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
            Game1.activeClickableMenu = null;
            helper.Reflection.GetMethod(game, "Update").Invoke();
            return quit;
        }

        public void unload()
        {
            
        }
    }
}
