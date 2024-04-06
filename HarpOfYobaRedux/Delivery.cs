using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;

namespace HarpOfYobaRedux
{
    internal class Delivery
    {

        public Delivery()
        {

        }

        public static void checkForProgress(GameLocation location, SheetMusic sheet)
        {

            if (sheet.sheetMusicID == "birthday" && (sheet.magic as BirthdayMagic).lastBirthday != null && (sheet.magic as BirthdayMagic).lastBirthday.Name == "Wizard")
            {
                if (Instrument.allAdditionalSaveData.ContainsKey("wizard"))
                    Instrument.allAdditionalSaveData["wizard"] = "true";
                else
                    Instrument.allAdditionalSaveData.Add("wizard", "true");

                Game1.addMailForTomorrow("hoy_dark");
                Game1.playSound("crystal");
            }

            if (location is Beach && Game1.isRaining && (location as Beach).bridgeFixed.Value)
            {
                if (Instrument.allAdditionalSaveData.ContainsKey("mariner"))
                {
                    List<string> played = new List<string>(Instrument.allAdditionalSaveData["mariner"].Split(' '));
                    if (played.Contains(sheet.sheetMusicID))
                        return;

                    played.Add(sheet.sheetMusicID);
                    Instrument.allAdditionalSaveData["mariner"] = String.Join(" ",played.ToArray());
                }
                else
                    Instrument.allAdditionalSaveData.Add("mariner", sheet.sheetMusicID);

                Game1.addMailForTomorrow("hoy_mariner");
                Game1.playSound("crystal");
            }

            if (location is Farm)
            {
                if (Instrument.allAdditionalSaveData.ContainsKey("granpa"))
                {
                    List<string> played = new List<string>(Instrument.allAdditionalSaveData["granpa"].Split(' '));
                    
                    played.Add(sheet.sheetMusicID);
                    Instrument.allAdditionalSaveData["granpa"] = String.Join(" ", played.ToArray());
                }
                else
                    Instrument.allAdditionalSaveData.Add("granpa", sheet.sheetMusicID);

                Game1.addMailForTomorrow("hoy_granpa");
                Game1.playSound("crystal");
            }

        }

        public static void checkMail()
        {
            Dictionary<string, string> stats = Instrument.allAdditionalSaveData;

            if (!Game1.player.mailReceived.Contains("hoy_birthday"))
                Game1.addMailForTomorrow("hoy_birthday");

            if (Game1.player.isMarriedOrRoommates() && !Game1.player.mailReceived.Contains("hoy_yoba"))
                Game1.addMailForTomorrow("hoy_yoba");

            if (Game1.player.eventsSeen.Contains("2") && !Game1.player.mailReceived.Contains("hoy_thunder"))
                Game1.addMailForTomorrow("hoy_thunder");

            if (Game1.player.eventsSeen.Contains("14") && !Game1.player.mailReceived.Contains("hoy_animals"))
                Game1.addMailForTomorrow("hoy_animals");

            if (Game1.stats.MonstersKilled >= 100 && !Game1.player.mailReceived.Contains("hoy_adventure"))
                Game1.addMailForTomorrow("hoy_adventure");

            if (Game1.player.eventsSeen.Contains("191393") && !Game1.player.mailReceived.Contains("hoy_wanderer"))
                Game1.addMailForTomorrow("hoy_wanderer");

            if (Game1.player.eventsSeen.Contains("112") && !Game1.player.mailReceived.Contains("hoy_dark"))
                Game1.addMailForTomorrow("hoy_dark");

            if (stats.ContainsKey("mariner") && stats["mariner"].Split(' ').Length >= 5 && !Game1.player.mailReceived.Contains("hoy_mariner"))
                Game1.addMailForTomorrow("hoy_mariner");

            if (stats.ContainsKey("granpa") && stats["granpa"].Split(' ').Length >= 2 && !Game1.player.mailReceived.Contains("hoy_granpa"))
                Game1.addMailForTomorrow("hoy_granpa");

            if (Game1.player.eventsSeen.Contains("18") && !Game1.player.mailReceived.Contains("hoy_time"))
                Game1.addMailForTomorrow("hoy_time");

        }

    }
}
