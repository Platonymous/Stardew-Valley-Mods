using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;


namespace SwimSuit
{
    public class SwimSuitMod : Mod
    {

        private SConfig config;

        public override void Entry(IModHelper helper)
        {
            ControlEvents.KeyPressed += ControlEvents_KeyPressed;
            config = Helper.ReadConfig<SConfig>(); 
        }

        private void ControlEvents_KeyPressed(object sender, EventArgsKeyPressed e)
        {


            if(e.KeyPressed == config.swimKey)
            {

                List<Vector2> tiles = getSurroundingTiles();
                Vector2 jumpLocation = Vector2.Zero;

                bool nextToWater = false;
                try
                {
                    nextToWater = Game1.currentLocation.waterTiles[(int)tiles.Last().X, (int)tiles.Last().Y];
                }
                catch (Exception exeption)
                {

                }

                bool nextToBarrier = Game1.currentLocation.isTilePassable(new Location((int)tiles.Last().X, (int)tiles.Last().Y), Game1.viewport); ;

                foreach (Vector2 tile in tiles)
                {
                    bool isWater = false;
                    bool isPassable = false;
                    try
                    {
                        isPassable = Game1.currentLocation.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport);
                        isWater = Game1.currentLocation.waterTiles[(int)tile.X, (int)tile.Y];
                    }
                    catch (Exception exeption)
                    {

                    }

                    if (Game1.player.swimming && !isWater && isPassable && !nextToBarrier)
                    {
                        jumpLocation = tile;
      
                    }

                    if (!Game1.player.swimming && isWater && isPassable && nextToWater)
                    {
                        jumpLocation = tile;
         
                    }


                }
               
                if(jumpLocation != Vector2.Zero)
                {
                    if (Game1.player.swimming)
                    {
                        Game1.player.changeOutOfSwimSuit();
                        Game1.player.swimming = false;
                        Game1.player.position = new Vector2(jumpLocation.X * Game1.tileSize, jumpLocation.Y * Game1.tileSize);
                    }
                    else
                    {
                        Game1.player.changeIntoSwimsuit();
                        Game1.player.swimming = true;
                        Game1.player.position = new Vector2(jumpLocation.X * Game1.tileSize, jumpLocation.Y * Game1.tileSize);
                    }
                    
                }
                
            }
        }

        private List<Vector2> getSurroundingTiles()
        {
            List<Vector2> tiles = new List<Vector2>();
            int dir = Game1.player.facingDirection;
            if (dir == 1)
            {

                for (int i = 8; i > 0; i--)
                {
                    tiles.Add(Game1.player.getTileLocation() + new Vector2(i, 0));
                }

            }

            if (dir == 2)
            {

                for (int i = 8; i > 0; i--)
                {
                    tiles.Add(Game1.player.getTileLocation() + new Vector2(0, i));
                }

            }

            if (dir == 3)
            {

                for (int i = 8; i > 0; i--)
                {
                    tiles.Add(Game1.player.getTileLocation() - new Vector2(i, 0));
                }

            }

            if (dir == 0)
            {

                for (int i = 8; i > 0; i--)
                {
                    tiles.Add(Game1.player.getTileLocation() - new Vector2(0, i));
                }

            }

            return tiles;

        }
    }
}
