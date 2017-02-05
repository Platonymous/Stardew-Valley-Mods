using System;

using System.Collections.Generic;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

using Microsoft.Xna.Framework;

using TheHarpOfYoba.Menus;
using System.Text.RegularExpressions;

namespace TheHarpOfYoba
{
    public class HarpOfYobaMod : Mod
    {
        public bool has_harp;

        public bool showLetter;
        public bool readyLetter;
        public bool[] has_sheet;
        public int next_letter;
        public bool once;

        private HarpOfYoba newHarp;
        private SheetMusic sheetMusic1;
        private SheetMusic sheetMusic2;
        private SheetMusic sheetMusic3;
        private SheetMusic sheetMusic4;
        private SheetMusic sheetMusic5;
        private SheetMusic sheetMusic6;

        public static bool[] processIndicators = { false, false, false, false, false, false };
        public bool playedAllSongsForFisherman;


        private LoadData dataLoader;
        public static string savstring;
        private static int numSheets = 6;
        private static List<SheetMusic> sheetList = new List<SheetMusic>();


        public override void Entry(IModHelper helper)
        {
            this.has_harp = false;
            this.showLetter = false;
            this.readyLetter = false;
            this.next_letter = 0;
            this.playedAllSongsForFisherman = false;
            this.once = true;
            sheetList = new List<SheetMusic>(numSheets);


            //ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            TimeEvents.TimeOfDayChanged += TimeEvents_TimeOfDayChanged;
            GameEvents.EighthUpdateTick += updateForMail;
            GameEvents.OneSecondTick += updateProcess;
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            SaveEvents.AfterSave += SaveEvents_AfterSave;
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;

        }

        private void SaveEvents_AfterSave(object sender, EventArgs e)
        {
            LoadAndReplace(savstring);
        //this.Monitor.Log("Loading: " + savstring);
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            loadObjects();
            if (dataLoader.doesSavFileExist(Game1.uniqueIDForThisGame, Game1.player.name)) { 
            savstring = dataLoader.loadSavStringFromFile(Game1.uniqueIDForThisGame, Game1.player.name);
            if (savstring != "") {

                LoadAndReplace(savstring);
            }
            }

        }


        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            checkProcess();
            dataLoader.saveSavStringToFile(this.RemoveAndSave(), Game1.uniqueIDForThisGame, Game1.player.name);
            // this.Monitor.Log("Saving: " + dataLoader.saveSavStringToFile(this.RemoveAndSave(), Game1.uniqueIDForThisGame, Game1.player.name));


        }


        private void loadObjects()
        {
            if (this.once) { 
            this.dataLoader = new LoadData();
            this.newHarp = new HarpOfYoba(dataLoader.getSprite(0), dataLoader.getSprite(1));
            this.sheetMusic1 = new SheetMusic(dataLoader.getSprite(2), 5, "The Fisherman's Lament", "poppy", new FisherEvent());
            this.sheetMusic2 = new SheetMusic(dataLoader.getSprite(2), 4, "Ballad of the Wanderer", "honkytonky", new TeleportEvent());
            this.sheetMusic3 = new SheetMusic(dataLoader.getSprite(2), 3, "Serenade of Thunder", "AbigailFluteDuet", new RainEvent());
            this.sheetMusic4 = new SheetMusic(dataLoader.getSprite(2), 2, "Prelude of Yoba", "wedding", new YobaEvent());
            this.sheetMusic5 = new SheetMusic(dataLoader.getSprite(2), 1, "Ode to the Dark", "heavy", new MonsterEvent());
            this.sheetMusic6 = new SheetMusic(dataLoader.getSprite(2), 0, "Birthday Sonata", "shimmeringbastion", new BirthdayEvent());
                sheetList = new List<SheetMusic>(numSheets);
                sheetList.Add(sheetMusic6);
                sheetList.Add(sheetMusic5);
                sheetList.Add(sheetMusic4);
                sheetList.Add(sheetMusic3);
                sheetList.Add(sheetMusic2);
                sheetList.Add(sheetMusic1);

                this.once = false;
            }
        }

      

        private string LoadAndReplace(string save)
        {

            loadObjects();

            string[] savedata = Regex.Split(save,"-S-");

            string[] saveHarpDataSet = Regex.Split(savedata[0], "-");
            string saveHarp = saveHarpDataSet[1];
            string saveHarpAttachement = saveHarpDataSet[2];

            if (saveHarp != "none")
            {
                string[] saveHarpPlacement = Regex.Split(saveHarp,"/");

                if (saveHarpPlacement[0] == "items")
                {
                   
                    Game1.player.items[int.Parse(saveHarpPlacement[1])] = newHarp;
                    HarpOfYoba.owned = true;
                    if (saveHarpAttachement != "none")
                    {
                        newHarp.sheet = sheetList[int.Parse(saveHarpAttachement)];
                    }

                } else if(saveHarpPlacement[0] == "chest")
                {
                    GameLocation gl = Game1.getLocationFromName(saveHarpPlacement[1]);
                   Chest che = (Chest) gl.objects[new Vector2(int.Parse(saveHarpPlacement[2]), int.Parse(saveHarpPlacement[3]))];
                    che.items[int.Parse(saveHarpPlacement[4])] = newHarp;
                   HarpOfYoba.owned = true;
                    if(saveHarpAttachement != "none")
                    {
                        newHarp.sheet = sheetList[int.Parse(saveHarpAttachement)];
                    }
                }
                

            }

            if (savedata[1] != "none") {

                string[] saveSheetDataSet = Regex.Split(savedata[1], "-");

                foreach (string saveSheetData in saveSheetDataSet)
                {
                    string[] saveSheetPlacement = Regex.Split(saveSheetData, "/");

                    if (saveSheetPlacement[1] == "items")
                    {
                        if (saveHarpAttachement != saveSheetPlacement[0]) { 
                        Game1.player.items[int.Parse(saveSheetPlacement[2])] = sheetList[int.Parse(saveSheetPlacement[0])];
                        }
                        SheetMusic.owned[int.Parse(saveSheetPlacement[0])] = true;
                    }
                    else if (saveSheetPlacement[1] == "chest")
                    {
                        if (saveHarpAttachement != saveSheetPlacement[0])
                        {
                            GameLocation gl = Game1.getLocationFromName(saveSheetPlacement[2]);
                        Chest che = (Chest)gl.objects[new Vector2(int.Parse(saveSheetPlacement[3]), int.Parse(saveSheetPlacement[4]))];
                        che.items[int.Parse(saveSheetPlacement[5])] = sheetList[int.Parse(saveSheetPlacement[0])];
                        }
                        SheetMusic.owned[int.Parse(saveSheetPlacement[0])] = true;

                    }

                }


            }

            string[] processData = Regex.Split(savedata[2],"-");

            for (int i = 0; i < processData.Length; i++)
            {
                if(processData[i] == "True")
                {
                    processIndicators[i] = true;

                }
                
            }

            return save;

        }


        private string RemoveAndSave()
        {

            savstring = Game1.player.name;


            bool harp_saved = false;
            string harp_storage = "none";

            string harp_attachement = "none";

            if (!HarpOfYoba.owned)
            {
                harp_saved = true;
            }

            if (this.newHarp.sheet != null)
            {
                harp_attachement = this.newHarp.sheet.pos.ToString();
                
            }

            if (!harp_saved)
            {
                for (int index = 0; index < Game1.player.items.Count; ++index)
                {
                    if (Game1.player.items[index] is HarpOfYoba)
                    {
                        harp_storage = $"items/{index}";
                        harp_saved = true;
                        HarpOfYoba.owned = false;
                        Game1.player.items[index] = new Hat(1); ;
                        break;
                    }
                }
            }
            if (!harp_saved)
            {
                foreach (GameLocation gl in Game1.locations)
                {
                    if (harp_saved) { break; }
                    foreach (Vector2 KeyV in gl.objects.Keys)
                    {
                        if (harp_saved) { break; }
                        if (gl.objects[KeyV] is Chest)
                        {
                            Chest c = (Chest)gl.objects[KeyV];
                            for (int index = 0; index < c.items.Count; ++index)
                            {
                                if (c.items[index] is HarpOfYoba)
                                {
                                    harp_storage = $"chest/{gl.name}/{KeyV.X}/{KeyV.Y}/{index}";
                                    harp_saved = true;
                                    HarpOfYoba.owned = false;
                                    c.items[index] = new Hat(1);
                                    break;
                                }
                            }
                        }

                    }

                }


            }

            savstring += "-" + harp_storage + "-" + harp_attachement;


            int nSheets = numSheets;
            string sheet_storage = "-S";


            for (int index = 0; index < Game1.player.items.Count; ++index)
            {
                if (nSheets == 0) { break; }
                if (Game1.player.items[index] is SheetMusic)
                {
                    SheetMusic sm = (SheetMusic)Game1.player.items[index];
                    nSheets--;
                    if(sm.pos.ToString() != harp_attachement)
                    { 
                    sheet_storage += $"-{sm.pos}/items/{index}";
                    SheetMusic.owned[sm.pos] = false;
                    
                    Game1.player.items[index] = new Hat(1);
                    }
                }
            }

            if (nSheets > 0)
            {
                foreach (GameLocation gl in Game1.locations)
                {
                    if (nSheets == 0) { break; }

                    foreach (Vector2 KeyV in gl.objects.Keys)
                    {
                        if (nSheets == 0) { break; }

                        if (gl.objects[KeyV] is Chest)
                        {
                            Chest c = (Chest)gl.objects[KeyV];
                            for (int index = 0; index < c.items.Count; ++index)
                            {
                                if (nSheets == 0) { break; }
                                if (c.items[index] is SheetMusic)
                                {
                                    SheetMusic sm = (SheetMusic)c.items[index];
                                    nSheets--;
                                    if (sm.pos.ToString() != harp_attachement)
                                    {
                                        sheet_storage += $"-{sm.pos}/chest/{gl.name}/{KeyV.X}/{KeyV.Y}/{index}";
                                        SheetMusic.owned[sm.pos] = false;
                                        
                                        c.items[index] = new Hat(1);
                                    }
                                }
                            }
                        }

                    }

                }


            }

            if (sheet_storage == "-S")
            {

                sheet_storage = "-S-none";

            }

            savstring += sheet_storage+"-S";


            for (int i = 0; i < processIndicators.Length; i++)
            {
                savstring += "-" + processIndicators[i].ToString();
                processIndicators[i] = false;
            }


            this.once = true;

            return(savstring);

        }

        private void updateProcess(object sender, EventArgs e)
        {
            checkProcess();
        }


        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            if (e.KeyPressed.ToString() == "P")
            {
                this.Monitor.Log(this.RemoveAndSave());
            }

            if (e.KeyPressed.ToString() == "O")
            {
               LoadAndReplace(savstring);
            }

            if (e.KeyPressed.ToString() == "L")
            {
                this.Monitor.Log("Send Letters");
                sendLetters();

            }

            if (e.KeyPressed.ToString() == "R")
            {
                processIndicators[5] = true;
                Game1.isRaining = true;
                this.Monitor.Log("Start Rain");

            }

            if (e.KeyPressed.ToString() == "I")
            {
                this.newHarp.charger = true;
                for (int i = 0; i < processIndicators.Length; i++) { 
                this.Monitor.Log($"Indicator {i} : {processIndicators[i].ToString()}");
                }

            }

        }
 
        private void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
            if (e.PriorInt == 600 || e.PriorInt == 1200 || e.PriorInt == 1800)
            {

                sendLetters();

            }
        }

        private void checkProcess()
        {

            playedAllSongsForFisherman = true;
            for (int f = 0; f < processIndicators.Length; f++)
            {

                if (!processIndicators[f])
                {
                    playedAllSongsForFisherman = false;
                    break;
                }

            }

            if (!HarpOfYoba.owned)
            {
                for (int index = 0; index < Game1.player.items.Count; ++index)
                {
                    if (Game1.player.items[index] is HarpOfYoba)
                        HarpOfYoba.owned = true;
                }
            }

            for (int j = 0; j < SheetMusic.owned.Length; j++)
            {
                if (!SheetMusic.owned[j])
                {
                    for (int index = 0; index < Game1.player.items.Count; ++index)
                    {
                        if (Game1.player.items[index] is SheetMusic)
                        {
                            SheetMusic temp = (SheetMusic)Game1.player.items[index];
                            SheetMusic.owned[temp.pos] = true;
                        }
                        else if(Game1.player.items[index] is HarpOfYoba)
                        {
                            HarpOfYoba temp = (HarpOfYoba) Game1.player.items[index];
                            if(temp.sheet != null) { 
                            SheetMusic.owned[temp.sheet.pos] = true;
                            }
                        }
                    }
                }
            }


        }

        private void updateForMail(object sender, EventArgs e)
        {
        
            if (Game1.activeClickableMenu is LetterViewerMenu && readyLetter)
            {


                if (!this.showLetter && !Game1.mailbox.Contains("robinWell"))
                {



                    if (next_letter == 0)
                    {
                        List<Item> items = new List<Item>();
                        items.Add(this.newHarp);
                        items.Add(this.sheetMusic6);
                        Game1.activeClickableMenu = new CustomLetterMenu(dataLoader.getLetter(0), "Harp of Yoba and Birthday Song", items);

                    }

                    if (next_letter == 10)
                    {
                       
                        Game1.activeClickableMenu = new CustomLetterMenu(dataLoader.getLetter(0), "Harp of Yoba", this.newHarp);

                    }

                    if (next_letter == 11)
                    {
                        Game1.activeClickableMenu = new CustomLetterMenu(dataLoader.getLetter(0), "Birthday Song", this.sheetMusic6);

                    }

                    if (next_letter == 1)
                    {

                        Game1.activeClickableMenu = new CustomLetterMenu(dataLoader.getLetter(1), "Monster Song", this.sheetMusic5);


                    }

                    if (next_letter == 2)
                    {

                        Game1.activeClickableMenu = new CustomLetterMenu(dataLoader.getLetter(2), "Wedding Song", this.sheetMusic4);


                    }

                    if (next_letter == 3)
                    {

                        Game1.activeClickableMenu = new CustomLetterMenu(dataLoader.getLetter(3), "Rain Song", this.sheetMusic3);


                    }

                    if (next_letter == 4)
                    {

                        Game1.activeClickableMenu = new CustomLetterMenu(dataLoader.getLetter(4), "Wanderer Song", this.sheetMusic2);

                    }

                    if (next_letter == 5)
                    {

                        Game1.activeClickableMenu = new CustomLetterMenu(dataLoader.getLetter(5), "Fisher Song", this.sheetMusic1);

                    }

                    this.showLetter = true;
                    this.readyLetter = false;
                }
            }
            else
            {
                this.showLetter = false;
            }

        }


        private void sendLetters()
        {
            loadObjects();

            if (!this.readyLetter && (!HarpOfYoba.owned && !SheetMusic.owned[0]))
            {
                Game1.mailbox.Enqueue("robinWell");
                this.readyLetter = true;
                this.next_letter = 0;
            }

            if (!this.readyLetter && (!HarpOfYoba.owned && SheetMusic.owned[0]))
            {
                Game1.mailbox.Enqueue("robinWell");
                this.readyLetter = true;
                this.next_letter = 10;
            }

            if (!this.readyLetter && (HarpOfYoba.owned && !SheetMusic.owned[0]))
            {
                Game1.mailbox.Enqueue("robinWell");
                this.readyLetter = true;
                this.next_letter = 11;
            }


            if (!this.readyLetter && HarpOfYoba.owned && !SheetMusic.owned[1] && processIndicators[5])
            {
                Game1.mailbox.Enqueue("robinWell");
                this.readyLetter = true;
                this.next_letter = 1;
            }

            if (!this.readyLetter && HarpOfYoba.owned && !SheetMusic.owned[2] && Game1.player.isMarried())
            {
                Game1.mailbox.Enqueue("robinWell");
                this.readyLetter = true;
                this.next_letter = 2;
            }

            if (!this.readyLetter && HarpOfYoba.owned && !SheetMusic.owned[3] && Game1.player.eventsSeen.Contains(2))
            {
                Game1.mailbox.Enqueue("robinWell");
                this.readyLetter = true;
                this.next_letter = 3;
            }

            if (!this.readyLetter && HarpOfYoba.owned && !SheetMusic.owned[4] && Game1.player.eventsSeen.Contains(191393))
            {
                Game1.mailbox.Enqueue("robinWell");
                this.readyLetter = true;
                this.next_letter = 4;
            }

            if (!this.readyLetter && HarpOfYoba.owned && !SheetMusic.owned[5] && playedAllSongsForFisherman)
            {
                Game1.mailbox.Enqueue("robinWell");
                this.readyLetter = true;
                this.next_letter = 5;
            }



        }


    }
}
 
