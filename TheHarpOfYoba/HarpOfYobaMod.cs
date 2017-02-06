using System;

using System.Collections.Generic;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Locations;
using StardewValley.Buildings;

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
        public static string savstring = "";
        private static int numSheets = 6;
        private static List<SheetMusic> sheetList = new List<SheetMusic>(numSheets);


        public override void Entry(IModHelper helper)
        {
            this.has_harp = false;
            this.showLetter = false;
            this.readyLetter = false;
            this.next_letter = 0;
            this.playedAllSongsForFisherman = false;
            this.once = true;


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
            loadObjects();
            //LoadAndReplace(savstring);
            if (savstring != "") { 
            this.Monitor.Log("Loading: " + LoadAndReplace(savstring));
            }
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
            loadObjects();
            checkProcess();
            //dataLoader.saveSavStringToFile(this.RemoveAndSave(), Game1.uniqueIDForThisGame, Game1.player.name);
           this.Monitor.Log("Saving: " + dataLoader.saveSavStringToFile(this.RemoveAndSave(), Game1.uniqueIDForThisGame, Game1.player.name));


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

            string[] savedata = Regex.Split(save,"-S-");

            string[] saveHarpDataSet = Regex.Split(savedata[1], "-");
            string saveHarpAttachement = savedata[2];

            if (saveHarpAttachement != "none")
            {
                newHarp.sheet = sheetList[int.Parse(saveHarpAttachement)];
                SheetMusic.owned[int.Parse(saveHarpAttachement)] = true;
            }

            foreach (string saveHarp in saveHarpDataSet) { 
                string[] saveHarpPlacement = Regex.Split(saveHarp,"/");

               
                if (saveHarpPlacement[2] == "items")
                {
                    if (saveHarpPlacement[0] == "harp")
                    {
                        Game1.player.items[int.Parse(saveHarpPlacement[3])] = this.newHarp;
                        HarpOfYoba.owned = true;
                    }
                    else if (saveHarpPlacement[0] == "sheet")
                    {
                        Game1.player.items[int.Parse(saveHarpPlacement[3])] = sheetList[int.Parse(saveHarpPlacement[1])];
                        SheetMusic.owned[int.Parse(saveHarpPlacement[1])] = true;
                    }

                    } else if(saveHarpPlacement[2] == "chest")
                {
                    GameLocation gl;

                    if (saveHarpPlacement[3] == "GL")
                    {
                        gl = Game1.getLocationFromName(saveHarpPlacement[4]);
                       
                    }
                    else if (saveHarpPlacement[3] == "BGL")
                    {
                        BuildableGameLocation bgl = (BuildableGameLocation)Game1.getLocationFromName(saveHarpPlacement[4]);
                        gl = bgl.buildings[int.Parse(saveHarpPlacement[5])].indoors;

                    }
                    else
                    {
                        gl = new GameLocation();
                    }

                    Chest che = (Chest)gl.objects[new Vector2(int.Parse(saveHarpPlacement[6]), int.Parse(saveHarpPlacement[7]))];

                    if (saveHarpPlacement[0] == "harp")
                    {
                        che.items[int.Parse(saveHarpPlacement[8])] = this.newHarp;
                        HarpOfYoba.owned = true;
                    }
                    else if (saveHarpPlacement[0] == "sheet")
                    {
                        che.items[int.Parse(saveHarpPlacement[8])] = sheetList[int.Parse(saveHarpPlacement[1])];
                        SheetMusic.owned[int.Parse(saveHarpPlacement[1])] = true;
                    }


                }

            }


            string[] processData = Regex.Split(savedata[3],"-");

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


            string harp_storage = "";

            string harp_attachement = "none";

            
            if (this.newHarp.sheet != null)
            {
                harp_attachement = this.newHarp.sheet.pos.ToString();
                SheetMusic.owned[this.newHarp.sheet.pos] = false;
                this.newHarp.sheet = null;
            }


            for (int index = 0; index < Game1.player.items.Count; ++index)
            {
                if (Game1.player.items[index] is HarpOfYoba)
                {
                    harp_storage += $"-harp/0/items/{index}";

                    HarpOfYoba.owned = false;
                    Game1.player.items[index] = new Hat(1); ;

                }
                else if (Game1.player.items[index] is SheetMusic)
                {
                    SheetMusic sm = (SheetMusic) Game1.player.items[index];
                    harp_storage += $"-sheet/{sm.pos}/items/{index}";
                    SheetMusic.owned[sm.pos] = false;
                    Game1.player.items[index] = new Hat(1);
                }
            }
          
                foreach (GameLocation gl in Game1.locations)
                {
                    

                    foreach (Vector2 KeyV in gl.objects.Keys)
                    {
                       
                        if (gl.objects[KeyV] is Chest)
                        {
                            Chest c = (Chest)gl.objects[KeyV];
                        for (int index = 0; index < c.items.Count; ++index)
                        {
                            if (c.items[index] is HarpOfYoba)
                            {
                                harp_storage += $"-harp/0/chest/GL/{gl.name}/0/{KeyV.X}/{KeyV.Y}/{index}";

                                HarpOfYoba.owned = false;

                                c.items[index] = new Hat(1);

                            }
                            else if (c.items[index] is SheetMusic)
                            {
                                SheetMusic sm = (SheetMusic)c.items[index];
                                if (sm.pos.ToString() != harp_attachement)
                                {
                                    harp_storage += $"-sheet/{sm.pos}/chest/GL/{gl.name}/0/{KeyV.X}/{KeyV.Y}/{index}";
                                    c.items[index] = new Hat(1);
                                }
                                SheetMusic.owned[sm.pos] = false;

                            }
                        }
                        }

                    }

                }




            foreach (GameLocation gl in Game1.locations)
            {

                if (gl is BuildableGameLocation)
                {

                    BuildableGameLocation bgl = (BuildableGameLocation)gl;
                    
                    for (int bIndex = 0; bIndex < bgl.buildings.Count; bIndex++)
                    {

                        GameLocation bid = bgl.buildings[bIndex].indoors;
                        if(bid == null ) { continue; }
                        if (bid.objects == null) { continue; }
                        foreach (Vector2 KeyV in bid.objects.Keys)
                        {
                            if (bid.objects[KeyV] == null) { continue; }
                            if (bid.objects[KeyV] is Chest)
                            {
                                Chest c = (Chest)bid.objects[KeyV];
                                for (int index = 0; index < c.items.Count; ++index)
                                {
                                    if (c.items[index] is HarpOfYoba)
                                    {
                                        harp_storage += $"-harp/0/chest/BGL/{gl.name}/{bIndex}/{KeyV.X}/{KeyV.Y}/{index}";

                                        HarpOfYoba.owned = false;
                                        c.items[index] = new Hat(1);

                                    }
                                    else if (c.items[index] is SheetMusic)
                                    {
                                        SheetMusic sm = (SheetMusic)c.items[index];
                                        if (sm.pos.ToString() != harp_attachement)
                                        {
                                            harp_storage += $"-sheet/{sm.pos}/chest/BGL/{gl.name}/{bIndex}/{KeyV.X}/{KeyV.Y}/{index}";
                                            c.items[index] = new Hat(1);
                                        }
                                        SheetMusic.owned[sm.pos] = false;

                                    }
                                }
                            }

                        }

                    }

                }
            }
            
            if (harp_storage == "")
            {
                harp_storage = "-none";
            }
           
            savstring += "-S" + harp_storage + "-S-" + harp_attachement +"-S";


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
 
