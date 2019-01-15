using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using StardewValley;
using System.IO;

namespace PelicanTTS
{
    public class PelicanTTSMod : Mod
    {
        internal static bool greeted;
        internal static ModConfig config;
        internal static IModHelper _helper;
        internal static ITranslationHelper i18n => _helper.Translation;

        public override void Entry(IModHelper helper)
        {
            _helper = helper;
            config = Helper.ReadConfig<ModConfig>();
            Helper.WriteConfig<ModConfig>(config);
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
                    return i18n.Get("Sunday");
                case 1:
                    return i18n.Get("Monday");
                case 2:
                    return i18n.Get("Tuesday");
                case 3:
                    return i18n.Get("Wednesday");
                case 4:
                    return i18n.Get("Thursday");
                case 5:
                    return i18n.Get("Friday");
                case 6:
                    return i18n.Get("Saturday");
                default:
                    return "";
            }
        }

        private void performGreeting()
        {
            if (!config.Greeting)
                return;

            NPC birthday = Utility.getTodaysBirthdayNPC(Game1.currentSeason, Game1.dayOfMonth);
            string day = Game1.dayOfMonth.ToString();

            string greeting = i18n.Get("Greeting").ToString().Replace("{Player}", Game1.player.Name).Replace("{DayName}", dayNameFromDayOfSeason(Game1.dayOfMonth)).Replace("{Day}", @"[say-as interpret-as='date']??????" + (day.Length < 2 ? "0"+day : day) + @"[/say-as]").Replace("{Season}", i18n.Get(Game1.currentSeason)) + " ";
            if (birthday != null)
            {
                string person = birthday.Name;
                if (birthday == Game1.player.getSpouse())
                    if (birthday.Gender == 0)
                        person = i18n.Get("Your husband");
                    else
                        person = i18n.Get("Your wife");

                greeting += i18n.Get("BirthdayGreeting").ToString().Replace("{NPC}",person);
            }

            if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
            {
                int festivalTime = Utility.getStartTimeOfFestival();
                int ftHours = (int)Math.Floor((double)festivalTime / 100);
                int ftMinutes = festivalTime - (ftHours * 100);
                string timeInfo = i18n.Get("a.m.");
                if (ftHours > 12)
                {
                    ftHours -= 12;
                    timeInfo = i18n.Get("p.m.");
                }

                greeting += i18n.Get("FestivalGreeting") + " " + ftHours + " " + timeInfo + ".";
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
            SpeechHandlerPolly.Monitor = Monitor;
            SpeechHandlerPolly.start(Helper);

            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
        }


        public static void say(string text)
        {
            SpeechHandlerPolly.lastSay = text;
            SpeechHandlerPolly.currentText = text;
            SpeechHandlerPolly.speak = true;
        }
    }
}
