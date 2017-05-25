using CustomElementHandler;

using Microsoft.Xna.Framework;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using System.Collections.Generic;

using xTile;

namespace Aquaponics
{
    class AquaponicsLocation : DecoratableLocation, ISaveElement
    {

        private BuildableGameLocation buildAt;
        private Building building;

        public AquaponicsLocation()
            :base(AquaponicsMod.helper.Content.Load<Map>(@"assets\greenhouseMap.tbin", ContentSource.ModFolder),"Aquaponics_Temp")
        {

        }

        public AquaponicsLocation(Map map, string name, BuildableGameLocation buildAt)
            :base(map,name)
        {
            build(buildAt);
        }

        private void resetExitWarp()
        {
            int bIndex = buildAt.buildings.FindIndex(x => x.indoors is AquaponicsLocation apl && apl == this);
            building = buildAt.buildings[bIndex];

            Vector2 entrance = new Vector2(building.tileX, building.tileY) + new Vector2(building.humanDoor.X, building.humanDoor.Y);

            for (int i = 0; i < warps.Count; i++)
            {
                if (warps[i] is Warp w && w.TargetName == "Farm")
                {
                    Warp exit = new Warp(w.X, w.Y, buildAt.name, (int)entrance.X, (int)entrance.Y + 1, false);
                    warps[i] = exit;
                    break;
                }
            }
        }

        public override void resetForPlayerEntry()
        {
            resetExitWarp();
            base.resetForPlayerEntry();
        }

        public void build(BuildableGameLocation buildAt)
        {
            this.buildAt = buildAt;
            isFarm = true;
            isStructure = true;
            floor = new List<int>();
            wallPaper = new List<int>();
            terrainFeatures = new SerializableDictionary<Vector2, StardewValley.TerrainFeatures.TerrainFeature>();
            objects = new SerializableDictionary<Vector2, Object>();
            largeTerrainFeatures = new List<StardewValley.TerrainFeatures.LargeTerrainFeature>();
            furniture = new List<StardewValley.Objects.Furniture>();  
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            Dictionary<string, string> savedata = new Dictionary<string, string>();
            savedata.Add("name", name);
            savedata.Add("buildAt", buildAt.name);
            return savedata;
        }

        public object getReplacement()
        {
            Shed shed = new Shed(AquaponicsMod.helper.Content.Load<Map>(@"Maps\Shed.xnb", ContentSource.GameContent), "Shed");
            shed.objects = objects;
            shed.terrainFeatures = terrainFeatures;
            shed.furniture = furniture;
            shed.floor = floor;
            shed.wallPaper = wallPaper;
            shed.largeTerrainFeatures = largeTerrainFeatures;
            return shed;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            BuildableGameLocation e = (BuildableGameLocation) Game1.getLocationFromName(additionalSaveData["buildAt"]);

            build(e);
            name = additionalSaveData["name"];
            
            Shed shed = (Shed)replacement;      
            objects = shed.objects;
            terrainFeatures = shed.terrainFeatures;
            furniture = shed.furniture;
            floor = shed.floor;
            wallPaper = shed.wallPaper;
            largeTerrainFeatures = shed.largeTerrainFeatures;

        }
    }
}
