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
        

        public static void checkMail()
        {
            Letter nextLetter = checkForLetter();
            if (nextLetter != null)
                Game1.mailbox.Enqueue(nextLetter.id);
        }

        public static void checkForProgress(GameLocation location, SheetMusic sheet)
        {

            if (sheet.sheetMusicID == "birthday" && (sheet.magic as BirthdayMagic).lastBirthday != null && (sheet.magic as BirthdayMagic).lastBirthday.name == "Wizard")
                if (Instrument.allAdditionalSaveData.ContainsKey("wizard"))
                    Instrument.allAdditionalSaveData["wizard"] = "true";
                else
                    Instrument.allAdditionalSaveData.Add("wizard", "true");

            if (location is Beach && Game1.isRaining && (location as Beach).bridgeFixed && Game1.player.getTileX() > 70 && Game1.player.getTileY() < 15)
            {
                if (Instrument.allAdditionalSaveData.ContainsKey("mariner"))
                {
                    List<string> played = new List<string>(Instrument.allAdditionalSaveData["mariner"].Split(' '));
                    if (played.Contains(sheet.sheetMusicID) || played.Count >= 5)
                        return;

                    played.Add(sheet.sheetMusicID);
                    Instrument.allAdditionalSaveData["mariner"] = String.Join(" ",played.ToArray());
                }
                else
                    Instrument.allAdditionalSaveData.Add("mariner", sheet.sheetMusicID);

                Game1.playSound("crystal");
            }

            if (location is Farm && Game1.player.getTileX() < 15 && Game1.player.getTileY() < 15)
            {
                if (Instrument.allAdditionalSaveData.ContainsKey("granpa"))
                {
                    List<string> played = new List<string>(Instrument.allAdditionalSaveData["granpa"].Split(' '));
                    if (played.Contains(sheet.sheetMusicID) || played.Count >= 2)
                        return;

                    played.Add(sheet.sheetMusicID);
                    Instrument.allAdditionalSaveData["granpa"] = String.Join(" ", played.ToArray());
                }
                else
                    Instrument.allAdditionalSaveData.Add("granpa", sheet.sheetMusicID);

                Game1.playSound("crystal");
            }

        }

        private static Letter checkForLetter()
        {
            Dictionary<string, string> stats = Instrument.allAdditionalSaveData;

            if (!Instrument.hasInstument("harp"))
                return DataLoader.getLetter("hoy_birthday");

            if (Game1.player.isMarried() && !SheetMusic.hasSheet("yoba"))
                return DataLoader.getLetter("hoy_yoba");

            if (Game1.player.eventsSeen.Contains(2) && !SheetMusic.hasSheet("thunder"))
                return DataLoader.getLetter("hoy_thunder");

            if (Game1.player.eventsSeen.Contains(14) && !SheetMusic.hasSheet("animals"))
                return DataLoader.getLetter("hoy_animals");

            if (Game1.stats.monstersKilled >= 100 && !SheetMusic.hasSheet("adventure"))
                return DataLoader.getLetter("hoy_adventure");

            if (Game1.player.eventsSeen.Contains(191393) && !SheetMusic.hasSheet("wanderer"))
                return DataLoader.getLetter("hoy_wanderer");

            if (stats.ContainsKey("wizard") && stats["wizard"] == "true" && !SheetMusic.hasSheet("dark"))
                return DataLoader.getLetter("hoy_dark");

            if (stats.ContainsKey("mariner") && stats["mariner"].Split(' ').Length >= 5 && !SheetMusic.hasSheet("fisher"))
                return DataLoader.getLetter("hoy_mariner");

            if (stats.ContainsKey("granpa") && stats["granpa"].Split(' ').Length >= 2 && !SheetMusic.hasSheet("granpa"))
                return DataLoader.getLetter("hoy_granpa");

            if (Game1.player.eventsSeen.Contains(18) && !SheetMusic.hasSheet("time"))
                return DataLoader.getLetter("hoy_time");

            return null;
        }

    }
}
