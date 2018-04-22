using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.IO;
using SObject = StardewValley.Object;

namespace PlanImporter
{
    public class PlanImporterMod : Mod
    {
        Dictionary<string, Import> Imports;

        public override void Entry(IModHelper helper)
        {
            loadContentPacks();
            Helper.ConsoleCommands.Add("plan", "Imports the contents of a planner export into your farm",(s,p) => importFarm(p));
        }

        public void importFarm(string[] p)
        {
            if(Game1.currentLocation is Farm)
            {
                Monitor.Log("You can't do this while you are on the farm", LogLevel.Warn);
                return;
            }

            if(p.Length == 0)
            {
                Monitor.Log("No plan id provided", LogLevel.Warn);
                return;
            }

            string id = p[0];

            if (!Imports.ContainsKey(id))
            {
                Monitor.Log("Could not find a plan with the id " + id, LogLevel.Warn);
                return;
            }
            Import import = Imports[id];


            Monitor.Log("Clearing Farm", LogLevel.Trace);

            Farm farm = Game1.getFarm();
            farm.objects.Clear();
            farm.largeTerrainFeatures.Clear();
            farm.terrainFeatures.Clear();
            farm.buildings.Clear();
            farm.animals.Clear();
            farm.resourceClumps.Clear();

            Monitor.Log("OK", LogLevel.Trace);

            Monitor.Log("Importing: " + id, LogLevel.Trace);
            List<ImportTile> list = import.tiles;
            list.AddRange(import.buildings);
            foreach(ImportTile tile in list)
            {
                Vector2 pos = new Vector2(int.Parse(tile.x) / 16, int.Parse(tile.y) / 16);

                TerrainFeature tf = getTerrainFeature(tile.type, pos);
                if (tf != null) {

                    if (tf is FruitTree ft)
                    {
                        ft.growthStage = 5;
                        ft.daysUntilMature = 0;
                    }

                    if (farm.terrainFeatures.ContainsKey(pos))
                        farm.terrainFeatures[pos] = tf;
                    else
                        farm.terrainFeatures.Add(pos, tf);
                }

                SObject obj = getObject(tile.type, pos);
                if (obj != null)
                {
                    if (farm.Objects.ContainsKey(pos))
                        farm.Objects[pos] = obj;
                    else
                        farm.Objects.Add(pos, obj);
                }

                Building building = getBuilding(tile.type, pos);
                if (building != null)
                {
                    building.daysOfConstructionLeft = 0;
                    farm.buildings.Add(building);
                }

                if (tile.type == "large-log")
                    farm.addResourceClumpAndRemoveUnderlyingTerrain(602, 2, 2, pos);

                if (tile.type == "large-stump")
                    farm.addResourceClumpAndRemoveUnderlyingTerrain(600, 2, 2, pos);


                if (tile.type == "large-rock")
                    farm.addResourceClumpAndRemoveUnderlyingTerrain(752, 2, 2, pos);


            }
        }

        public TerrainFeature getTerrainFeature(string type, Vector2 pos)
        {
            switch (type)
            {
                case "wood-floor": return new Flooring(0);
                case "stone-floor": return new Flooring(1);
                case "weathered-floor": return new Flooring(2);
                case "crystal-floor": return new Flooring(3);
                case "straw-floor": return new Flooring(4);
                case "gravel-path": return new Flooring(5);
                case "wood-path": return new Flooring(6);
                case "crystal-path": return new Flooring(7);
                case "steppingstone-path": return new Flooring(8);
                case "road": return new Flooring(9);
                case "cherry-tree": return new FruitTree(628,5);
                case "apricot": return new FruitTree(629, 5);
                case "orange-tree": return new FruitTree(630, 5);
                case "peach": return new FruitTree(631, 5);
                case "pomegranate": return new FruitTree(632, 5);
                case "apple": return new FruitTree(633, 5);
                case "oak-tree": return new Tree(1, 5);
                case "pine-tree": return new Tree(3, 5);
                case "maple-tree": return new Tree(2, 5);
                case "tree": Tree tree = new Tree(1, 5); tree.stump = true; tree.health = 5f; return tree;
                case "large-rock": return new ResourceClump(672, 2, 2,pos);
                case "large-log": return new ResourceClump(600, 2, 2, pos);
                case "large-stump": return new ResourceClump(602, 2, 2, pos);
                case "tulips": Crop tulips = new Crop(427, (int)pos.X, (int)pos.Y); tulips.growCompletely(); return new HoeDirt(0, tulips);
                case "tulip": Crop tulip = new Crop(427, (int)pos.X, (int)pos.Y); tulip.growCompletely(); return new HoeDirt(0, tulip);
                case "summer-spangle": Crop summerspangle = new Crop(455, (int)pos.X, (int)pos.Y); summerspangle.growCompletely(); return new HoeDirt(0, summerspangle);
                case "blue-jazz": Crop bluejazz = new Crop(429, (int)pos.X, (int)pos.Y); bluejazz.growCompletely(); return new HoeDirt(0, bluejazz);
                case "fairy-rose": Crop fairyrose = new Crop(425, (int)pos.X, (int)pos.Y); fairyrose.growCompletely(); return new HoeDirt(0, fairyrose);
                case "poppy": Crop poppy = new Crop(453, (int)pos.X, (int)pos.Y); poppy.growCompletely(); return new HoeDirt(0, poppy);
                case "grape": Crop grape = new Crop(301, (int)pos.X, (int)pos.Y); grape.growCompletely(); return new HoeDirt(0, grape);
                case "trellis": Crop trellis = new Crop(301, (int)pos.X, (int)pos.Y); trellis.growCompletely(); return new HoeDirt(0, trellis);
                case "cauliflower": Crop cauliflower = new Crop(474, (int)pos.X, (int)pos.Y); cauliflower.growCompletely(); return new HoeDirt(0, cauliflower);
                case "garlic": Crop garlic = new Crop(476, (int)pos.X, (int)pos.Y); garlic.growCompletely(); return new HoeDirt(0, garlic);
                case "green-bean": Crop greenbean = new Crop(473, (int)pos.X, (int)pos.Y); greenbean.growCompletely(); return new HoeDirt(0, greenbean);
                case "kale": Crop kale = new Crop(477, (int)pos.X, (int)pos.Y); kale.growCompletely(); return new HoeDirt(0, kale);
                case "parsnip": Crop parsnip = new Crop(472, (int)pos.X, (int)pos.Y); parsnip.growCompletely(); return new HoeDirt(0, parsnip);
                case "potato": Crop potato = new Crop(475, (int)pos.X, (int)pos.Y); potato.growCompletely(); return new HoeDirt(0, potato);
                case "rhubarb": Crop rhubarb = new Crop(478, (int)pos.X, (int)pos.Y); rhubarb.growCompletely(); return new HoeDirt(0, rhubarb);
                case "strawberry": Crop strawberry = new Crop(745, (int)pos.X, (int)pos.Y); strawberry.growCompletely(); return new HoeDirt(0, strawberry);
                case "blueberry": Crop blueberry = new Crop(481, (int)pos.X, (int)pos.Y); blueberry.growCompletely(); return new HoeDirt(0, blueberry);
                case "corn": Crop corn = new Crop(487, (int)pos.X, (int)pos.Y); corn.growCompletely(); return new HoeDirt(0, corn);
                case "hops": Crop hops = new Crop(302, (int)pos.X, (int)pos.Y); hops.growCompletely(); return new HoeDirt(0, hops);
                case "hot-pepper": Crop hotpepper = new Crop(482, (int)pos.X, (int)pos.Y); hotpepper.growCompletely(); return new HoeDirt(0, hotpepper);
                case "melon": Crop melon = new Crop(479, (int)pos.X, (int)pos.Y); melon.growCompletely(); return new HoeDirt(0, melon);
                case "radish": Crop radish = new Crop(484, (int)pos.X, (int)pos.Y); radish.growCompletely(); return new HoeDirt(0, radish);
                case "red-cabbage": Crop redcabbage = new Crop(485, (int)pos.X, (int)pos.Y); redcabbage.growCompletely(); return new HoeDirt(0, redcabbage);
                case "starfruit": Crop starfruit = new Crop(486, (int)pos.X, (int)pos.Y); starfruit.growCompletely(); return new HoeDirt(0, starfruit);
                case "tomato": Crop tomato = new Crop(480, (int)pos.X, (int)pos.Y); tomato.growCompletely(); return new HoeDirt(0, tomato);
                case "wheat": Crop wheat = new Crop(483, (int)pos.X, (int)pos.Y); wheat.growCompletely(); return new HoeDirt(0, wheat);
                case "amaranth": Crop amaranth = new Crop(299, (int)pos.X, (int)pos.Y); amaranth.growCompletely(); return new HoeDirt(0, amaranth);
                case "ancient-fruit": Crop ancientfruit = new Crop(499, (int)pos.X, (int)pos.Y); ancientfruit.growCompletely(); return new HoeDirt(0, ancientfruit);
                case "artichoke": Crop artichoke = new Crop(489, (int)pos.X, (int)pos.Y); artichoke.growCompletely(); return new HoeDirt(0, artichoke);
                case "beet": Crop beet = new Crop(494, (int)pos.X, (int)pos.Y); beet.growCompletely(); return new HoeDirt(0, beet);
                case "bokchoy": Crop bokchoy = new Crop(491, (int)pos.X, (int)pos.Y); bokchoy.growCompletely(); return new HoeDirt(0, bokchoy);
                case "cranberry": Crop cranberry = new Crop(493, (int)pos.X, (int)pos.Y); cranberry.growCompletely(); return new HoeDirt(0, cranberry);
                case "pumpkin": Crop pumpkin = new Crop(490, (int)pos.X, (int)pos.Y); pumpkin.growCompletely(); return new HoeDirt(0, pumpkin);
                case "eggplant": Crop eggplant = new Crop(488, (int)pos.X, (int)pos.Y); eggplant.growCompletely(); return new HoeDirt(0, eggplant);
                case "sunflower": Crop sunflower = new Crop(431, (int)pos.X, (int)pos.Y); sunflower.growCompletely(); return new HoeDirt(0, sunflower);
                case "yam": Crop yam = new Crop(492, (int)pos.X, (int)pos.Y); yam.growCompletely(); return new HoeDirt(0, yam);
                case "grass": return new Grass(1, 4);
                case "farmland": return new HoeDirt(0);
                default: return null;
            }
        }

        public SObject getObject(string type, Vector2 pos)
        {
            switch (type)
            {
                case "bee-hive": return new SObject(pos,10);
                case "mayo": return new SObject(pos, 24);
                case "cheese-press": return new SObject(pos, 16);
                case "keg": return new SObject(pos, 12);
                case "loom": return new SObject(pos, 17);
                case "oil-maker": return new SObject(pos, 19);
                case "preserves": return new SObject(pos, 15);
                case "sprinkler": return new SObject(pos, 599, 1);
                case "q-sprinkler": return new SObject(pos, 621, 1);
                case "irid-sprinkler": return new SObject(pos, 645, 1);
                case "scarecrow": return new SObject(pos, 8);
                case "chest": return new Chest(pos) { playerChest = true };
                case "furnace": return new SObject(pos, 13);
                case "charcoal": return new SObject(pos, 114);
                case "seed-maker": return new SObject(pos, 25);
                case "crystal": return new SObject(pos, 21);
                case "egg-press": return new SObject(pos, 158);
                case "lighting-rod": return new SObject(pos, 9);
                case "recycling-machine": return new SObject(pos, 20);
                case "slime-incubator": return new SObject(pos, 156);
                case "worm-bin": return new SObject(pos, 154);
                case "fence": return new Fence(pos, 1, false);
                case "stone-fence": return new Fence(pos, 2, false);
                case "iron-fence": return new Fence(pos, 3, false);
                case "hardwood-fence": return new Fence(pos, 5, false);
                case "gate": return new Fence(pos, 4, true);
                case "torch": return new Torch(pos,1);
                case "wood-lamp-post": return new SObject(pos, 152);
                case "iron-lamp-post": return new SObject(pos, 153);
                case "campfire": return new SObject(pos, 146);
                case "stone": return new SObject(pos, 449,1);
                case "twig": return new SObject(pos,294, 1);
                default: return null;
            }
        }

        public Building getBuilding(string type, Vector2 pos)
        {
            switch (type)
            {
                case "silo": return new Building(new BluePrint("Silo"), pos);
                case "mill": return new Mill(new BluePrint("Mill"), pos);
                case "well": return new Building(new BluePrint("Well"), pos);
                case "coop": return new Coop(new BluePrint("Coop"), pos);
                case "barn": return new Barn(new BluePrint("Barn"), pos);
                case "stable": return new Stable(new BluePrint("Stable"),pos);
                case "slime-hutch": return new Building(new BluePrint("Slime Hutch"), pos);
                case "water-obelisk": return new Building(new BluePrint("Water Obelisk"), pos);
                case "earth-obelisk": return new Building(new BluePrint("Earth Obelisk"), pos);
                case "gold-clock": return new Building(new BluePrint("Gold Clock"), pos);
                case "junimo-hut": return new JunimoHut(new BluePrint("Junimo Hut"), pos);
                case "Shed": return new Building(new BluePrint("Shed"), pos);
                default: return null;
            }
        }

        public void loadContentPacks() { 
            Imports = new Dictionary<string, Import>();
            string[] files = Directory.GetFiles(Path.Combine(Helper.DirectoryPath, "imports"), "*.json", SearchOption.AllDirectories);
            foreach (string file in files) {
                Import import = Helper.ReadJsonFile<Import>(file);
                import.id = import.id == "" ? Path.GetFileNameWithoutExtension(file) : import.id;
                if (Imports.ContainsKey(import.id))
                    Imports[import.id] = import;
                else
                Imports.Add(import.id, import);
            }
        }

    }
}
