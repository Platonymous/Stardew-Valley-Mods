using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using Microsoft.Xna.Framework.Input;
using StardewValley;

using System.IO;

namespace PelicanTTS
{
    public class PelicanTTSMod : Mod
    {
        private bool greeted;
        private bool pollySetup;

        public override void Entry(IModHelper helper)
        {
            string tmppath = Path.Combine(Path.Combine(Environment.CurrentDirectory, "Content"), "TTS");

            if (Directory.Exists(Path.Combine(Helper.DirectoryPath, "TTS")))
            {
                Monitor.Log("Setting up Speech Directory");
                if (!Directory.Exists(tmppath))
                    Directory.CreateDirectory(tmppath);

                    //Directory.Move(Path.Combine(Helper.DirectoryPath, "TTS"), tmppath);
            }

            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;
        }

        private void GameEvents_OneSecondTick(object sender, EventArgs e)
        {
            if (!greeted && Game1.timeOfDay == 600 && Game1.activeClickableMenu == null)
            {
                ModConfig config = Helper.ReadConfig<ModConfig>();
                if (config.polly == "on" && pollySetup && SpeechHandlerPolly.culturelang.ToLower() == "en")
                    performGreeting();
                else if (SpeechHandler.culturelang.ToLower() == "en")
                    performGreeting();

                greeted = true;
            }
        }


        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            greeted = false;
        }

        private static string dayNameFromDayOfSeason(int dayOfSeason)
        {
            switch (dayOfSeason % 7)
            {
                case 0:
                    return "Sunday";
                case 1:
                    return "Monday";
                case 2:
                    return "Tuesday";
                case 3:
                    return "Wednesday";
                case 4:
                    return "Thursday";
                case 5:
                    return "Friday";
                case 6:
                    return "Saturday";
                default:
                    return "";
            }
        }

        private void performGreeting()
        {

            NPC birthday = Utility.getTodaysBirthdayNPC(Game1.currentSeason, Game1.dayOfMonth);
            string day = Game1.dayOfMonth.ToString();

            if (day.EndsWith("1") && day != "11")
            {
                day += "st";
            }
            else if (day.EndsWith("2") && day != "12")
            {
                day += "nd";
            }
            else if (day.EndsWith("3") && day != "13")
            {
                day += "rd";
            }
            else
            {
                day += "th";
            }

            string greeting = "Good Morning " + Game1.player.Name + ". It is " + dayNameFromDayOfSeason(Game1.dayOfMonth) + " the " + day + " day of " + Game1.currentSeason + ". ";

            if (birthday != null)
            {
                string person = birthday.Name;
                if (birthday == Game1.player.getSpouse())
                    if (birthday.Gender == 0)
                        person = "Your husband";
                    else
                        person = "Your wife";

                greeting += "Today is " + person + "'s birthday.";
            }

            if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
            {
                int festivalTime = Utility.getStartTimeOfFestival();
                int ftHours = (int)Math.Floor((double)festivalTime / 100);
                int ftMinutes = festivalTime - (ftHours * 100);
                string timeInfo = "a.m.";
                if (ftHours > 12)
                {
                    ftHours -= 12;
                    timeInfo = "p.m.";
                }

                greeting += "Today's festival starts at " + ftHours + " " + timeInfo + ".";
            }


            say(greeting);
        }



        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            ModConfig config = Helper.ReadConfig<ModConfig>();
            if (config.polly == "off" || !pollySetup)
            {
                if (e.KeyPressed == Keys.F7)
                    SpeechHandler.showInstalledVoices();

                if (e.KeyPressed == Keys.F8)
                    SpeechHandler.demoVoices();
            }
        }

        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            GameEvents.OneSecondTick -= GameEvents_OneSecondTick;
            ControlEvents.KeyPressed -= ControlEvents_KeyPressed;
            ModConfig config = Helper.ReadConfig<ModConfig>();
            if (config.polly == "on" && pollySetup)
                SpeechHandlerPolly.stop();
            else
                SpeechHandler.stop();
        }

        private void checkPollySetup()
        {
            pollySetup = true;
            ModConfig config = Helper.ReadConfig<ModConfig>();
            string tmppath = Path.Combine(Path.Combine(Environment.CurrentDirectory, "Content"), "TTS");
                string file = Path.Combine(Path.Combine(tmppath, "default"), "speech191348702.mp3");
                FileInfo fileInfo = new FileInfo(file);

                if (fileInfo.Exists && config.lang.StartsWith("en"))
                    pollySetup = true;
            
        }
        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            pollySetup = false;
            checkPollySetup();

            TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;
            ModConfig config = Helper.ReadConfig<ModConfig>();
            if(config.polly == "on" && pollySetup)
            {
                SpeechHandlerPolly.Monitor = Monitor;
                SpeechHandlerPolly.start(Helper);
                
            }
            else
                SpeechHandler.start(Helper,Monitor);
            
            GameEvents.OneSecondTick += GameEvents_OneSecondTick;
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
        }


        public static void say(string text)
        {

            SpeechHandlerPolly.currentText = text;
            SpeechHandlerPolly.speak = true;
            SpeechHandler.currentText = text;

        }
    }
}
