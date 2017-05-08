using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;
using CustomElementHandler;
using System;

namespace CustomTV
{
    public class TVIntercept : Furniture, ISaveElement
    {

        public static string weatherString = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13105");
        public static string fortuneString = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13107");
        public static string queenString = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13114");
        public static string landString = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13111");
        public static string rerunString = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13117");
   
        private TV tv;
        private int currentpage = 0;
        private List<List<Response>> pages = new List<List<Response>>();
        private static Dictionary<string, string> channels = new Dictionary<string, string>();

        private static Dictionary<string, Action<TV, TemporaryAnimatedSprite,StardewValley.Farmer,string>> actions = new Dictionary<string, Action<TV, TemporaryAnimatedSprite, StardewValley.Farmer, string>>();

        public IPrivateField<TemporaryAnimatedSprite> tvScreen;
        public static TVIntercept activeIntercept;

        public TVIntercept()
        {
        }

        public TVIntercept(int which, Vector2 tile)
             : base(which, tile)
		{
        }

        public TVIntercept(TV tv)
             : base(tv.parentSheetIndex, tv.tileLocation)
        {
            this.tv = tv;
            tvScreen = CustomTVMod.Modhelper.Reflection.GetPrivateField<TemporaryAnimatedSprite>(tv, "screen");
          
        }

        public static void addChannel(string id, string name, Action<TV, TemporaryAnimatedSprite, StardewValley.Farmer, string> action)
        {
            if (!channels.ContainsKey(id))
            {
                channels.Add(id, name);
            }

            if (!actions.ContainsKey(id))
            {
                actions.Add(id, action);
            }

        }

        public static void changeAction(string id, Action<TV, TemporaryAnimatedSprite, StardewValley.Farmer, string> action)
        {
            if (actions.ContainsKey(id))
            {
                actions[id] = action;
   
            }
           
        }

        public static void removeKey(string key)
        {
            if (channels.ContainsKey(key))
            {
                channels.Remove(key);
       
            }

            if (actions.ContainsKey(key))
            {
                actions.Remove(key);
            }

      
        }

        public void showChannels(int page)
        {
            currentpage = page;
            string question = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13120", new object[0]);
            List<string> defaults = new List<string>(new string[5] { "fortune", "weather", "queen", "rerun", "land" });

            Response more = new Response("more", "(More)");
            Response leave = new Response("leave", Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13118", new object[0]));

            pages = new List<List<Response>>();
            List<Response> responses = new List<Response>();

            if (channels.ContainsKey("weather"))
            {
                responses.Add(new Response("weather", channels["weather"]));
            }


            if (channels.ContainsKey("fortune"))
            {
                responses.Add(new Response("fortune", channels["fortune"]));
            }

            string text = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
            if ((text.Equals("Mon") || text.Equals("Thu")) && channels.ContainsKey("land"))
            {
                responses.Add(new Response("land", channels["land"]));
            }
            if (text.Equals("Sun") && channels.ContainsKey("queen"))
            {
                responses.Add(new Response("queen", channels["queen"]));
            }
            if (text.Equals("Wed") && Game1.stats.DaysPlayed > 7u && channels.ContainsKey("rerun"))
            {
                responses.Add(new Response("rerun", channels["rerun"]));
            }

            foreach (string id in channels.Keys)
            {
                if (defaults.Contains(id)) { continue; }

                responses.Add(new Response(id, channels[id]));
                
                if (responses.Count > 7)
                {
                    if (!responses.Contains(more))
                    {
                        responses.Add(more);
                    }

                    if (!responses.Contains(leave))
                    {
                        responses.Add(leave);
                    }

                    pages.Add(new List<Response>(responses.ToArray()));
                    responses = new List<Response>();
                }

            }

            if (!responses.Contains(leave))
            {
                responses.Add(leave);
            }

            if (responses.Count > 1)
            {
                pages.Add(new List<Response>(responses.ToArray()));
            }


            Game1.currentLocation.createQuestionDialogue(question, pages[page].ToArray(), new GameLocation.afterQuestionBehavior(selectChannel), null);
            Game1.player.Halt();
        }

        public static void showProgram(TemporaryAnimatedSprite sprite, string text, Action afterDialogues = null)
        {
            if(afterDialogues == null)
            {
                afterDialogues = endProgram;
            }
            
            activeIntercept.tvScreen.SetValue(sprite);
            Game1.drawObjectDialogue(Game1.parseText(text));
            Game1.afterDialogues = new Game1.afterFadeFunction(afterDialogues);
        }

        public static void endProgram()
        {
            activeIntercept.turnOffTV();
            activeIntercept = null;
        }

        public override bool checkForAction(StardewValley.Farmer who, bool justCheckingForActivity = false)
        {

            
            if (justCheckingForActivity)
            {
                return true;
            }

            activeIntercept = this;
            showChannels(0);

            return true;
        }

        public override Item getOne()
        {
            
            return tv.getOne();
        }

        public override void updateWhenCurrentLocation(GameTime time)
        {
            base.updateWhenCurrentLocation(time);
        }

        public void selectChannel(StardewValley.Farmer who, string answer)
        {
            
            string a = answer.Split(' ')[0];
            CustomTVMod.monitor.Log("Select Channel:"+a);
          
            if (a == "more")
            {
                showChannels(currentpage +1);
            }

            if (actions.ContainsKey(a))
            {
                actions[a].Invoke(tv,tvScreen.GetValue(),who,a);
            }

            return;
        }

        private string getFortuneTellerOpening()
        {
            IPrivateMethod method =  CustomTVMod.Modhelper.Reflection.GetPrivateMethod(tv, "getFortuneTellerOpening");
            return method.Invoke<string>(new object[0]);
            
        }

        private string getWeatherChannelOpening()
        {
            IPrivateMethod method = CustomTVMod.Modhelper.Reflection.GetPrivateMethod(tv, "getWeatherChannelOpening");
            return method.Invoke<string>(new object[0]);
        }

        public float getScreenSizeModifier()
        {
            return tv.getScreenSizeModifier();
        }

        public Vector2 getScreenPosition()
        {
            return tv.getScreenPosition();
        }

        public void proceedToNextScene()
        {
            tv.proceedToNextScene();
        }

        public void turnOffTV()
        {
            tv.turnOffTV();
        }

        private void setWeatherOverlay()
        {
            IPrivateMethod method = CustomTVMod.Modhelper.Reflection.GetPrivateMethod(tv, "setWeatherOverlay");
            method.Invoke(new object[0]);
        }

        private string getTodaysTip()
        {
            IPrivateMethod method = CustomTVMod.Modhelper.Reflection.GetPrivateMethod(tv, "getTodaysTip");
            return method.Invoke<string>(new object[0]);
        }

        private string[] getWeeklyRecipe()
        {
            IPrivateMethod method = CustomTVMod.Modhelper.Reflection.GetPrivateMethod(tv, "getWeeklyRecipe");
            return method.Invoke<string[]>(new object[0]);
        }

        private string getWeatherForecast()
        {
            IPrivateMethod method = CustomTVMod.Modhelper.Reflection.GetPrivateMethod(tv, "getWeatherForecast");
            return method.Invoke<string>(new object[0]);
        }

        private void setFortuneOverlay()
        {
            IPrivateMethod method = CustomTVMod.Modhelper.Reflection.GetPrivateMethod(tv, "setFortuneOverlay");
            method.Invoke(new object[0]);
        }

        private string getFortuneForecast()
        {
            IPrivateMethod method = CustomTVMod.Modhelper.Reflection.GetPrivateMethod(tv, "getFortuneForecast");
            return method.Invoke<string>(new object[0]);
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            tv.draw(spriteBatch, x, y, alpha);
        }

        
        public Dictionary<string, string> getAdditionalSaveData()
        {
            

           Dictionary <string, string> savedata = new Dictionary<string, string>();
            savedata.Add("name", tv.name);
            return savedata;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
           
            tv = new TV(((TV) replacement).parentSheetIndex, ((TV)replacement).tileLocation);
            tvScreen = CustomTVMod.Modhelper.Reflection.GetPrivateField<TemporaryAnimatedSprite>(tv, "screen");

            tileLocation = tv.tileLocation;
            parentSheetIndex = tv.parentSheetIndex;
            name = tv.name;
           
            furniture_type = tv.furniture_type;
            defaultSourceRect = tv.defaultSourceRect;
            drawHeldObjectLow = tv.drawHeldObjectLow;
            
            sourceRect = tv.sourceRect;
            defaultSourceRect = tv.defaultSourceRect;
            
            defaultBoundingBox = tv.defaultBoundingBox;
            boundingBox = tv.boundingBox;

            updateDrawPosition();
            rotations = tv.rotations;
            price = tv.price;
        }

        public object getReplacement()
        {
            return tv;
        }
    }
}
