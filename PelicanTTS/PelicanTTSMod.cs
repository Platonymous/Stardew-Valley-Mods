using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using StardewValley;

using System.IO;

namespace PelicanTTS
{
    public class PelicanTTSMod : Mod
    {
        private bool greeted;
        internal static ModConfig config;

        public override void Entry(IModHelper helper)
        {
            string tmppath = Path.Combine(Path.Combine(Environment.CurrentDirectory, "Content"), "TTS");

            if (Directory.Exists(Path.Combine(Helper.DirectoryPath, "TTS")))
                if (!Directory.Exists(tmppath))
                    Directory.CreateDirectory(tmppath);

            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (e.IsOneSecond && !greeted && Game1.timeOfDay == 600 && Game1.activeClickableMenu == null)
            {
                performGreeting();
                greeted = true;
            }
        }


        private void OnDayStarted(object sender, DayStartedEventArgs e)
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

            string greeting = config.Greeting.Replace("{Player}", Game1.player.Name).Replace("{DayName}", dayNameFromDayOfSeason(Game1.dayOfMonth)).Replace("{Day}", @"[say-as interpret-as='date']??????" + (day.Length < 2 ? "0"+day : day) + @"[/say-as]").Replace("{Season}", Game1.currentSeason) + " ";
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



        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            Helper.Events.Input.ButtonPressed -= OnButtonPressed;

            SpeechHandlerPolly.stop();
        }

        
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            config = Helper.ReadConfig<ModConfig>();
            Monitor.Log(config.Greeting);
            SpeechHandlerPolly.Monitor = Monitor;
            SpeechHandlerPolly.start(Helper);

            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
        }


        public static void say(string text)
        {
            SpeechHandlerPolly.currentText = text;
            SpeechHandlerPolly.speak = true;
        }
    }
}
