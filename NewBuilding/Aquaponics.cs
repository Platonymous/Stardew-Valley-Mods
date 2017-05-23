using CustomElementHandler;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;

using System.Collections.Generic;

using xTile;

namespace Aquaponics
{
    class Aquaponics : Building, ISaveElement
    {
        private GameLocation location;  
        
        public Aquaponics()
            :base()
        {

        }

        public Aquaponics(Vector2 position, BuildableGameLocation location)
            : base()
        {

            build(position, location,4);
            if (position != Vector2.Zero)
            {
                AquaponicsLocation apl = new AquaponicsLocation(AquaponicsMod.helper.Content.Load<Map>(@"assets\greenhouseMap.xnb", ContentSource.ModFolder), nameOfIndoors, (BuildableGameLocation)location);
                indoors = apl;
            }
            
        }

        private void build(Vector2 position, BuildableGameLocation location, int daysLeft)
        {
            this.location = location;
            tileX = (int) position.X;
            tileY = (int) position.Y;
            tilesWide = 7;
            tilesHigh = 3;
            humanDoor = new Point(2, 2);
            animalDoor = new Point(-1, -1);
            texture = AquaponicsMod.helper.Content.Load<Texture2D>(@"assets\greenhouse.xnb", ContentSource.ModFolder);
            buildingType = "Aquaponics";
            baseNameOfIndoors = buildingType;
            nameOfIndoorsWithoutUnique = baseNameOfIndoors;
            nameOfIndoors = baseNameOfIndoors + "_" + location.name + "_" + tileX + "_" + tileY + "_" + location.buildings.FindAll(x => x is Aquaponics).Count;
            maxOccupants = -1;
            magical = false;
            daysOfConstructionLeft = daysLeft;
            owner = Game1.player.uniqueMultiplayerID;
        }

        public override void draw(SpriteBatch b)
        {
            if (this.daysOfConstructionLeft > 0)
            {
                this.drawInConstruction(b);
            }
            else
            {
                this.drawShadow(b, -1, -1);
                b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(this.tileX * Game1.tileSize), (float)(this.tileY * Game1.tileSize + this.tilesHigh * Game1.tileSize))), new Rectangle?(this.texture.Bounds), this.color * this.alpha, 0.0f, new Vector2(0.0f, (float)this.texture.Bounds.Height), 0.5f * Game1.pixelZoom, SpriteEffects.None, (float)((this.tileY + this.tilesHigh - 2) * Game1.tileSize) / 10000f);
            }
        }

        public override void drawInMenu(SpriteBatch b, int x, int y)
        {
            CarpenterMenu menu = Game1.activeClickableMenu as CarpenterMenu;
            float texScale = 2;
            int num1 = (menu.maxWidthOfBuildingViewer - (int)(texture.Width * texScale)) / 2;
            num1 -= (int)(texture.Width / 3.5);
            int num2 = (menu.maxHeightOfBuildingViewer - (int)(texture.Height * texScale)) / 2;
            this.drawShadow(b, num1, num2);
            b.Draw(this.texture, new Rectangle(Game1.activeClickableMenu.xPositionOnScreen + num1, Game1.activeClickableMenu.yPositionOnScreen + num2, (int)(texture.Width * texScale), (int)(texture.Height * texScale)), Color.White);
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            Dictionary<string, string> savedata = new Dictionary<string, string>();
            savedata.Add("name", buildingType);
            savedata.Add("location", location.name);
            return savedata;
        }

        public object getReplacement()
        {
            Building building = new Building(new BluePrint("Shed"), new Vector2(tileX,tileY));
            building.daysOfConstructionLeft = daysOfConstructionLeft;
            building.tilesHigh = tilesHigh;
            building.tilesWide = tilesWide;
            building.indoors = indoors;
            building.tileX = tileX;
            building.tileY = tileY;
            return building;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            Building building = (Building)replacement;
            indoors = building.indoors;
            Vector2 p = new Vector2(building.tileX, building.tileY);
            BuildableGameLocation l = (BuildableGameLocation) Game1.getLocationFromName(additionalSaveData["location"]);
            build(p, l, building.daysOfConstructionLeft);
        }

    }
}
