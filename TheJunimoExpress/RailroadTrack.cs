using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley.Tools;
using StardewValley.TerrainFeatures;
using StardewValley;

using System.Collections.Generic;

namespace TheJunimoExpress
{
    class RailroadTrack : TerrainFeature
    {
    
        public int textureRow;
        public int textureCol;
        public int textureDirection = 0;
        public int whichFloor = 0;
        public static SerializableDictionary<GameLocation,List<Vector2>> trackList = new SerializableDictionary<GameLocation, List<Vector2>>();
        public GameLocation location;
        public Vector2 positon;
        public Vector2 tileLocation;


        public RailroadTrack()
        {
            this.loadSprite();
            if (Flooring.drawGuide != null)
                return;
            Flooring.populateDrawGuide();
            
        }

        public RailroadTrack(int row, int col)
      : this()
    {
 
            this.textureRow = row;
            this.textureCol = col;
            this.textureDirection = 0;
    

        }

        public override Rectangle getBoundingBox(Vector2 tileLocation)
        {
         
            return new Rectangle((int)((double)tileLocation.X * (double)Game1.tileSize), (int)((double)tileLocation.Y * (double)Game1.tileSize), Game1.tileSize, Game1.tileSize);
        }

        public override void loadSprite()
        {
       
        }

        public override void doCollisionAction(Rectangle positionOfCollider, int speedOfCollision, Vector2 tileLocation, Character who, GameLocation location)
        {
            base.doCollisionAction(positionOfCollider, speedOfCollision, tileLocation, who, location);
            if (who == null || !(who is Farmer) || !(location is Farm))
                return;
            (who as Farmer).temporarySpeedBuff = 0.1f;
        }

        public override bool isPassable(Character c = null)
        {
            return true;
        }

        public string getFootstepSound()
        {

            return "stoneStep";

        }

        public override bool performToolAction(Tool t, int damage, Vector2 tileLocation, GameLocation location = null)
        {
            if (location == null)
                location = Game1.currentLocation;
            if (t == null && damage <= 0 || damage <= 0 && !(t.GetType() == typeof(Pickaxe)) && !(t.GetType() == typeof(Axe)))
                return false;
            Game1.createRadialDebris(location, this.whichFloor == 0 ? 12 : 14, (int)tileLocation.X, (int)tileLocation.Y, 4, false, -1, false, -1);
           
            
           Game1.playSound("hammer");

            trackList[this.location].Remove(this.positon);
           
            location.debris.Add(new Debris((Item)new StardewValley.Object(388, 1, false, -1, 0), tileLocation * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2))));

            return true;
        }

        private bool doesTileCountForDrawing(Vector2 surroundingLocations)
        {
            return false;
        }

       
        public void updateDirection()
        {
  
            if (trackList.ContainsKey(this.location)) {
                if (!trackList[this.location].Contains(this.positon))
                {
                    trackList[this.location].Add(this.positon);
                }
                List<Vector2> sVectors = new List<Vector2>();
                sVectors.Add(new Vector2(-1f, 0.0f) + this.positon);
                sVectors.Add(new Vector2(1f, 0.0f) + this.positon);
                sVectors.Add(new Vector2(0.0f, 1f) + this.positon);
                sVectors.Add(new Vector2(0.0f, -1f) + this.positon);

                bool vRight = false;
                bool vLeft = false;
                bool vTop = false;
                bool vBottom = false;

                for (int i = 0; i < trackList[this.location].Count;i++)
                {
                    if (trackList[this.location][i] == sVectors[0]) {

                        vLeft = true;

                    }

                    if (trackList[this.location][i] == sVectors[1])
                    {

                        vRight = true;

                    }

                    if (trackList[this.location][i] == sVectors[2])
                    {

                        vBottom = true;

                    }

                    if (trackList[this.location][i] == sVectors[3])
                    {

                        vTop = true;

                    }
                }

                this.textureDirection = 0;

                if (vTop || vBottom) { 
                this.textureDirection = 1;
                }

                if (vTop && vLeft)
                {
                    this.textureDirection = 2;
                }

                if (vBottom && vLeft)
                {
                    this.textureDirection = 3;
                }

                if (vTop && vRight)
                {
                    this.textureDirection = 4;
                }

                if (vBottom && vRight)
                {
                    this.textureDirection = 5;
                }

                if(vRight && vLeft && vBottom)
                {
                    this.textureDirection = 7;
                }

                if (vRight && vLeft && vTop)
                {
                    this.textureDirection = 8;
                }

                if (vBottom && vTop && vLeft)
                {
                    this.textureDirection = 9;
                }

                if (vBottom && vTop && vRight)
                {
                    this.textureDirection = 10;
                }

                if (vBottom && vTop && vRight && vLeft)
                {
                    this.textureDirection = 6;
                }

            }
            else
            {
                trackList.Add(this.location,new List<Vector2>());
                trackList[this.location].Add(this.positon);

            }


        }

        public override void draw(SpriteBatch spriteBatch, Vector2 tl)
        {
            this.positon = tl;
            this.tileLocation = tl;
            this.location = Game1.currentLocation;
            this.updateDirection();

            int addCol = this.textureDirection;
            int addRow = 0;
       
            spriteBatch.Draw(StardewValley.Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * (float)Game1.tileSize, tileLocation.Y * (float)Game1.tileSize)), new Rectangle?(new Rectangle((this.textureCol+addCol) * 16, (this.textureRow+addRow) * 16, 16, 16)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 1E-09f);
        }

        public override bool tickUpdate(GameTime time, Vector2 tileLocation)
        {
            return false;
        }

        public override void dayUpdate(GameLocation environment, Vector2 tileLocation)
        {
        }

        public override bool seasonUpdate(bool onLoad)
        {
            return false;
        }

    }


}


