using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;
using xTile.Dimensions;

using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Linq;

using StardewValley.Objects;


namespace InteractiveMapLayer
{
    public class InteractiveMapLayerMod :Mod
    {
        
        Dictionary<Vector2, IPropertyCollection> interactiveProperties;
        Dictionary<string, Dictionary<string, bool>> switches;
        Dictionary<string, dynamic> switchTiles;
        Dictionary<Vector2, Dictionary<string, string>> repeatActions;

        Vector2 playerPosition;
        Vector2 prePlayerPosition;
        GameLocation workingLocation;

        public override void Entry(IModHelper helper)
        {
           
            SaveEvents.AfterLoad += (x, y) => startEventlisteners();
        }

        private void startEventlisteners()
        {
            reset();

            LocationEvents.CurrentLocationChanged += LocationEvents_CurrentLocationChanged;
            GameEvents.UpdateTick += GameEvents_UpdateTick;
            SaveEvents.BeforeSave += (x, y) => reset();
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            

        }

        private void reset()
        {
            switches = new Dictionary<string, Dictionary<string, bool>>();

        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            string key = e.KeyPressed.ToString();

            if(key == "I")
            {
              
            }
        }

        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
         
            if(workingLocation == null || Game1.currentLocation != workingLocation)
            {
                return;
            }
            
            if(playerPosition != null)
            {
                prePlayerPosition = new Vector2(playerPosition.X,playerPosition.Y);
            }

            playerPosition = Game1.player.getTileLocation();

            if(playerPosition != prePlayerPosition)
            {
                handleProperties(playerPosition);
            }

            handleRepeatActions();
        }

        private void handleRepeatActions()
        {
            if(repeatActions.Count == 0)
            {
                return;
            }

            foreach(Vector2 keyV in repeatActions.Keys)
            {
                
                foreach(string condition in repeatActions[keyV].Keys)
                {
                    if (conditionIsMet(condition))
                    {
                        handleActions(repeatActions[keyV][condition], keyV);
                    }
                }

            }

        }

        private void locationSetup()
        {
            interactiveProperties = new Dictionary<Vector2, IPropertyCollection>();
            repeatActions = new Dictionary<Vector2, Dictionary<string, string>>();
            switchTiles = new Dictionary<string, dynamic>();
            workingLocation = Game1.currentLocation;
        }

        private void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
          
            locationSetup();
           

            Map map = Game1.currentLocation.map;

            Layer layer = map.GetLayer("Interactive");

            if (layer == null)
            {
                return;
            }

            IPropertyCollection LayerProperties = layer.Properties;

            for (int x = 0; x < map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < map.Layers[0].LayerHeight; y++)
                {
                       
                    Tile tile = layer.PickTile(new Location(x * Game1.tileSize, y * Game1.tileSize), Game1.viewport.Size);

                    if (tile == null)
                    {
                        continue;
                    }
                    IPropertyCollection tileProperties = tile.Properties;

                    if (tileProperties.Count > 0)
                    {

                        if (tileProperties.ContainsKey("REPEATACTION") && tileProperties["REPEATACTION"] != null)
                        {
                            string condition = "";
                            string action = tileProperties["REPEATACTION"].ToString().ToLower(); ;
                            if (tileProperties.ContainsKey("CONDITION")) { condition = tileProperties["CONDITION"].ToString().ToLower(); }
                            if (!repeatActions.ContainsKey(new Vector2(x, y))) {
                            Dictionary<string, string> repeatActionData =  new Dictionary<string, string>();
                            repeatActionData.Add(condition, action);
                            repeatActions.Add(new Vector2(x, y), repeatActionData);
                            }

                        }

                        if (tileProperties.ContainsKey("TYPE") && tileProperties["TYPE"] != null && tileProperties["TYPE"].ToString().ToLower() == "switch")
                        {
                            string[] switchID = tileProperties["ID"].ToString().ToLower().Split(' ');

                            if (tileProperties["LAYER"] != null) {
                                dynamic switchTile = new { layer = tileProperties["LAYER"].ToString(), position = new Vector2(x,y) };
                                if (!switchTiles.ContainsKey(tileProperties["ID"])){
                                    switchTiles.Add(switchID[0] + " " +switchID[1], switchTile);
                                }
                            }

                            if (!switches.ContainsKey(switchID[0]))
                            {
                                switches.Add(switchID[0], new Dictionary<string, bool>());
                            }

                            if (!switches[switchID[0]].ContainsKey(switchID[1]))
                            {
                                switches[switchID[0]].Add(switchID[1], false);
                            }
                             
                        }

                        interactiveProperties.Add(new Vector2(x, y), tileProperties);
                    }  

                }
                    
            }
        }

        private void changeSwitchTileIndex(string switchgroup, string switchid, int tileShift)
        {
            string switchName = switchgroup + " " + switchid;
            string layer = (string) switchTiles[switchName].layer;
            Vector2 position = (Vector2)switchTiles[switchName].position;
            Tile tile = Game1.currentLocation.map.GetLayer(layer).PickTile(new Location((int)position.X * Game1.tileSize, (int)position.Y * Game1.tileSize), Game1.viewport.Size);
            tile.TileIndex = tile.TileIndex + tileShift;

        }


        private void performSwitchAction(string switchgroup, string switchid, string newstate)
        {
            bool setState = true;
            int tileShift = 1;
            bool playSound = false;
            if (newstate == "off")
            {
                setState = false;
                tileShift = -1;
            }

            

            if (switchid == "all")
            {
                List<string> allSwitches = new List<string>();
                foreach (string sid in switches[switchgroup].Keys)
                {
                    if (switches[switchgroup][sid] != setState)
                    {
                        playSound = true;
                        allSwitches.Add(sid);
                    }

                }

                for(int i = 0; i < allSwitches.Count(); i++)
                {
                    switches[switchgroup][allSwitches[i]] = setState;
                    changeSwitchTileIndex(switchgroup, allSwitches[i], tileShift);
                }
                
            }
            else
            {
                if(switches[switchgroup][switchid] != setState)
                {
                    playSound = true;
                    switches[switchgroup][switchid] = setState;
                    changeSwitchTileIndex(switchgroup, switchid, tileShift);
                }
                
            }

            if (playSound)
            {
                Game1.playSound("coin");
            }

        }

        private bool conditionIsMet(string condition)
        {
            string[] conditions = condition.Split(',');

            foreach (string c in conditions)
            {

                string[] conditionData = c.Split(' ');

                if (conditionData[0] == "switch")
                {
                    bool checkFor = (conditionData[3] == "on") ? true : false;

                    if (conditionData[2] == "all")
                    {
                        foreach (bool check in switches[conditionData[1]].Values)
                        {
                            if (check != checkFor)
                            {
                                return false;
                            }
                        }
                    }
                    else if (switches[conditionData[1]][conditionData[2]] != checkFor)
                    {
                        return false;
                    }

                }
            }

            return true;
        }

        private void performSpawnAction(string type, string obj, string variant, string num, Vector2 position )
        {
            
            if (type == "treasure")
            {
                if (!Game1.currentLocation.objects.ContainsKey(position))
                {
                    Item chestItem = new Hat(1);

                    List<Item> items = new List<Item>();
                    if(obj == "item")
                    {      
                        int parentSheetIndex = int.Parse(variant);
                        int number = int.Parse(num);
                        Monitor.Log("Item: "+number+"x "+parentSheetIndex );
                        chestItem = new StardewValley.Object(parentSheetIndex, number, false, -1, 4);
                        Monitor.Log("Item: " + number + "x " + parentSheetIndex + " -> "+ chestItem.Name);
                        items.Add(chestItem);
                    }

                    Chest chest = new Chest(false);
                    chest.addItem(chestItem);
                   

                Game1.playSound("crystal");
                Game1.currentLocation.objects.Add(position, chest);
                }


            }

        }

        private void handleProperties(Vector2 position)
        {

            if (!interactiveProperties.ContainsKey(position))
            {
                return;
            }

            if (interactiveProperties[position].ContainsKey("ACTION"))
            {

                if (interactiveProperties[position].ContainsKey("CONDITION"))
                {
                    string condition = interactiveProperties[position]["CONDITION"].ToString().ToLower();
                    if (!conditionIsMet(condition))
                    {
                        return;
                    }
                }

                handleActions(interactiveProperties[position]["ACTION"].ToString().ToLower(), position);
             

                


            }
        }

        private void handleActions(string actionProperty, Vector2 position)
        {

            string[] actions = actionProperty.Split(',');

            foreach (string action in actions)
            {
                string[] actionData = action.Split(' ');

                if (actionData[0] == "switch")
                {
                    performSwitchAction(actionData[1], actionData[2], actionData[3]);
                }

                if (actionData[0] == "spawn")
                {
                    
                    performSpawnAction(actionData[1], actionData[2], actionData[3], actionData[4], position);
                }

            }
        }

    }
    

}
