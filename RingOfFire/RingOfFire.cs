using CustomElementHandler;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

using System;
using System.Collections.Generic;

namespace RingOfFire
{
    class RingOfFire : Ring, ISaveElement
    {
        private List<Flame> flames;
        public static bool active;
        private Random rnd;
        public static Texture2D ringTexture;
        private float RotationAngle;
        public static List<Texture2D> flameTextures = new List<Texture2D>();

        public RingOfFire()
            :base(517)
        {
            build();
        }

        private void build()
        {
            name = "Ring of Fire";
            displayName = "Ring of Fire";
            description = "Unleash the Flames";

            RotationAngle = 0;

            rnd = new Random();
            active = false;
            flames = new List<Flame>();
        }

        public override Item getOne()
        {
            return this;
        }

        private void animate()
        {
            int frame = 89;
            bool flip = false;
            if (Game1.player.facingDirection == 3)
            {
                flip = true;
            }

            if (Game1.player.facingDirection == 2)
            {
                frame = 76;
            }
            
            if (Game1.player.facingDirection == 0)
            {
                frame = 55;
            }

            List<FarmerSprite.AnimationFrame> animation = new List<FarmerSprite.AnimationFrame>();
            animation.Add(new FarmerSprite.AnimationFrame(frame, 20, true, flip, (AnimatedSprite.endOfAnimationBehavior)null, false));

            Game1.player.FarmerSprite.setCurrentAnimation(animation.ToArray());
            Game1.player.FarmerSprite.loopThisAnimation = false;
            Game1.player.FarmerSprite.PauseForSingleAnimation = true;
        }

        private List<Vector2> tilesAffected(Vector2 tileLocation, StardewValley.Farmer who)
        {
            List<Vector2> vector2List = new List<Vector2>();
            vector2List.Add(tileLocation);

            if (who.facingDirection == 0)
            {

                vector2List.Add(tileLocation + new Vector2(0.0f, -1f));
                vector2List.Add(tileLocation + new Vector2(0.0f, -2f));

                vector2List.Add(tileLocation + new Vector2(0.0f, -3f));
                vector2List.Add(tileLocation + new Vector2(0.0f, -4f));


                vector2List.RemoveAt(vector2List.Count - 1);
                vector2List.RemoveAt(vector2List.Count - 1);
                vector2List.Add(tileLocation + new Vector2(1f, -2f));
                vector2List.Add(tileLocation + new Vector2(1f, -1f));
                vector2List.Add(tileLocation + new Vector2(1f, 0.0f));
                vector2List.Add(tileLocation + new Vector2(-1f, -2f));
                vector2List.Add(tileLocation + new Vector2(-1f, -1f));
                vector2List.Add(tileLocation + new Vector2(-1f, 0.0f));


                for (int index = vector2List.Count - 1; index >= 0; --index)
                    vector2List.Add(vector2List[index] + new Vector2(0.0f, -3f));

            }
            else if (who.facingDirection == 1)
            {

                vector2List.Add(tileLocation + new Vector2(1f, 0.0f));
                vector2List.Add(tileLocation + new Vector2(2f, 0.0f));


                vector2List.Add(tileLocation + new Vector2(3f, 0.0f));
                vector2List.Add(tileLocation + new Vector2(4f, 0.0f));


                vector2List.RemoveAt(vector2List.Count - 1);
                vector2List.RemoveAt(vector2List.Count - 1);
                vector2List.Add(tileLocation + new Vector2(0.0f, -1f));
                vector2List.Add(tileLocation + new Vector2(1f, -1f));
                vector2List.Add(tileLocation + new Vector2(2f, -1f));
                vector2List.Add(tileLocation + new Vector2(0.0f, 1f));
                vector2List.Add(tileLocation + new Vector2(1f, 1f));
                vector2List.Add(tileLocation + new Vector2(2f, 1f));

                for (int index = vector2List.Count - 1; index >= 0; --index)
                    vector2List.Add(vector2List[index] + new Vector2(3f, 0.0f));

            }
            else if (who.facingDirection == 2)
            {

                vector2List.Add(tileLocation + new Vector2(0.0f, 1f));
                vector2List.Add(tileLocation + new Vector2(0.0f, 2f));

                vector2List.Add(tileLocation + new Vector2(0.0f, 3f));
                vector2List.Add(tileLocation + new Vector2(0.0f, 4f));

                vector2List.RemoveAt(vector2List.Count - 1);
                vector2List.RemoveAt(vector2List.Count - 1);
                vector2List.Add(tileLocation + new Vector2(1f, 2f));
                vector2List.Add(tileLocation + new Vector2(1f, 1f));
                vector2List.Add(tileLocation + new Vector2(1f, 0.0f));
                vector2List.Add(tileLocation + new Vector2(-1f, 2f));
                vector2List.Add(tileLocation + new Vector2(-1f, 1f));
                vector2List.Add(tileLocation + new Vector2(-1f, 0.0f));

                for (int index = vector2List.Count - 1; index >= 0; --index)
                    vector2List.Add(vector2List[index] + new Vector2(0.0f, 3f));

            }
            else if (who.facingDirection == 3)
            {

                vector2List.Add(tileLocation + new Vector2(-1f, 0.0f));
                vector2List.Add(tileLocation + new Vector2(-2f, 0.0f));

                vector2List.Add(tileLocation + new Vector2(-3f, 0.0f));
                vector2List.Add(tileLocation + new Vector2(-4f, 0.0f));

                vector2List.RemoveAt(vector2List.Count - 1);
                vector2List.RemoveAt(vector2List.Count - 1);
                vector2List.Add(tileLocation + new Vector2(0.0f, -1f));
                vector2List.Add(tileLocation + new Vector2(-1f, -1f));
                vector2List.Add(tileLocation + new Vector2(-2f, -1f));
                vector2List.Add(tileLocation + new Vector2(0.0f, 1f));
                vector2List.Add(tileLocation + new Vector2(-1f, 1f));
                vector2List.Add(tileLocation + new Vector2(-2f, 1f));

                for (int index = vector2List.Count - 1; index >= 0; --index)
                    vector2List.Add(vector2List[index] + new Vector2(-3f, 0.0f));

            }

            return vector2List;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber)
        {
            spriteBatch.Draw(ringTexture, location + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)) * scaleSize, new Rectangle?(Game1.getSourceRectForStandardTileSheet(ringTexture, 0, 16, 16)), Color.White * transparency, 0.0f, new Vector2(8f, 8f) * scaleSize, scaleSize * (float)Game1.pixelZoom, SpriteEffects.None, layerDepth);
        }


        public override string getDescription()
        {
            return description;
        }

        public override string DisplayName {
            get => name;
            set => displayName = value;
        }

        public void addFlames()
        {
            if (active)
            {
                float elapsed = (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;

                RotationAngle += elapsed;
                float circle = MathHelper.Pi * 2;
                RotationAngle = RotationAngle % circle;
                StardewValley.Farmer f = Game1.player;
                f.jitterStrength = 4f;
                Vector2 vector = f.getLocalPosition(Game1.viewport) + f.jitter + f.armOffset;
                vector.Y -= (Game1.tileSize / 2);
                if (!f.isMoving())
                {
                    f.xVelocity = 0f;
                    f.yVelocity = 0f;
                    f.MovePosition(Game1.currentGameTime, Game1.viewport, Game1.currentLocation);
                }

                if (f.facingDirection == 3)
                {
                    f.xVelocity = 0.5f;
                }

                if (f.facingDirection == 2)
                {
                    f.yVelocity = 0.5f;

                    vector.X += (Game1.tileSize / 2);
                }

                if (f.facingDirection == 0)
                {
                    f.yVelocity = -0.5f;

                    vector.X += (Game1.tileSize / 2);
                    vector.Y -= Game1.tileSize;
                }

                if (f.facingDirection == 1)
                {

                    f.xVelocity = -0.5f;
                    vector.X += Game1.tileSize;
                }

               for (int i = 0; i < 3; i++)
                {

                    flames.Add(new Flame(flameTextures[rnd.Next(flameTextures.Count)], new Vector2(vector.X, vector.Y), (float)rnd.NextDouble(), (float)(rnd.NextDouble() * RotationAngle), (float)rnd.NextDouble(), ((float)rnd.Next(-100, 100) / 100f), f.facingDirection));
                }
            }
        }

        public void drawFlames()
        {
            foreach (Flame f in flames)
            {
                Game1.spriteBatch.Draw(f.texture, new Rectangle((int)f.position.X, (int)f.position.Y, Game1.tileSize * 2, Game1.tileSize * 2), new Rectangle?(new Rectangle(0, 0, f.texture.Width, f.texture.Height)), Color.White * f.alpha, f.rotation, new Vector2(f.texture.Width / 2, f.texture.Height / 2), SpriteEffects.None, 1f);
            }
   
        }

        public void update()
        {
            if (rnd.NextDouble() > 0)
            {
                addFlames();
            }

            if (active)
            {
                animate();
            }

            List<Flame> remove = new List<Flame>();

            foreach (Flame f in flames)
            {

                f.rotation += 0.2f;
                f.scale += 0.1f;
                int xChange = 8;
                int yChange = 2;
                if (f.direction == 3)
                {
                    f.position.X -= xChange;
                    f.position.Y -= yChange * f.angle;
                }

                if (f.direction == 2)
                {
                    f.position.X -= yChange * f.angle;
                    f.position.Y += xChange;
                }

                if (f.direction == 1)
                {
                    f.position.X += xChange;
                    f.position.Y -= yChange * f.angle;
                }

                if (f.direction == 0)
                {
                    f.position.X -= yChange * f.angle;
                    f.position.Y -= xChange;
                }

                f.alpha -= 0.02f;
                if (f.alpha <= 0)
                {
                    remove.Add(f);
                }
            }

            foreach (Flame r in remove)
            {
                flames.Remove(r);
            }

            if (active)
            {
                List<Vector2> affectedTiles = tilesAffected(Game1.player.getTileLocation(), Game1.player);
                GameLocation location = Game1.currentLocation;
                List<Monster> removeMonsters = new List<Monster>();
                for (int i = 0; i < location.characters.Count; i++)
                {
                    if (location.characters[i] is Monster m && affectedTiles.Contains(m.getTileLocation()))
                    {
                        Rectangle area = new Rectangle(0, 0, 1, 1);

                        if (Game1.player.facingDirection == 1)
                        {
                            area = new Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X, (int)Game1.player.getLocalPosition(Game1.viewport).Y - (Game1.tileSize * 2), Game1.tileSize * 6, Game1.tileSize * 3);

                        }

                        if (Game1.player.facingDirection == 3)
                        {
                            area = new Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X - (Game1.tileSize * 5), (int)Game1.player.getLocalPosition(Game1.viewport).Y - (Game1.tileSize * 2), Game1.tileSize * 6, Game1.tileSize * 3);
                        }

                        if (Game1.player.facingDirection == 0)
                        {
                            area = new Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X - Game1.tileSize, (int)Game1.player.getLocalPosition(Game1.viewport).Y - (Game1.tileSize * 5), Game1.tileSize * 3, Game1.tileSize * 6);
                        }

                        if (Game1.player.facingDirection == 2)
                        {
                            area = new Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X - Game1.tileSize, (int)Game1.player.getLocalPosition(Game1.viewport).Y - Game1.tileSize, Game1.tileSize * 3, Game1.tileSize * 6);
                        }

                        Game1.currentLocation.damageMonster(area, 1, 3, true, 1.5f, 100, 0f, 1f, false, Game1.player);
                        
                       
                    }
                }

               

                foreach (Vector2 v in affectedTiles)
                {
                    

                    TerrainFeature tf = null;
                    StardewValley.Object svo = null;

                    if (location.terrainFeatures.ContainsKey(v))
                    {

                        tf = location.terrainFeatures[v];

                    }

                    if (location.objects.ContainsKey(v))
                    {
                        svo = location.objects[v];
                    }

                  


                    if (svo != null)
                    {

                        switch (svo.parentSheetIndex)
                        {
                            case 0:
                            case 313:
                            case 314:
                            case 315:
                            case 316:
                            case 317:
                            case 318:
                            case 319:
                            case 320:
                            case 321:
                            case 452:
                            case 674:
                            case 675:
                            case 676:
                            case 677:
                            case 678:
                            case 679:
                            case 750:
                            case 784:
                            case 785:
                            case 786:
                            case 792:
                            case 793:
                            case 794:
                                if (rnd.NextDouble() < 0.03)
                                {
                                    location.objects.Remove(v);
                                }
                                break;

                            case 388:
                            case 294:
                            case 295:
                            case 30:
                                if (rnd.NextDouble() < 0.03)
                                {
                                    location.objects.Remove(v);
                                    if (rnd.NextDouble() < 0.3)
                                    {
                                        location.debris.Add(new Debris((Item)new StardewValley.Object(382, 1, false, -1, 0), v * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2))));
                                    }
                                }
                                break;
                        }

                    }

                    if (svo is Torch to)
                    {
                        to.isOn = true;
                    }

                    if (tf is HoeDirt h && h.crop != null)
                    {
                        if (rnd.NextDouble() < 0.03)
                        {
                            h.crop = null;
                        }
                    }

                    if (tf is Grass g)
                    {
                        if (rnd.NextDouble() < 0.05)
                        {
                            g.numberOfWeeds--;
                        }

                        if (g.numberOfWeeds <= 0)
                        {
                            location.terrainFeatures.Remove(v);
                        }

                    }

                    if (tf is FruitTree ft)
                    {
                        ft.performUseAction(new Vector2(v.X + 1, v.Y));
                        if (rnd.NextDouble() < 0.01)
                        {
                            location.terrainFeatures.Remove(v);

                        }
                    }
                    
                    if (tf is Tree t)
                    {
                        t.performUseAction(new Vector2(v.X + 1, v.Y));
                        if (rnd.NextDouble() < 0.15)
                        {
                            t.health--;
                        }

                        if (t.health <= 0)
                        {
                            if (svo != null)
                            {
                                location.debris.Add(new Debris((Item)svo.getOne(), v * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2))));
                                location.objects.Remove(v);
                            }
                            {

                            }
                            location.terrainFeatures.Remove(v);
                            if (rnd.NextDouble() < 0.03)
                            {
                                location.debris.Add(new Debris((Item)new StardewValley.Object(382, rnd.Next(1, 3), false, -1, 0), v * (float)Game1.tileSize + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2))));
                            }
                        }

                    }
                }

            }


        }

        public object getReplacement()
        {
            if(Game1.player.leftRing == this || Game1.player.rightRing == this)
            {
                return new Ring(517);
            }
            else
            {
                return new Chest(true);
            }
            
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            Dictionary<string, string> savedata = new Dictionary<string, string>();
            savedata.Add("name", name);
            return savedata;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            string[] strArray = Game1.objectInformation[517].Split('/');
            category = -96;
            name = strArray[0];
            price = Convert.ToInt32(strArray[1]);
            indexInTileSheet = 517;
            uniqueID = Game1.year + Game1.dayOfMonth + Game1.timeOfDay + this.indexInTileSheet + Game1.player.getTileX() + (int)Game1.stats.MonstersKilled + (int)Game1.stats.itemsCrafted;
            RingOfFireMod.helper.Reflection.GetPrivateMethod(this, "loadDisplayFields").Invoke();
            build();
        }
    }
}
