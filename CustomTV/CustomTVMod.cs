using System;
using System.Collections.Generic;
using System.Linq;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;
using StardewValley.Locations;
namespace CustomTV
{
    public class CustomTVMod : Mod
    {

        internal static IModHelper Modhelper;
        internal static IMonitor monitor;

        public override void Entry(IModHelper helper)
        {
            Modhelper = Helper;
            monitor = Monitor;

            LocationEvents.CurrentLocationChanged += LocationEvents_CurrentLocationChanged;

            loadDefaultChannels();

        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if(e.KeyPressed.ToString().ToLower() == "h")
            {
                checkLocation((DecoratableLocation)Game1.currentLocation);
            }
        }

        private void checkLocation(DecoratableLocation location)
        {
            location = (DecoratableLocation)Game1.currentLocation;

                for (int i = 0; i < location.furniture.Count; i++)
                {
                    if (location.furniture[i] is TV tv)
                    {
                        location.furniture[i] = new TVIntercept(tv);
                    }

                }
            
        }

        private void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
           if(Game1.currentLocation is DecoratableLocation dl)
            {
                checkLocation(dl);
            }
        }

        private void loadDefaultChannels()
        {
            addChannel("weather", TVIntercept.weatherString, showOriginalProgram);
            addChannel("fortune", TVIntercept.fortuneString, showOriginalProgram);
            addChannel("land", TVIntercept.landString, showOriginalProgram);
            addChannel("queen", TVIntercept.queenString, showOriginalProgram);
            addChannel("rerun", TVIntercept.rerunString, showOriginalProgram);
            
            
        }


        private static void defaultAction(TV tv, TemporaryAnimatedSprite sprite, StardewValley.Farmer who, string a)
        {

        }

        private static void showOriginalProgram(TV tv, TemporaryAnimatedSprite sprite, StardewValley.Farmer who, string a)
        {
            switch (a)
            {
                case "weather": a = "Weather"; break;
                case "queen": a = "The"; break;
                case "rerun": a = "The"; break;
                case "land": a = "Livin'"; break;
                case "fortune": a = "Fortune"; break;
                default: a = "Weather"; break;
            }

            tv.selectChannel(who, a);
        }

        public static void changeAction(string id, Action<TV, TemporaryAnimatedSprite, StardewValley.Farmer, string> action)
        {
            TVIntercept.changeAction(id, action);
        }

        public static void removeChannel(string key)
        {
           TVIntercept.removeKey(key);
        }


        public static void addChannel(string id, string name, Action<TV,TemporaryAnimatedSprite,StardewValley.Farmer,string> action = null)
        {
            if (action == null)
            {
                action = defaultAction;
            }

           TVIntercept.addChannel(id, name, action);
        }

        public static void endProgramm()
        {
            if (TVIntercept.activeIntercept != null)
            {
                TVIntercept.endProgramm();
            }
        }

        public static void showProgram(TemporaryAnimatedSprite sprite, string text, Action afterDialogues = null)
        {
     
            if(TVIntercept.activeIntercept != null)
            {
           
                TVIntercept.showProgramm(sprite, text, afterDialogues);
            }
        }




    }
}
