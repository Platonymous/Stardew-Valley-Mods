using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using xTile;
using xTile.Layers;
using Microsoft.Xna.Framework.Input;
using xTile.Tiles;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;

namespace CustomNPC
{

    public class CustomNPCMod : Mod, IAssetLoader, IAssetEditor
    {

        private List<NPCBlueprint> NPCS = new List<NPCBlueprint>();
        private List<string> markedAssets = new List<string>();
        private string npcpath;

        private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        private Dictionary<string, Dictionary<string, string>> npcevents = new Dictionary<string, Dictionary<string, string>>();
        private bool waitingForDancePartner = false;
        private List<string> customLocations = new List<string>();
        private Dictionary<string, string> npcmail = new Dictionary<string, string>();
        private Dictionary<string, NPCBlueprint> shoplocations = new Dictionary<string, NPCBlueprint>();
        private Dictionary<CustomBuilding, Map> buildingMaps = new Dictionary<CustomBuilding, Map>();
        private Dictionary<string, Texture2D> buildingTilesheets = new Dictionary<string, Texture2D>();
        private bool waitingForShop = false;
        private bool menuChanging = false;
        private bool npcMapLocationsSet = false;

        public override void Entry(IModHelper helper)
        {
            npcpath = Path.Combine("Mods", "CustomNPC", "Npcs");
            loadNPCs();
            foreach (NPCBlueprint blueprint in NPCS)
                foreach (CustomBuilding building in blueprint.buildings)
                {
                    Map map = Helper.Content.Load<Map>($"NPCs/{blueprint.fileDirectory}/" + building.map);
                    if (!buildingMaps.ContainsKey(building))
                        buildingMaps.Add(building, map);

                    foreach (TileSheet ts in map.TileSheets)
                        if (ts.Id.StartsWith("z"))
                        {
                            string file = new FileInfo(ts.ImageSource).Name;
                            buildingTilesheets.Add(building.map + "_" + ts.Id, Helper.Content.Load<Texture2D>($"NPCs/{blueprint.fileDirectory}/" + file));
                            if (file.StartsWith("spring_")){
                                buildingTilesheets.Add(building.map + "_" + ts.Id + "_summer", Helper.Content.Load<Texture2D>($"NPCs/{blueprint.fileDirectory}/" + file.Replace("spring_","summer_")));
                                buildingTilesheets.Add(building.map + "_" + ts.Id + "_fall", Helper.Content.Load<Texture2D>($"NPCs/{blueprint.fileDirectory}/" + file.Replace("spring_", "fall_")));
                                buildingTilesheets.Add(building.map + "_" + ts.Id + "_winter", Helper.Content.Load<Texture2D>($"NPCs/{blueprint.fileDirectory}/" + file.Replace("spring_", "winter_")));
                            }
                        }
                }


            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;
            LocationEvents.CurrentLocationChanged += LocationEvents_CurrentLocationChanged;
            SaveEvents.AfterReturnToTitle += SaveEvents_AfterReturnToTitle;
        }

        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            MenuEvents.MenuChanged -= MenuEvents_MenuChanged;
        }

        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {
            placeLocations();
            placeNPCs();

            if (Game1.player.isMarried() && isCustomNPC(Game1.player.spouse))
                placeSpouseRoom();

            placeShops();
            receiveMail();

            if (Helper.ModRegistry.IsLoaded("NPCMapLocationsMod") && !npcMapLocationsSet)
            {
                Type apiClass = Type.GetType("NPCMapLocations.MapModMain, NPCMapLocations");

                IPrivateField<Dictionary<string, string>> indoorLocationsF = Helper.Reflection.GetPrivateField<Dictionary<string, string>>(apiClass, "indoorLocations");
                IPrivateField<Dictionary<string, string>> startingLocationsF = Helper.Reflection.GetPrivateField<Dictionary<string, string>>(apiClass, "startingLocations");
                IPrivateField<Dictionary<string, Double[]>> locationVectorsF = Helper.Reflection.GetPrivateField<Dictionary<string, Double[]>>(apiClass, "locationVectors");

                Dictionary<string, string> indoorLocations = indoorLocationsF.GetValue();
                Dictionary<string, string> startingLocations = startingLocationsF.GetValue();
                Dictionary<string, Double[]> locationVectors = locationVectorsF.GetValue();

                foreach (NPCBlueprint blueprint in NPCS)
                {
                    if (!startingLocations.ContainsKey(blueprint.name))
                        startingLocations.Add(blueprint.name, blueprint.map);

                    foreach (CustomRoom room in blueprint.rooms)
                        if (room.mapLocation != null && !locationVectors.ContainsKey(room.name))
                            locationVectors.Add(room.name, room.mapLocation);
                }

                startingLocationsF.SetValue(startingLocations);
                locationVectorsF.SetValue(locationVectors);

                npcMapLocationsSet = true;
            }

            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {

        }

        private void receiveMail()
        {
            Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
            foreach (string mail in npcmail.Keys)
            {
                if (meetsConditions(npcmail[mail]) && !Game1.player.hasOrWillReceiveMail(mail))
                    if (mail.EndsWith("next"))
                        Game1.addMailForTomorrow(mail);
                    else if (mail.EndsWith("nomail"))
                        Game1.player.mailReceived.Add(mail);
                    else
                        Game1.mailbox.Enqueue(mail);

            }
                
        }

        private void placeShops()
        {
            foreach(string shoplocation in shoplocations.Keys)
            {
                NPCBlueprint blueprint = shoplocations[shoplocation];
                Game1.getLocationFromName(shoplocation).setTileProperty(blueprint.shopPosition[0], blueprint.shopPosition[1], "Buildings", "Action", "Message \"NPCShop_" + blueprint.name + "\"");
            }
        }

        private void placeRoom(Map map, string name, bool isOutdoor)
        {
            GameLocation room = new GameLocation(map, name);
            room.IsOutdoors = isOutdoor;
            room.ignoreOutdoorLighting = !isOutdoor;
            Game1.locations.Add(room);
        }

        private void placeLocations()
        {
            foreach (NPCBlueprint blueprint in NPCS)
                foreach (CustomRoom room in blueprint.rooms)
                    if (!doseNotmeetsPlacementCondition(blueprint) && meetsConditions(room.conditions.Replace("ID", "99" + blueprint.name.GetHashCode().ToString().Substring(0, 5))))
                        placeRoom(Helper.Content.Load<Map>($"NPCs/{blueprint.fileDirectory}/" + room.map), room.name, room.isOutdoor);

            foreach (NPCBlueprint blueprint in NPCS)
                foreach (CustomBuilding building in blueprint.buildings)
                    if (building.clear && Game1.getLocationFromName(building.location) is GameLocation location)
                        clearSpace(location, Helper.Content.Load<Map>($"NPCs/{blueprint.fileDirectory}/" + building.map), new Vector2((int)building.position[0], (int)building.position[1]));
                 
            NPC.populateRoutesFromLocationToLocationList();
        }

        private void placeNPCs()
        {

            foreach (NPCBlueprint blueprint in NPCS)
            {
                if (!(Game1.getCharacterFromName(blueprint.name) is NPC))
                {
                    NPC cNPC = new NPC(getSprite(blueprint), getPosition(blueprint), blueprint.map, blueprint.facing, blueprint.name, new Dictionary<int, int[]>(), getPortrait(blueprint), false);
                    cNPC.displayName = blueprint.displayName;
                    string author = blueprint.author == "none" ? "" : " by " + blueprint.author;
                    Monitor.Log(blueprint.name + " " + blueprint.version + author, LogLevel.Info);
                    Monitor.Log("Placed " + blueprint.name + " on Map: " + blueprint.map);
                    Game1.getLocationFromName(blueprint.map).addCharacter(cNPC);

                    if (Game1.player.friendships.ContainsKey(blueprint.name) && Game1.player.getFriendshipLevelForNPC(blueprint.name) > 2000)
                        cNPC.datingFarmer = true;

                    cNPC.dayUpdate(Game1.dayOfMonth);
                    cNPC.updateDialogue();
                    setSpecialDialogue(blueprint);
                    if (cNPC.isMarried())
                    {
                        Game1.newDay = true;
                        cNPC.marriageDuties();
                        Game1.newDay = false;
                    }

                    if (doseNotmeetsPlacementCondition(blueprint))
                        cNPC.isInvisible = true;
                }
            }
        }

        private bool doseNotmeetsPlacementCondition(NPCBlueprint blueprint)
        {
            if (Game1.stats.daysPlayed < blueprint.firstDay)
                return false;

            return !meetsConditions(blueprint.conditions.Replace("ID", "99" + blueprint.name.GetHashCode().ToString().Substring(0, 5)));
        }

        private bool meetsConditions(string conditions)
        {
            if (conditions == "none")
                return true;

            if (conditions.StartsWith("NOT"))
                return !(Helper.Reflection.GetPrivateMethod(Game1.currentLocation, "checkEventPrecondition").Invoke<int>(new object[] { "9999984/" + conditions.Replace("NOT ", "") }) != -1);

            return (Helper.Reflection.GetPrivateMethod(Game1.currentLocation, "checkEventPrecondition").Invoke<int>(new object[] { "9999984/" + conditions }) != -1);
        }

        private void loadNPCs()
        {
            int countNPCs = 0;

            string[] files = parseDir(Path.Combine(Helper.DirectoryPath, "Npcs"), "*.json");

            countNPCs = files.Length;

            foreach (string file in files)
            {
                NPCBlueprint blueprint = Helper.ReadJsonFile<NPCBlueprint>(file);
                blueprint.fileDirectory = new FileInfo(file).Directory.Name;
                if (blueprint.displayName == "none") { blueprint.displayName = blueprint.name; }
                textures.Add("Portrait_" + blueprint.name, getPortrait(blueprint));
                textures.Add("Sprite_" + blueprint.name, getSprite(blueprint).Texture);
                NPCS.Add(blueprint);

                if (blueprint.shopLocation != "none")
                    if (shoplocations.ContainsKey(blueprint.shopLocation))
                        shoplocations[blueprint.shopLocation] = blueprint;
                    else
                        shoplocations.Add(blueprint.shopLocation, blueprint);

                if (blueprint.events != "none")
                    setEvents(blueprint);

                foreach (string loc in blueprint.customLocations)
                    customLocations.Add(loc);
            }

            Monitor.Log(countNPCs + " custom NPCs found.");
        }

        private NPCBlueprint getNPCBlueprintByName(string name)
        {
            return NPCS.Find(b => b.name == name);
        }

        private void placeSpouseRoom()
        {
            if (getNPCBlueprintByName(Game1.player.getSpouse().name) is NPCBlueprint blueprint && blueprint.spouseRoom != "none")
            {
                FarmHouse farmHouse = (FarmHouse)Game1.getLocationFromName("FarmHouse");
                Map farmHouseMap = farmHouse.map;
                Map spouseRoomMap = Helper.Content.Load<Map>($"NPCs/{blueprint.fileDirectory}/" + blueprint.spouseRoom);
                injectIntoMap(spouseRoomMap, farmHouseMap, Game1.player.houseUpgradeLevel < 2 ? new Vector2(blueprint.spouseRoomPos[0], blueprint.spouseRoomPos[1]) : new Vector2(blueprint.spouseRoomPos[2], blueprint.spouseRoomPos[3]), farmHouse, true);
            }
        }

        private void clearSpace(GameLocation targetLocation, Map sourceMap, Vector2 position)
        {
                 for (int x = (int)position.X; x < ((int)position.X + sourceMap.DisplayWidth / Game1.tileSize); x++)
                    for (int y = (int)position.Y; y < ((int)position.Y + sourceMap.DisplayHeight / Game1.tileSize); y++)
                    {
                        Vector2 key = new Vector2(x, y);

                        if (targetLocation.objects.ContainsKey(key))
                            targetLocation.objects.Remove(key);

                        if (targetLocation.terrainFeatures.ContainsKey(key))
                            targetLocation.terrainFeatures.Remove(key);

                        int index = targetLocation.largeTerrainFeatures.FindIndex(l => l.tilePosition == key);

                        if (index > -1)
                            targetLocation.largeTerrainFeatures.RemoveAt(index);
                    }
        }

        private string parseVariables(string data, NPCBlueprint blueprint)
        {            
            string[] data1 = data.Split(new string[]{ "::" },StringSplitOptions.None);
            if (data.Length > 2)
            {
                Dictionary<string, string> dialogues = getDialogue(blueprint, false);
                Dictionary<string, string> animations = getAnimations(blueprint, false);
                Dictionary<string, string> schedules = getScheduleVariables(blueprint);

                for (int i = 1; i < data1.Length; i += 2)
                    if (dialogues.ContainsKey(data1[i]))
                        data1[i] = parseVariables(dialogues[data1[i]], blueprint);
                    else if (animations.ContainsKey(data1[i]))
                        data1[i] = parseVariables(animations[data1[i]].Replace('/', ' '), blueprint);
                    else if (schedules.ContainsKey(data1[i]))
                        data1[i] = parseVariables(schedules[data1[i]], blueprint);
            }
            else
                return data;

            return String.Join("", data1);
        }

        private Dictionary<string,string> getScheduleVariables(NPCBlueprint blueprint)
        {
            return loadCSV(blueprint, blueprint.schedule, blueprint.translateSchedule, 3);
        }

        private string correctSeperator(string data)
        {
            if (!data.Contains(';') && !data.Contains('\t') && data.Contains(','))
                if (data.Contains('"'))
                {
                    string[] parts = data.Split('"');
                    for(int i = 0; i < parts.Length; i++)
                        if(i % 2 == 0)
                            parts[i] = parts[i].Replace(',', ';');

                    data = String.Join("\"", parts);
                }
                else
                    data = data.Replace(',', ';');
            
            return data;
        }

        private string cleanUpString(string data)
        {
            if (data.Length < 2)
                return data;

            data = data.Replace("\t\t\t\t\t\t\t\t", "\t").Replace("\t\t\t\t", "\t").Replace("\t\t", "\t").Replace("\t\t", "\t").Replace(";;;;;;", ";").Replace(";;;;", ";").Replace(";;", ";").Replace(";;", ";").Replace("  ", " ").Replace("\\", "").Replace("''", "'").Replace("''", "'").Replace("''", "'").Replace("\"\"", "\"").Replace("\"\"", "\"").Replace("\"\"", "\"").Replace("\"\"", "\"");
            if (data[0] == '"')
                data = data.Substring(1);

            if (data[data.Length - 1] == '"')
                data = data.Remove(data.Length - 1);

            if (data[0] == '\'')
                data = data.Substring(1);

            if (data[data.Length - 1] == '\'')
                data = data.Remove(data.Length - 1);

            return data;
        }

        private string setBasicVariables(string data, NPCBlueprint blueprint)
        {
            data = data.Replace("<Name>", blueprint.name).Replace("<name>", blueprint.name.ToLower()).Replace("<NAME>", blueprint.name.ToUpper()).Replace("<birthday>", blueprint.birthdaySeason + "_" + blueprint.birthday);
            return data;
        }


        private void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {

            if (waitingForShop)
            {
                waitingForShop = false;
            }

            if (waitingForDancePartner)
            {
                waitingForDancePartner = false;
                MenuEvents.MenuClosed -= MenuEvents_MenuClosed;
            }

            if (Game1.CurrentEvent is Event && Game1.CurrentEvent.isFestival)
            {
                GameLocation temp = (GameLocation)Helper.Reflection.GetPrivateValue<GameLocation>(Game1.CurrentEvent, "temporaryLocation");
                GameEvents.OneSecondTick += GameEvents_OneSecondTick;
            }
            else if (shoplocations.ContainsKey(Game1.currentLocation.name))
            {
                waitingForShop = true;
                menuChanging = false;
                
            }
            

        }

        private void MenuEvents_MenuClosed1(object sender, EventArgsClickableMenuClosed e)
        {
            menuChanging = false;
            MenuEvents.MenuClosed -= MenuEvents_MenuClosed1;
            if (Game1.CurrentEvent is Event evt)
                evt.skipEvent();
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (menuChanging || !(Game1.activeClickableMenu is DialogueBox box) || !(box.getCurrentString().Contains("NPCShop") || box.getCurrentString().Contains("CustomItemGrab")))
                return;

            menuChanging = true;

            box.closeDialogue();

            if (box.getCurrentString().Contains("NPCShop"))
            {
                string shopkeeper = "none";
                shopkeeper = box.getCurrentString().Split('_')[1];

                if (getNPCBlueprintByName(shopkeeper) is NPCBlueprint blueprint && shopConditionsAreMet(blueprint))
                    showShop(blueprint);
                else
                    Game1.activeClickableMenu = new DialogueBox(getClosedString(shopkeeper));
            }
            else if (box.getCurrentString().Contains("CustomItemGrab"))
            {
                List<Item> itemList = new List<Item>();
                string[] items = box.getCurrentString().Split(' ');
                foreach (string item in items)
                {
                    if (item == "CustomItemGrab")
                        continue;

                    string[] itemData = item.Split(':');

                    ForSaleItem newItem = new ForSaleItem();
                    newItem.type = itemData[0];
                    newItem.name = itemData[1];
                    itemList.Add(getItem(newItem));
                   
                }
                ItemGrabMenu itemMenu = new ItemGrabMenu(itemList);
                Game1.activeClickableMenu = itemMenu;
                Helper.ConsoleCommands.Trigger("replace_custom_farming", new string[] { "itemMenu" });
                Helper.ConsoleCommands.Trigger("replace_custom_furniture", new string[] { "itemMenu" });

            }

            MenuEvents.MenuClosed += MenuEvents_MenuClosed1;
            
        }

        private string getClosedString(string shopkeeper)
        {
            NPCBlueprint blueprint = getNPCBlueprintByName(shopkeeper);
            return (getDialogue(blueprint).ContainsKey("closedshop") ? getDialogue(blueprint)["closedshop"] : "Closed");
        }

        private bool shopConditionsAreMet(NPCBlueprint blueprint)
        {
            if ((meetsConditions(blueprint.shopConditions)) && !Game1.getCharacterFromName(blueprint.name).isInvisible && Game1.getCharacterFromName(blueprint.name).getTileLocation() == new Vector2(blueprint.shopkeeperPosition[0], blueprint.shopkeeperPosition[1]))
                return true;

            return false;
        }

        private void showShop(NPCBlueprint blueprint)
        {
            Dictionary<Item, int[]> forSale = new Dictionary<Item, int[]>();

            foreach (ForSaleItem item in blueprint.inventory)
                if (!meetsConditions(item.condition))
                    continue;
                else
                    forSale.Add(getItem(item), new int[] { item.price, int.MaxValue });

            ShopMenu shop = new ShopMenu(forSale, 0, blueprint.name);
            shop.portraitPerson = Game1.getCharacterFromName(blueprint.name);
            shop.setUpShopOwner(blueprint.name);
            shop.potraitPersonDialogue = Game1.parseText(getDialogue(blueprint)["shopText"], Game1.dialogueFont, Game1.tileSize * 5 - Game1.pixelZoom * 4);
            Game1.activeClickableMenu = shop;
            Helper.ConsoleCommands.Trigger("replace_custom_farming", new string[] { "shop" });
            Helper.ConsoleCommands.Trigger("replace_custom_furniture", new string[] { "shop" });
        }

        private void GameEvents_OneSecondTick(object sender, EventArgs e)
        {
            GameEvents.OneSecondTick -= GameEvents_OneSecondTick;
            if (Game1.CurrentEvent.FestivalName == "Flower Dance")
            {
                waitingForDancePartner = true;
                MenuEvents.MenuClosed += MenuEvents_MenuClosed;
            }
            addToFestival();
        }

        private bool isCustomNPC(string name)
        {
            return (NPCS.Find(blueprint => blueprint.name == name) != null);
        }

        private void MenuEvents_MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {

            if (e.PriorMenu is DialogueBox && Game1.player.dancePartner != null && isCustomNPC(Game1.player.dancePartner.name))
            {
                waitingForDancePartner = false;
                MenuEvents.MenuClosed -= MenuEvents_MenuClosed;
                setUpFlowerDanceMainEvent();
            }
        }

        private bool hasTileSheet(Map map, TileSheet tilesheet)
        {
            foreach (TileSheet ts in map.TileSheets)
                if (tilesheet.ImageSource.EndsWith(new FileInfo(ts.ImageSource).Name) || tilesheet.Id == ts.Id)
                    return true;

            return false;
        }

        private void injectIntoMap(Map source, Map target, Vector2 targetPosition, GameLocation location, bool includeEmpty)
        {
            Microsoft.Xna.Framework.Rectangle sourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, source.DisplayWidth / Game1.tileSize, source.DisplayHeight / Game1.tileSize);

            foreach (TileSheet tilesheet in source.TileSheets)
                if (tilesheet.Id.StartsWith("z") && !hasTileSheet(target, tilesheet))
                    target.AddTileSheet(new TileSheet(tilesheet.Id, target, tilesheet.ImageSource, tilesheet.SheetSize, tilesheet.TileSize));

            if (location != null)
                target.LoadTileSheets(Game1.mapDisplayDevice);

            for (Vector2 _x = new Vector2(sourceRectangle.X, targetPosition.X); _x.X < sourceRectangle.Width; _x += new Vector2(1, 1))
            {
                for (Vector2 _y = new Vector2(sourceRectangle.Y, targetPosition.Y); _y.X < sourceRectangle.Height; _y += new Vector2(1, 1))
                {
                    foreach (Layer layer in source.Layers)
                    {
                        Tile sourceTile = layer.Tiles[(int)_x.X, (int)_y.X];
                        Layer mapLayer = target.GetLayer(layer.Id);

                        if (mapLayer == null)
                        {
                            target.InsertLayer(new Layer(layer.Id, target, target.Layers[0].LayerSize, target.Layers[0].TileSize), target.Layers.Count);
                            mapLayer = target.GetLayer(layer.Id);
                        }


                        if (sourceTile == null)
                        {
                            if (includeEmpty)
                            {
                                try
                                {
                                    mapLayer.Tiles[(int)_x.Y, (int)_y.Y] = null;
                                }
                                catch { }

                            }

                            continue;

                        }

                        TileSheet tilesheet = target.GetTileSheet(sourceTile.TileSheet.Id);
                        int index = sourceTile.TileIndex;
                        Tile newTile = new StaticTile(mapLayer, tilesheet, BlendMode.Additive, index);

                        if (sourceTile is AnimatedTile aniTile)
                        {
                            List<StaticTile> staticTiles = new List<StaticTile>();
                            foreach (StaticTile frame in aniTile.TileFrames)
                                staticTiles.Add(new StaticTile(mapLayer, tilesheet, BlendMode.Additive, frame.TileIndex));

                            newTile = new AnimatedTile(mapLayer, staticTiles.ToArray(), aniTile.FrameInterval);

                        }

                        mapLayer.Tiles[(int)_x.Y, (int)_y.Y] = newTile;

                        if (location != null && (layer.Id == "Buildings" || layer.Id == "Front"))
                            Helper.Reflection.GetPrivateMethod(location, "adjustMapLightPropertiesForLamp").Invoke(mapLayer.Tiles[(int)_x.Y, (int)_y.Y].TileIndex, (int)_x.Y, (int)_y.Y, mapLayer.Id);

                        foreach (var prop in sourceTile.Properties)
                            newTile.Properties.Add(prop);
                    }

                }

            }

            target.LoadTileSheets(Game1.mapDisplayDevice);

            if (location is GameLocation)
            {
                location.seasonUpdate(Game1.currentSeason);
                location.DayUpdate(Game1.dayOfMonth);
                location.loadLights();
            }
        }

        private void addToFestival()
        {
            foreach (NPCBlueprint blueprint in NPCS)
            {

                if (blueprint.specialPositions == "none")
                {
                    continue;
                }

                NPC npc = Game1.getCharacterFromName(blueprint.name);
                string festival = Game1.CurrentEvent.FestivalName;

                Dictionary<string, string> specialPositions = getSpecialPositions(blueprint);

                if (specialPositions.ContainsKey(festival) && Game1.currentLocation.characters.Find(a => a.name == npc.name) is null)
                {
                    string[] posString = specialPositions[festival].Split(' ');
                    int x = int.Parse(posString[0]);
                    int y = int.Parse(posString[1]);
                    int face = int.Parse(posString[2]);
                    GameLocation temp = (GameLocation)Helper.Reflection.GetPrivateValue<GameLocation>(Game1.CurrentEvent, "temporaryLocation");
                    if (temp is GameLocation && Game1.CurrentEvent.actors.Find(n => n.name == blueprint.name) is null)
                    {
                        NPC anpc = new NPC(getSprite(blueprint), new Vector2((float)(x * Game1.tileSize), (float)(y * Game1.tileSize)), temp.Name, face, blueprint.name, (Dictionary<int, int[]>)null, getPortrait(blueprint), true);
                        anpc.eventActor = true;
                        Dictionary<string, string> dialogue = getDialogue(blueprint);
                        if (dialogue.ContainsKey(festival))
                        {
                            anpc.setNewDialogue(dialogue[festival], false, false);
                        }

                        Game1.CurrentEvent.actors.Add(anpc);
                    }
                }


            }
        }

        private string getFestivalPositions()
        {
            List<string> positioning = new List<string>();
            foreach (NPCBlueprint blueprint in NPCS)
            {

                if (blueprint.specialPositions == "none")
                {
                    continue;
                }

                NPC npc = Game1.getCharacterFromName(blueprint.name);
                string festival = Game1.CurrentEvent.FestivalName;

                Dictionary<string, string> specialPositions = getSpecialPositions(blueprint);

                if (specialPositions.ContainsKey(festival))
                {
                    string[] posString = specialPositions[festival].Split(' ');
                    int x = int.Parse(posString[0]);
                    int y = int.Parse(posString[1]);
                    int face = int.Parse(posString[2]);
                    positioning.Add(blueprint.name + " " + x + " " + y + " " + face);
                }

            }
            return String.Join(" ", positioning);
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            foreach (NPCBlueprint blueprint in NPCS)
            {
                if (Game1.getCharacterFromName(blueprint.name) is NPC npc)
                {
                    if(Game1.getCharacterFromName(blueprint.name).Schedule is Dictionary<int, SchedulePathDescription> schedule)
                        schedule.Clear();

                    Game1.removeThisCharacterFromAllLocations(npc);
                }

                foreach (CustomRoom room in blueprint.rooms)
                    if (Game1.getLocationFromName(room.name) is GameLocation location)
                        Game1.locations.Remove(location);
            }
        }

        private AnimatedSprite getSprite(NPCBlueprint blueprint)
        {
            AnimatedSprite sprite = new AnimatedSprite(Helper.Content.Load<Texture2D>(Path.Combine("Npcs", blueprint.fileDirectory, blueprint.sprite.Replace("spring_",Game1.currentSeason + "_"))), 0, Game1.tileSize / 4, Game1.tileSize * 2 / 4);
            return sprite;
        }

        private Texture2D getPortrait(NPCBlueprint blueprint)
        {
            Texture2D texture = Helper.Content.Load<Texture2D>(Path.Combine("Npcs", blueprint.fileDirectory, blueprint.portrait));
            return texture;
        }

        private Vector2 getPosition(NPCBlueprint blueprint)
        {
            return new Vector2((float)(blueprint.position[0] * Game1.tileSize), (float)(blueprint.position[1] * Game1.tileSize));
        }

        private Dictionary<string, string> loadCSV(NPCBlueprint blueprint, string file, bool translate, int type = 0)
        {
            string pathToCSV = Path.Combine(npcpath, LocalizedContentManager.CurrentLanguageCode.ToString() + "_" + blueprint.fileDirectory, file);

            if (!translate || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en || !blueprint.translations.Contains<string>(LocalizedContentManager.CurrentLanguageCode.ToString()))
                pathToCSV = Path.Combine(npcpath, blueprint.fileDirectory, file);

            string[] lines = File.ReadLines(pathToCSV).ToArray();
            Random r = new Random();

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            
            foreach (string line in lines)
            {
                string[] parts = setBasicVariables(correctSeperator(line), blueprint).Split(new char[] { '\t' , ';' },StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                    continue;

                List<string> partList = new List<string>(parts);
                partList.Remove(parts[0]);

                if (type == 0)
                    dictionary.Add(cleanUpString(parts[0]), parseVariables(cleanUpString(partList[r.Next(0, partList.Count)]), blueprint));

                if (type == 1)
                    dictionary.Add(cleanUpString(parts[0]), parseVariables(cleanUpString(String.Join("/", partList)), blueprint));

                if (type == 3)
                    dictionary.Add(cleanUpString(parts[0]), cleanUpString(partList[r.Next(0, partList.Count)]));

                if (type == 4)
                    dictionary.Add(cleanUpString(parts[0] + ":" + parts[1]), cleanUpString(partList[r.Next(1, partList.Count)]));

                if (type == 6)
                    dictionary.Add(cleanUpString(parts[0] + ":" + parts[1]), parseVariables(cleanUpString(partList[r.Next(1, partList.Count)]),blueprint));

                if (type == 5)
                {
                    partList.Remove(parts[1]);
                    dictionary.Add(cleanUpString(parts[0] + ":" + parts[1]), parseVariables(cleanUpString(String.Join("/", partList)), blueprint));
                }
            }

            return dictionary;
        }

        private Dictionary<string, string> getSchedule(NPCBlueprint blueprint)
        {
            return loadCSV(blueprint, blueprint.schedule, blueprint.translateSchedule, 1);
        }
        
        private Dictionary<string,string> getMail(NPCBlueprint blueprint)
        {
            Dictionary<string, string> dictionary = loadCSV(blueprint, blueprint.mail, blueprint.translateMail, 6);
            Dictionary<string, string> outDictionary = new Dictionary<string, string>();
            foreach(string conditions in dictionary.Keys)
            {
                string[] parts = conditions.Split(':');
                
                string key = blueprint.name.ToLower() + "_" + parts[1];
                string mail = dictionary[conditions];
                string cond = parts[0];

                if (!cond.StartsWith("default"))
                    if (npcmail.ContainsKey(key))
                        npcmail.Add(key, cond.Replace("ID", "99" + blueprint.name.GetHashCode().ToString().Substring(0, 5)));
                    else
                        npcmail[key] = cond.Replace("ID", "99" + blueprint.name.GetHashCode().ToString().Substring(0, 5)); 

                if(outDictionary.ContainsKey(key))
                    outDictionary[key] = mail;
                else
                    outDictionary.Add(key, mail);
            }

            return outDictionary;
        }

        private Dictionary<string, string> getAnimations(NPCBlueprint blueprint, bool parse = true)
        {
                return loadCSV(blueprint, blueprint.animations, blueprint.translateAnimations, parse ? 0 : 3);
        }

        private Item getItem(ForSaleItem item)
        {
            bool isRecipe = item.type.Contains("Recipe");
            bool isCooking = item.type.Contains("Cooking");

            if (item.index == -1)
            {
                if (isRecipe)
                    return new StardewValley.Object(new CraftingRecipe(item.name, isCooking).createItem().parentSheetIndex, 1, true);

                if (item.type == "Hat")
                    return new Hat(getItemByName("hats", item.name));

                if (item.type == "Ring")
                    return new Ring(getItemByName("ObjectInformation", item.name));

                if (item.type == "Furniture")
                    return new Furniture(getItemByName("Furniture", item.name), Vector2.Zero);

                if (item.type == "Weapon")
                    return new MeleeWeapon(getItemByName("weapons", item.name));

                if (item.type == "Slingshot")
                    return new Slingshot(getItemByName("weapons", item.name));

                if (item.type == "TV")
                    return new TV(getItemByName("Furniture", item.name), Vector2.Zero);

                if (item.type == "Boots")
                    return new Boots(getItemByName("Boots", item.name));

                if (item.type == "Object")
                    return new StardewValley.Object(getItemByName("ObjectInformation", item.name), 1, false);

                if (item.type == "CustomFurniture" && Helper.ModRegistry.IsLoaded("Platonymous.CustomFurniture"))
                {
                    StardewValley.Object standIn = new Chest(true);
                    standIn.name = item.name;
                    standIn.preservedParentSheetIndex = item.price;
                    return standIn;
                }

                if (item.type == "CustomFarming" && Helper.ModRegistry.IsLoaded("Platonymous.CustomFarming"))
                {
                    StardewValley.Object standIn = new Chest(true);
                    standIn.name = item.name;
                    standIn.preservedParentSheetIndex = item.price;
                    return standIn;
                }

            }

            if (item.index != -1)
            {
                if (isRecipe)
                    return new StardewValley.Object(item.index, 1, true);

                if (item.type == "Hat")
                    return new Hat(item.index);

                if (item.type == "Ring")
                    return new Ring(item.index);

                if (item.type == "Furniture")
                    return new Furniture(item.index, Vector2.Zero);

                if (item.type == "Wallpaper")
                    return new Wallpaper(item.index, false);

                if (item.type == "Floor")
                    return new Wallpaper(item.index, true);

                if (item.type == "Boots")
                    return new Boots(item.index);

                if (item.type == "Weapon")
                    return new MeleeWeapon(item.index);

                if (item.type == "Slingshot")
                    return new Slingshot(item.index);

                if (item.type == "TV")
                    return new TV(item.index, Vector2.Zero);

                if (item.type == "FishingRod")
                    return new FishingRod(item.index);

                if (item.type == "CrabPot")
                    return new CrabPot(Vector2.Zero);

                if (item.type == "Object")
                    return new StardewValley.Object(item.index, 1, false);
            }

            return new StardewValley.Object(item.index, 1, false);

        }

        private int getItemByName(string type, string name)
        {
            Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\"+type);
            return (dictionary.Where(d => d.Value.StartsWith(name)).FirstOrDefault()).Key;
        }

        private Dictionary<string, string> getDialogue(NPCBlueprint blueprint, bool parse = true)
        {
            return loadCSV(blueprint, blueprint.dialogue,true, parse ? 0 : 3);
        }

        private Dictionary<string, string> getSpecialPositions(NPCBlueprint blueprint)
        {
            return loadCSV(blueprint, blueprint.specialPositions, false);
        }

        private Dictionary<string, string> getMarriageDialogue(NPCBlueprint blueprint)
        {
            return loadCSV(blueprint, blueprint.marriageDialogue, blueprint.translateMarriage);
        }

        private void setSpecialDialogue(NPCBlueprint blueprint)
        {
            if (!Game1.player.friendships.ContainsKey(blueprint.name))
            {
                NPC npc = Game1.getCharacterFromName(blueprint.name);
                Game1.getCharacterFromName(blueprint.name).CurrentDialogue.Push(new Dialogue(getDialogue(blueprint)["Introduction"], npc));
            }

        }


        public string getGiftTastes(NPCBlueprint blueprint)
        {
            Dictionary<string, string> npcdialogue = getDialogue(blueprint);
            string giftTastes = npcdialogue["loveItemDialogue"] + "/" + String.Join(" ", blueprint.loves) + "/" + npcdialogue["likeItemDialogue"] + " / " + String.Join(" ", blueprint.likes) + "/" + npcdialogue["dislikeItemDialogue"] + " / " + String.Join(" ", blueprint.dislikes) + "/" + npcdialogue["hateItemDialogue"] + " / " + String.Join(" ", blueprint.hates) + "/" + npcdialogue["neutralItemDialogue"] + "//";
            return giftTastes;
        }

        public string getDispositions(NPCBlueprint blueprint)
        {
            return blueprint.age + "/" + blueprint.manners + "/" + blueprint.socialAnxiety + "/" + blueprint.optimism + "/" + blueprint.gender + "/" + blueprint.datable + "/" + blueprint.crush + "/" + blueprint.homeRegion + "/" + blueprint.birthdaySeason + " " + blueprint.birthday + "/" + blueprint.relations + "/" + "null" + " " + blueprint.position[0] + " " + blueprint.position[1] + "/" + blueprint.displayName;
        }



        private void setEvents(NPCBlueprint blueprint)
        {
            Dictionary<string, string> dictionary = dictionary = loadCSV(blueprint, blueprint.events, blueprint.translateEvents, 5);

            foreach (string conditions in dictionary.Keys)
            {
                string[] parts = conditions.Split(':');
                string key = cleanUpString(parts[0]);
                string id = cleanUpString(parts[1]).Replace("ID", "99" + blueprint.name.GetHashCode().ToString().Substring(0, 5));
                string commands = parseVariables(dictionary[conditions],blueprint);

                if (!npcevents.ContainsKey(key))
                    npcevents.Add(key, new Dictionary<string, string>());

                if (!npcevents[key].ContainsKey(id))
                    npcevents[key].Add(id, commands);
                else
                    npcevents[key][id] = commands;
                
            }

       }

        private void setupActorsForFlowerDanceMainEvent()
        {
            Event current = Game1.CurrentEvent;
            GameLocation temp = (GameLocation)Helper.Reflection.GetPrivateValue<GameLocation>(Game1.CurrentEvent, "temporaryLocation");
            string[] split = new string[] { "loadActors", "MainEvent" };
            current.actors.Clear();

            if (current.npcControllers != null)
                current.npcControllers.Clear();

            Dictionary<string, string> source = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");

            for (int x = 0; x < temp.map.GetLayer(split[1]).LayerWidth; ++x)
                for (int y = 0; y < temp.map.GetLayer(split[1]).LayerHeight; ++y)
                    if (temp.map.GetLayer(split[1]).Tiles[x, y] != null)
                    {
                        int index = temp.map.GetLayer(split[1]).Tiles[x, y].TileIndex / 4;
                        int facingDirection = temp.map.GetLayer(split[1]).Tiles[x, y].TileIndex % 4;
                        string key = source.ElementAt<KeyValuePair<string, string>>(index).Key;

                        if (key != null && Game1.getCharacterFromName(key, false) != null)
                            Helper.Reflection.GetPrivateMethod(Game1.CurrentEvent, "addActor").Invoke(new object[] { key, x, y, facingDirection, temp });
                    }
        }

        private void setUpFlowerDanceMainEvent()
        {
            Dictionary<string, string> festivalData = Helper.Reflection.GetPrivateValue<Dictionary<string, string>>(Game1.CurrentEvent, "festivalData");

            Game1.CurrentEvent.eventCommands = festivalData["mainEvent"].Split('/');
            Game1.CurrentEvent.CurrentCommand = 0;
            Game1.CurrentEvent.eventSwitched = true;
            Game1.CurrentEvent.playerControlSequence = false;
            setupActorsForFlowerDanceMainEvent();
            addToFestival();

            if (Game1.IsServer)
                MultiplayerUtility.sendServerToClientsMessage("festivalEvent");

            List<string> stringList1 = new List<string>();
            List<string> stringList2 = new List<string>();
            List<string> source = new List<string>() { "Abigail", "Penny", "Leah", "Maru", "Haley", "Emily" };
            List<string> stringList3 = new List<string>() { "Sebastian", "Sam", "Elliott", "Harvey", "Alex", "Shane" };

            foreach (NPCBlueprint blueprint in NPCS)
                if (blueprint.datable == "datable")
                    if (blueprint.gender == "male")
                        source.Add(blueprint.name);
                    else
                        stringList3.Add(blueprint.name);



            for (int index = 0; index < Game1.numberOfPlayers(); ++index)
            {
                StardewValley.Farmer fromFarmerNumber = Utility.getFarmerFromFarmerNumber(index + 1);

                if (fromFarmerNumber.dancePartner != null)
                    if (fromFarmerNumber.dancePartner.gender == 1)
                    {
                        stringList1.Add(fromFarmerNumber.dancePartner.name);
                        source.Remove(fromFarmerNumber.dancePartner.name);
                        stringList2.Add("farmer" + (object)(index + 1));
                    }
                    else
                    {
                        stringList2.Add(fromFarmerNumber.dancePartner.name);
                        stringList3.Remove(fromFarmerNumber.dancePartner.name);
                        stringList1.Add("farmer" + (object)(index + 1));
                    }
            }

            while (stringList1.Count < 6)
            {
                string who = source.Last<string>();
                if (stringList3.Contains(Utility.getLoveInterest(who)))
                {
                    stringList1.Add(who);
                    stringList2.Add(Utility.getLoveInterest(who));
                }
                source.Remove(who);
            }

            string str = festivalData["mainEvent"];
            str = str.Replace("/loadActors MainEvent", "");

            for (int index = 1; index <= 6; ++index)
                str = str.Replace("Girl" + (object)index, stringList1[index - 1]).Replace("Guy" + (object)index, stringList2[index - 1]);

            Regex regex1 = new Regex("showFrame (?<farmerName>farmer\\d) 44");
            Regex regex2 = new Regex("showFrame (?<farmerName>farmer\\d) 40");
            Regex regex3 = new Regex("animate (?<farmerName>farmer\\d) false true 600 44 45");
            Regex regex4 = new Regex("animate (?<farmerName>farmer\\d) false true 600 43 41 43 42");
            Regex regex5 = new Regex("animate (?<farmerName>farmer\\d) false true 300 46 47");
            Regex regex6 = new Regex("animate (?<farmerName>farmer\\d) false true 600 46 47");

            string input1 = str;
            string replacement = "showFrame $1 12/faceDirection $1 0";
            string input2 = regex1.Replace(input1, replacement);
            string input3 = regex2.Replace(input2, "showFrame $1 0/faceDirection $1 2");
            string input4 = regex3.Replace(input3, "animate $1 false true 600 12 13 12 14");
            string input5 = regex4.Replace(input4, "animate $1 false true 596 4 0");
            string input6 = regex5.Replace(input5, "animate $1 false true 150 12 13 12 14");
            string commands = regex6.Replace(input6, "animate $1 false true 600 0 3");

            Game1.CurrentEvent.eventCommands = commands.Split('/');

        }

        private string[] parseDir(string path, string extension)
        {
            return Directory.GetFiles(path, extension, SearchOption.AllDirectories);
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (markedAssets.Contains(asset.AssetName))
                return false;

            foreach (Map map in buildingMaps.Values)
                foreach (TileSheet ts in map.TileSheets)
                    if (ts.Id.StartsWith("z"))
                    {
                        string file = new FileInfo(ts.ImageSource).Name;
                        if (asset.AssetNameEquals(@"Maps\" + file))
                            return true;

                        if (asset.AssetNameEquals(@"Maps\" + file.Replace("spring_","summer_")))
                            return true;

                        if (asset.AssetNameEquals(@"Maps\" + file.Replace("spring_", "fall_")))
                            return true;

                        if (asset.AssetNameEquals(@"Maps\" + file.Replace("spring_", "winter_")))
                            return true;
                    }
                        

            foreach (string location in customLocations)
                if (asset.AssetNameEquals(@"Data\Events\" + location))
                    return true;

            List<string> assets = new List<string>();

            foreach (NPCBlueprint blueprint in NPCS)
            {
                assets.Add(@"Characters\Dialogue\" + blueprint.name);

                if (blueprint.marriageDialogue != "none")
                    assets.Add(@"Characters\Dialogue\MarriageDialogue" + blueprint.name);

                assets.Add(@"Portraits\" + blueprint.name);
                assets.Add(@"Characters\" + blueprint.name);
                assets.Add(@"Characters\schedules\" + blueprint.name);
            }

            foreach (string a in assets)
                if (asset.AssetNameEquals(a))
                {
                    markedAssets.Add(asset.AssetName);
                    return true;
                }

            return false;
        }

        public T Load<T>(IAssetInfo asset)
        {
            foreach (string location in npcevents.Keys)
                if (asset.AssetNameEquals(@"Data\Events\" + location))
                {
                    Dictionary<string, string> locationEvents = new Dictionary<string, string>();
                    foreach (string key in npcevents[location].Keys)
                        if (!locationEvents.ContainsKey(key))
                            locationEvents.Add(key, npcevents[location][key]);

                    return (T)(object)locationEvents;
                }

            foreach (NPCBlueprint blueprint in NPCS)
            {
                if (blueprint.dialogue != "none" && asset.AssetNameEquals(@"Characters\Dialogue\" + blueprint.name))
                    return (T)(object)getDialogue(blueprint);
                else if (blueprint.marriageDialogue != "none" && asset.AssetNameEquals(@"Characters\Dialogue\MarriageDialogue" + blueprint.name))
                    return (T)(object)getMarriageDialogue(blueprint);
                else if (asset.AssetNameEquals(@"Characters\" + blueprint.name))
                    return (T)(object)textures["Sprite_" + blueprint.name];
                else if (asset.AssetNameEquals(@"Portraits\" + blueprint.name))
                    return (T)(object)textures["Portrait_" + blueprint.name];
                else if (blueprint.schedule != "none" && asset.AssetNameEquals(@"Characters\schedules\" + blueprint.name))
                    return (T)(object)getSchedule(blueprint);
            }

            foreach (CustomBuilding building in buildingMaps.Keys)
                foreach (TileSheet ts in buildingMaps[building].TileSheets)
                    if (ts.Id.StartsWith("z"))
                    {
                        string file = new FileInfo(ts.ImageSource).Name;

                        if (asset.AssetNameEquals(@"Maps\" + file))
                            return (T)(object)buildingTilesheets[building.map + "_" + ts.Id];

                        if (asset.AssetNameEquals(@"Maps\" + file.Replace("spring_", "summer_")))
                            return (T)(object)buildingTilesheets[building.map + "_" + ts.Id + "_summer"];

                        if (asset.AssetNameEquals(@"Maps\" + file.Replace("spring_", "fall_")))
                            return (T)(object)buildingTilesheets[building.map + "_" + ts.Id + "_fall"];

                        if (asset.AssetNameEquals(@"Maps\" + file.Replace("spring_", "winter_")))
                            return (T)(object)buildingTilesheets[building.map + "_" + ts.Id + "_winter"];
                    }
                       

            return (T)(object)null;
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            foreach (string location in npcevents.Keys)
                if (asset.AssetNameEquals(@"Data\Events\" + location))
                    return true;

            foreach (NPCBlueprint blueprint in NPCS)
                foreach (AdditionalWarp warp in blueprint.warps)
                    if (asset.AssetNameEquals(@"Maps\" + warp.mapEntry))
                        return true;

            foreach (NPCBlueprint blueprint in NPCS)
                foreach (CustomBuilding building in blueprint.buildings)
                    if (asset.AssetNameEquals(@"Maps\" + building.location))
                        return true;


            return (asset.AssetNameEquals(@"Strings\StringsFromMaps") || asset.AssetNameEquals(@"Data\EngagementDialogue") || asset.AssetNameEquals(@"Data\mail") || asset.AssetNameEquals(@"Data\animationDescriptions") || asset.AssetNameEquals(@"Data\NPCGiftTastes") || asset.AssetNameEquals(@"Data\NPCDispositions"));
        }

        public void Edit<T>(IAssetData asset)
        {
            if (!markedAssets.Contains(asset.AssetName))
            {

                foreach (NPCBlueprint blueprint in NPCS)
                    foreach (CustomBuilding building in blueprint.buildings)
                        if (asset.AssetNameEquals(@"Maps\" + building.location))
                            injectIntoMap(buildingMaps[building], (asset.Data as Map), new Vector2(building.position[0], building.position[1]), null, building.inlcudeEmpty);

                foreach (NPCBlueprint blueprint in NPCS)
                    foreach (AdditionalWarp warp in blueprint.warps)
                        if (asset.AssetNameEquals(@"Maps\" + warp.mapEntry))
                            (asset.Data as Map).Properties["Warp"] += " " + warp.entry[0] + " " + warp.entry[1] + " " + warp.mapExit + " " + warp.exit[0] + " " + warp.exit[1];

                foreach (string location in npcevents.Keys)
                    if (asset.AssetNameEquals(@"Data\Events\" + location))
                        foreach (string key in npcevents[location].Keys)
                            if (!asset.AsDictionary<string, string>().Data.ContainsKey(key))
                                asset.AsDictionary<string, string>().Data.Add(key, npcevents[location][key]);

                if (asset.AssetNameEquals(@"Strings\StringsFromMaps"))
                    foreach (string shoplocation in shoplocations.Keys)
                        if (!asset.AsDictionary<string, string>().Data.ContainsKey("NPCShop_" + shoplocations[shoplocation].name))
                            asset.AsDictionary<string, string>().Data.Add("NPCShop_" + shoplocations[shoplocation].name, "NPCShop_" + shoplocations[shoplocation].name);

                if (asset.AssetNameEquals(@"Data\EngagementDialogue"))
                    foreach (NPCBlueprint blueprint in NPCS)
                    {
                        Dictionary<string, string> dialogue = getDialogue(blueprint);
                        string key0 = blueprint.name + "0";
                        string key1 = blueprint.name + "1";
                        if (dialogue.ContainsKey("engagement1") && !asset.AsDictionary<string, string>().Data.ContainsKey(key0))
                        {
                            asset
                            .AsDictionary<string, string>()
                            .Data.Add(key0, dialogue["engagement1"]);
                            asset
                            .AsDictionary<string, string>()
                            .Data.Add(key1, dialogue["engagement2"]);
                        }
                    }


                if (asset.AssetNameEquals(@"Data\NPCGiftTastes"))
                    foreach (NPCBlueprint blueprint in NPCS)
                    {
                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                        dictionary.Add(blueprint.name, getGiftTastes(blueprint));

                        foreach (string key in dictionary.Keys)
                            if (!asset.AsDictionary<string, string>().Data.ContainsKey(key))
                                asset
                                .AsDictionary<string, string>()
                                .Data.Add(key, dictionary[key]);
                    }


                if (asset.AssetNameEquals(@"Data\NPCDispositions"))
                    foreach (NPCBlueprint blueprint in NPCS)
                    {
                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                        dictionary.Add(blueprint.name, getDispositions(blueprint));

                        foreach (string key in dictionary.Keys)
                            if (!asset.AsDictionary<string, string>().Data.ContainsKey(key))
                                asset
                                .AsDictionary<string, string>()
                                .Data.Add(key, dictionary[key]);
                    }


                if (asset.AssetNameEquals(@"Data\animationDescriptions"))
                    foreach (NPCBlueprint blueprint in NPCS)
                    {
                        if (blueprint.animations == "none")
                            continue;

                        Dictionary<string, string> dictionary = getAnimations(blueprint,true);

                        foreach (string key in dictionary.Keys)
                            if (!asset.AsDictionary<string, string>().Data.ContainsKey(key))
                                asset
                                .AsDictionary<string, string>()
                                .Data.Add(key, dictionary[key]);
                    }

                if (asset.AssetNameEquals(@"Data\mail"))
                    foreach (NPCBlueprint blueprint in NPCS)
                    {
                        if (blueprint.mail == "none")
                            continue;

                        Dictionary<string, string> mail = getMail(blueprint);
                        foreach (string key in mail.Keys)
                            if (!asset.AsDictionary<string, string>().Data.ContainsKey(key))
                                asset.AsDictionary<string, string>().Data.Add(key, mail[key]);
                    }

                markedAssets.Add(asset.AssetName);
            }
        }
    }
}
