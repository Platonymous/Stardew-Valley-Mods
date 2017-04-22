using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace HarpOfYobaRedux
{
    internal class Delivery
    {
        private static List<Item> items;
        public static bool showsLetter;
        private static Letter nextLetter;
        private static string currentLetter;

        public Delivery()
        {

        }

        public static void showLetter()
        {
     
            if(nextLetter != null && currentLetter == "robinWell")
            {
                showsLetter = true;
                items = nextLetter.items;
                Game1.activeClickableMenu = new LetterViewerMenu(nextLetter.text, nextLetter.letterID);
                MenuEvents.MenuClosed += MenuEvents_MenuClosed; ;
            }
            else if(Game1.mailbox.Count > 0)
            {
                currentLetter = Game1.mailbox.Peek();
            }
            else
            {
                currentLetter = "none";
            }
            
        }

        private static void MenuEvents_MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            nextLetter = (Letter)null;
            if (Game1.mailbox.Count > 0)
            {
                currentLetter = Game1.mailbox.Peek();
            }
            else
            {
                currentLetter = "none";
            }
            MenuEvents.MenuClosed -= MenuEvents_MenuClosed;
            Game1.activeClickableMenu = new ItemGrabMenu(items);
            showsLetter = false;
        }

        public static void checkMail()
        {
            nextLetter = checkForLetter();
            if(nextLetter != null && !Game1.mailbox.Contains("robinWell"))
            {
                Game1.mailbox.Enqueue("robinWell");
                
            }
            if (Game1.mailbox.Count > 0)
            {
                currentLetter = Game1.mailbox.Peek();
            }
            else
            {
                currentLetter = "none";
            }
          
        }



        public static void checkForProgress(GameLocation location, SheetMusic sheet)
        {

            if (sheet.sheetMusicID == "birthday" && (sheet.magic as BirthdayMagic).lastBirthday != null && (sheet.magic as BirthdayMagic).lastBirthday.name == "Wizard")
            {
                if (Instrument.allAdditionalSaveData.ContainsKey("wizard"))
                {
                    Instrument.allAdditionalSaveData["wizard"] = "true";
                }
                else
                {
                    Instrument.allAdditionalSaveData.Add("wizard", "true");
                }
            }

            if (location is Beach && Game1.isRaining && (location as Beach).bridgeFixed && Game1.player.getTileX() > 70 && Game1.player.getTileY() < 15)
            {
                
                if (Instrument.allAdditionalSaveData.ContainsKey("mariner"))
                {

                    List<string> played = new List<string>(Instrument.allAdditionalSaveData["mariner"].Split(' '));
                    if (played.Contains(sheet.sheetMusicID) || played.Count >= 5)
                    {
                        return;
                    }
                    played.Add(sheet.sheetMusicID);
                    Instrument.allAdditionalSaveData["mariner"] = String.Join(" ",played.ToArray());
                }
                else
                {
                    Instrument.allAdditionalSaveData.Add("mariner", sheet.sheetMusicID);
                }
                Game1.playSound("crystal");
            }

            if (location is Farm && Game1.player.getTileX() < 15 && Game1.player.getTileY() < 15)
            {
                if (Instrument.allAdditionalSaveData.ContainsKey("granpa"))
                {
                    List<string> played = new List<string>(Instrument.allAdditionalSaveData["granpa"].Split(' '));
                    if (played.Contains(sheet.sheetMusicID) || played.Count >= 2)
                    {
                        return;
                    }
                    played.Add(sheet.sheetMusicID);
                    Instrument.allAdditionalSaveData["granpa"] = String.Join(" ", played.ToArray());
                }
                else
                {
                    Instrument.allAdditionalSaveData.Add("granpa", sheet.sheetMusicID);
                }
                Game1.playSound("crystal");
            }

        }

        private static Letter checkForLetter()
        {
            string sentLetter = "none";
            List<Item> items = new List<Item>();
            Dictionary<string, string> stats = Instrument.allAdditionalSaveData;

            if (!Instrument.hasInstument("harp") && !SheetMusic.hasSheet("birthday"))
            {
                sentLetter = DataLoader.getLetter("birthday");
                items.Add(new Instrument("harp"));
                items.Add(new SheetMusic("birthday"));
                return new Letter(sentLetter, items);
            }

            if (!Instrument.hasInstument("harp"))
            {
                sentLetter = DataLoader.getLetter("birthday");
                items.Add(new Instrument("harp"));
                return new Letter(sentLetter, items);
            }

            if (!SheetMusic.hasSheet("birthday"))
            {
                sentLetter = DataLoader.getLetter("birthday");
                items.Add(new SheetMusic("birthday"));
                return new Letter(sentLetter, items);
            }

            if (Game1.player.isMarried() && !SheetMusic.hasSheet("yoba"))
            {
                sentLetter = DataLoader.getLetter("yoba");
                items.Add(new SheetMusic("yoba"));

                return new Letter(sentLetter, items);
            }

            if (Game1.player.eventsSeen.Contains(2) && !SheetMusic.hasSheet("thunder"))
            {
                sentLetter = DataLoader.getLetter("thunder");
                items.Add(new SheetMusic("thunder"));

                return new Letter(sentLetter, items);
            }

            if (Game1.player.eventsSeen.Contains(14) && !SheetMusic.hasSheet("animals"))
            {
                sentLetter = DataLoader.getLetter("animals");
                items.Add(new SheetMusic("animals"));

                return new Letter(sentLetter, items);
            }

            if (Game1.stats.monstersKilled >= 100 && !SheetMusic.hasSheet("adventure"))
            {
                sentLetter = DataLoader.getLetter("adventure");
                items.Add(new SheetMusic("adventure"));

                return new Letter(sentLetter, items);
            }

            if (Game1.player.eventsSeen.Contains(191393) && !SheetMusic.hasSheet("wanderer"))
            {
                sentLetter = DataLoader.getLetter("wanderer");
                items.Add(new SheetMusic("wanderer"));

                return new Letter(sentLetter, items);
            }

            if (stats.ContainsKey("wizard") && stats["wizard"] == "true" && !SheetMusic.hasSheet("dark"))
            {
                sentLetter = DataLoader.getLetter("dark");
                items.Add(new SheetMusic("dark"));

                return new Letter(sentLetter, items);
            }

            if (stats.ContainsKey("mariner") && stats["mariner"].Split(' ').Length >= 5 && !SheetMusic.hasSheet("fisher"))
            {
                sentLetter = DataLoader.getLetter("fisher");
                items.Add(new SheetMusic("fisher"));

                return new Letter(sentLetter, items);
            }

            if (stats.ContainsKey("granpa") && stats["granpa"].Split(' ').Length >= 2 && !SheetMusic.hasSheet("granpa"))
            {
                sentLetter = DataLoader.getLetter("granpa");
                items.Add(new SheetMusic("granpa"));

                return new Letter(sentLetter, items);
            }

            if (Game1.player.eventsSeen.Contains(18) && !SheetMusic.hasSheet("time"))
            {
                sentLetter = DataLoader.getLetter("time");
                items.Add(new SheetMusic("time"));

                return new Letter(sentLetter, items);
            }

            return (Letter) null;



        }

    }
}
