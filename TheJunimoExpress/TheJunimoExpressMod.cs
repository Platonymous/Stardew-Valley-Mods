using System;
using System.Linq;
using System.Collections.Generic;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.TerrainFeatures;

using Microsoft.Xna.Framework;
using StardewValley.Objects;
using Microsoft.Xna.Framework.Graphics;

namespace TheJunimoExpress
{
    public class TheJunimoExpressMod : Mod
    {

        public int textureOriginTracks = 0;
        public int textureOriginHelper = 0;
        public int[] tempA;
        public bool started = false;
        private static LoadData DataLoader = new LoadData();
       

        public override void Entry(IModHelper helper)
        {
           
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            SaveEvents.AfterSave += SaveEvents_AfterSave;
            GameEvents.HalfSecondTick += GameEvents_HalfSecondTick; 
            GameEvents.UpdateTick += GameEvents_UpdateTick;
            LocationEvents.CurrentLocationChanged += LocationEvents_CurrentLocationChanged;
            TimeEvents.TimeOfDayChanged += TimeEvents_TimeOfDayChanged;

        }

        private void reloadContent()
        {
            this.started = true;
            start();

            string savestring = DataLoader.loadSavStringFromFile(Game1.uniqueIDForThisGame, Game1.player.name);
            DataLoader.LoadFromString(savestring);
        }

        private void SaveEvents_AfterSave(object sender, EventArgs e)
        {
            reloadContent();
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            string save = DataLoader.SaveAndRemove(Game1.uniqueIDForThisGame, Game1.player.name);
            this.started = false;
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            reloadContent();
        }

        private void TimeEvents_TimeOfDayChanged(object sender, EventArgsIntChanged e)
        {
           
            int time = e.PriorInt;

            if (time % 100 == 0 && !Game1.isDarkOut())
            {

                for (int i = 0; i < LoadData.objectlist.Count(); i++)
                {
                    if (LoadData.objectlist[i] is JunimoHelper)
                    {
                        (LoadData.objectlist[i] as JunimoHelper).helperAction();
                    }
                }
            }
        }

        private void LocationEvents_CurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {

            LoadData.previousLocation =  e.PriorLocation;
        }
        
        private void GameEvents_UpdateTick(object sender, EventArgs e)
        {
           
            for (int i = 0; i < LoadData.objectlist.Count(); i++)
            {
                if (LoadData.objectlist[i] is JunimoHelper) { 
                (LoadData.objectlist[i] as JunimoHelper).run();
                }
            }
        }
   
        private void GameEvents_HalfSecondTick(object sender, EventArgs e)
        {
            if (Game1.hasLoadedGame)
            {
                this.transform();
            }
        }


        private void transform()
        {

            GameLocation gl = Game1.currentLocation;

            List<Vector2> vl = new List<Vector2>();
            List<Vector2> vl2 = new List<Vector2>();
            List<Vector2> vl3 = new List<Vector2>();
            List<Vector2> vl4 = new List<Vector2>();

            Dictionary<Vector2, StardewValley.Object> allObjects = gl.objects;

            List<Vector2> sVectors = new List<Vector2>();
            Vector2 check = new Vector2(0, 0);
            Vector2 pPosition = new Vector2(Game1.player.getTileLocation().X, Game1.player.getTileLocation().Y);
            for (float i = -3; i <= 3.0; i++)
            {
                for (float j = -3; j <= 3.0; j++)
                {
                    check = (new Vector2(i, j) + pPosition);
                    if (allObjects.ContainsKey(check) && allObjects[check].name == "Tracks")
                    {
                        vl.Add(check);
                    }
                    if (allObjects.ContainsKey(check) && allObjects[check].name == "Junimo Helper")
                    {
                        vl2.Add(check);
                    }

                    if (allObjects.ContainsKey(check) && allObjects[check] is Chest && (allObjects[check] as Chest).items.Count == 1 && !(allObjects[check] is LinkedChest) && (allObjects[check] as Chest).items.FindIndex( x => (x is StardewValley.Object) && x.Name == "Junimo Helper") >= 0)
                    {
                        vl4.Add(check);
                    }


                    if (allObjects.ContainsKey(check) && allObjects[check] is JunimoHelper)
                    {
                        if ((allObjects[check] as JunimoHelper).targetChest == null) { 
                        vl3.Add(check);
                        }
                    }
                }
            }


            for (int i = 0; i < vl4.Count; i++)
            {
                LinkedChest newChest = new LinkedChest(true);
                (allObjects[vl4[i]] as Chest).items.RemoveAt(0);
                allObjects.Remove(vl4[i]);
                allObjects.Add(vl4[i], newChest);
                newChest.tileLocation = vl4[i];
                newChest.linkedJunimo = null;
                newChest.objectID = -1;
                LoadData.objectlist.Add(newChest);
                Game1.activeClickableMenu.exitThisMenu();
            }

                for (int i = 0; i < vl.Count; i++)
            {
                int row = (int)Math.Floor(this.textureOriginTracks / 24.0);
                int col = this.textureOriginTracks % 24;
              
                
                if (!gl.terrainFeatures.ContainsKey(vl[i]) && Game1.currentLocation.name == "Farm") {
                TerrainFeature t = new RailroadTrack(row, col);
                gl.terrainFeatures.Add(vl[i], t);
                    LoadData.terrainFeatureList.Add(t);
                }
                else
                {
                   Game1.player.addItemToInventory(gl.objects[vl[i]]);
                }
                gl.objects.Remove(vl[i]);

            }
        

            for (int i = 0; i < vl2.Count; i++)
            {
               
                
                    gl.objects.Remove(vl2[i]);
                    JunimoHelper junimo = new JunimoHelper(vl2[i], this.textureOriginHelper, false);
                    gl.objects.Add(vl2[i], junimo );
                    
                    LoadData.objectlist.Add(junimo);

            }
            for (int i = 0; i < vl3.Count; i++)
            {
                (gl.objects[vl3[i]] as JunimoHelper).checkForChest();
            }

        }

        public void start()
        {
            DataLoader = new LoadData();

            Game1.bigCraftableSpriteSheet = Game1.content.Load<Texture2D>("TileSheets\\Craftables");
            Game1.objectSpriteSheet = Game1.content.Load<Texture2D>("Maps\\springobjects");
            Game1.bigCraftablesInformation = Game1.content.Load<Dictionary<int, string>>("Data\\BigCraftablesInformation");
            Game1.objectInformation = Game1.content.Load<Dictionary<int, string>>("Data\\ObjectInformation");
            if (Game1.player.craftingRecipes.ContainsKey("Tracks"))
            {
                Game1.player.craftingRecipes.Remove("Tracks");
            }
            if (Game1.player.craftingRecipes.ContainsKey("Junimo Helper"))
            {
                Game1.player.craftingRecipes.Remove("Junimo Helper");
            };

            int i = DataLoader.setTracksTextures();
            this.textureOriginTracks = i;
            int trackObjectID = i;
            string trackInformation = "Tracks/50/-300/Crafting -24/Tracks/Driving the train doesn't set its course. The real job is laying the track.";

                Game1.objectInformation.Add(trackObjectID, trackInformation);
            CraftingRecipe.craftingRecipes.Add("Tracks", "388 30 335 2/Home/" + trackObjectID.ToString() + " 10/false/null");
            
            Game1.player.craftingRecipes.Add("Tracks", 0);
            
            int j = DataLoader.setHelperTextures();
            this.textureOriginHelper = j;
            int helperObjectID = j;
            int helperRecipeID = trackObjectID + 74;
         
            string helperInformation = "Junimo Helper/50/-300/Crafting -24/Junimo Helper/Get by with a little help from your friends.";          

            Game1.objectInformation.Add(helperRecipeID, helperInformation);
            CraftingRecipe.craftingRecipes.Add("Junimo Helper", "268 50/Home/" + helperRecipeID.ToString() + "/false/null");
            Game1.player.craftingRecipes.Add("Junimo Helper", 0);

            LoadData.craftables.Add(trackObjectID);
            LoadData.craftables.Add(helperRecipeID);
            LoadData.craftables.Add(textureOriginTracks);
            LoadData.craftables.Add(textureOriginHelper);
            LoadData.recipes.Add("Tracks");
            LoadData.recipes.Add("Junimo Helper");
        }

        

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {
            

            if (e.KeyPressed.ToString() == "I")
            {

            }


            if (e.KeyPressed.ToString() == "O")
            {

     
            }

            
            if (e.KeyPressed.ToString() == "P")
            {
                
     
            }

        }

        
    }
}
