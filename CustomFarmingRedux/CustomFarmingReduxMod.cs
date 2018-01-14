using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using System.IO;
using PyTK;
using PyTK.Extensions;
using PyTK.Types;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework.Graphics;
using Harmony;
using System.Reflection;
using PyTK.CustomElementHandler;
using StardewValley.Objects;
using SObject = StardewValley.Object;
using Microsoft.Xna.Framework;

namespace CustomFarmingRedux
{
    public class CustomFarmingReduxMod : Mod
    {
        public static IModHelper _helper;
        public static IMonitor _monitor;
        public static List<CustomMachineBlueprint> machines = new List<CustomMachineBlueprint>();
        public static List<CustomFarmingPack> packs = new List<CustomFarmingPack>();
        public static Config _config;
        public static string folder = "Machines";
        public static string legacyFolder = "MachinesCF1";
        private static Dictionary<string, int> craftingrecipes = new Dictionary<string, int>();

        public override void Entry(IModHelper helper)
        {
            _helper = Helper;
            _monitor = Monitor;
            _config = Helper.ReadConfig<Config>();

            loadPacks();

            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            SaveEvents.AfterLoad += (s, e) => Game1.player.craftingRecipes.AddOrReplace(craftingrecipes);
            harmonyFix();
            SaveHandler.addPreprocessor(legacyFix);
            SaveHandler.addReplacementPreprocessor(fixLegacyObject);
            
        }

        private static string legacyFix(string dataString)
        {
            if (dataString.Contains("simpleMachine"))
            {

                string[] data = SaveHandler.splitElemets(dataString);
                Dictionary<string, string> additionalSaveData = new Dictionary<string, string>();

                for (int i = 3; i < data.Length; i++)
                {
                    string[] entry = data[i].Split(SaveHandler.valueSeperator);
                    additionalSaveData.Add(entry[0], entry[1]);
                }

                string id = new DirectoryInfo(additionalSaveData["modfolder"]).Name + "." + additionalSaveData["filename"];
                CustomMachineBlueprint machine = machines.Find(m => m.legacy == id);

                if (machine == null)
                    return dataString;

                dataString = SaveHandler.prefix + SaveHandler.seperator + "Object" + SaveHandler.seperator + "CustomFarmingRedux.CustomMachine, CustomFarmingRedux" + SaveHandler.seperator + "id" + SaveHandler.valueSeperator + machine.fullid;
                _monitor.Log("Legacy machine converted: " + dataString, LogLevel.Trace);
                return dataString;
            }
           

            return dataString;
        }

        private static object fixLegacyObject(object replacement)
        {
            if (replacement is Chest c)
            {
                if (c.name.Contains("simpleMachine"))
                {
                    c.items = new List<Item>();
                    return c;
                }

                if (c.name.Contains("customNamedObject"))
                {
                    SObject item = (SObject) c.items[0];
                    SObject obj = new SObject(Vector2.Zero, item.parentSheetIndex, item.stack);
                    obj.name = item.name;
                    obj.quality = item.quality;
                    return obj;
                }
            }

            return replacement;
        }

        private void harmonyFix()
        {
            var instance = HarmonyInstance.Create("Platonymous.CustomFarmingRedux");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (Game1.activeClickableMenu is GameMenu activeMenu && Helper.Reflection.GetField<List<IClickableMenu>>(activeMenu, "pages").GetValue().Find(p => p is CraftingPage) is CraftingPage craftingPage)
            {
                foreach (CustomMachineBlueprint blueprint in machines.Where(m => m.crafting != null))
                    for (int i = 0; i < craftingPage.pagesOfCraftingRecipes.Count; i++)
                    {
                        if (craftingPage.pagesOfCraftingRecipes[i].Find(k => k.Value.name == blueprint.fullid) is KeyValuePair<ClickableTextureComponent, CraftingRecipe> kv && kv.Value != null && kv.Key != null)
                        {
                            kv.Key.texture = Helper.Content.Load<Texture2D>($"{blueprint.pack.baseFolder}/{blueprint.folder}/{blueprint.texture}");
                            kv.Key.sourceRect = Game1.getSourceRectForStandardTileSheet(kv.Key.texture, blueprint.tileindex, blueprint.tilewidth, blueprint.tileheight);
                            kv.Value.DisplayName = blueprint.name;
                            Helper.Reflection.GetField<string>(kv.Value, "description").SetValue(blueprint.description);
                        }
                    }
            }
        }

        private void loadPacks()
        {
            string machineDir = Path.Combine(Helper.DirectoryPath, folder);
            if (Directory.Exists(machineDir) && new DirectoryInfo(machineDir).GetDirectories().Length > 0)
                PyUtils.loadContentPacks(out packs, machineDir, SearchOption.AllDirectories, Monitor);
            machines = new List<CustomMachineBlueprint>();
            Dictionary<string, string> toCrafting = new Dictionary<string, string>();

            List<LegacyBlueprint> legacyPacks = new List<LegacyBlueprint>();
            string legacyDir = Path.Combine(Helper.DirectoryPath, legacyFolder);
            if(Directory.Exists(legacyDir) && new DirectoryInfo(legacyDir).GetDirectories().Length > 0)
                PyUtils.loadContentPacks(out legacyPacks, legacyDir, SearchOption.AllDirectories, Monitor);

            foreach (LegacyBlueprint lPack in legacyPacks)
            {
                string lid = lPack.folderName + "." + lPack.fileName;

                bool exists = false;
                packs.useAll(p => exists = exists || p.machines.Exists(m => m.legacy == lid));

                if (exists)
                {
                    Monitor.Log("Skipped legacy machine " + lid + " because a new version was found.", LogLevel.Trace);
                    continue;
                }
                    
                CustomFarmingPack next = new CustomFarmingPack();
                next.legacy = true;
                next.author = lPack.author;
                next.fileName = lPack.fileName;
                next.folderName = lPack.folderName;
                next.name = lPack.name;
                next.machines = new List<CustomMachineBlueprint>();
                CustomMachineBlueprint legacyMachine = new CustomMachineBlueprint();
                legacyMachine.id = 0;
                legacyMachine.pack = next;
                legacyMachine.category = lPack.CategoryName;
                legacyMachine.description = lPack.Description;
                legacyMachine.name = lPack.Name;
                legacyMachine.frames = lPack.WorkAnimationFrames;
                legacyMachine.pulsate = lPack.WorkAnimationFrames <= 0;
                legacyMachine.readyindex = lPack.ReadyTileIndex;
                legacyMachine.tileindex = lPack.TileIndex;
                legacyMachine.texture = lPack.Tilesheet;
                legacyMachine.fps = 6;
                legacyMachine.showitem = lPack.displayItem;
                legacyMachine.itempos = new int[] { lPack.displayItemX, lPack.displayIemY };
                legacyMachine.itemzoom = lPack.displayItemZoom;
                legacyMachine.crafting = lPack.Crafting;

                if (lPack.StarterMaterial != -1)
                {
                    IngredientBlueprint starter = new IngredientBlueprint();
                    starter.index = lPack.StarterMaterial;
                    starter.stack = lPack.StarterMaterialStack;
                    legacyMachine.starter = starter;
                }

                if (lPack.Produce != null)
                {
                    legacyMachine.production = new List<RecipeBlueprint>();
                    legacyMachine.legacy = lid;
                    Monitor.Log("Legacy:" + legacyMachine.legacy);
                    RecipeBlueprint baseProduce = new RecipeBlueprint();
                    baseProduce.name = lPack.Produce.Name;
                    baseProduce.index = lPack.Produce.ProduceID;
                    baseProduce.colored = lPack.Produce.useColor;
                    baseProduce.prefix = lPack.Produce.usePrefix;
                    baseProduce.suffix = lPack.Produce.useSuffic;
                    baseProduce.texture = lPack.Produce.Tilesheet;
                    baseProduce.tileindex = lPack.TileIndex;
                    baseProduce.time = lPack.Produce.ProductionTime;
                    baseProduce.stack = lPack.Produce.Stack;
                    baseProduce.description = lPack.Produce.Description;
                    baseProduce.quality = lPack.Produce.Quality;
                    List<int> materials = lPack.Materials.ToList();
                    baseProduce.materials = new List<IngredientBlueprint> { new IngredientBlueprint() };
                    baseProduce.materials[0].index = materials[0];
                    baseProduce.materials[0].exactquality = lPack.Produce.MaterialQuality;
                    baseProduce.materials[0].stack = lPack.RequiredStack;

                    if (lPack.SpecialProduce != null)
                    {
                        foreach (LegacySpecialProduce pnext in lPack.SpecialProduce)
                        {
                            RecipeBlueprint nextProduce = new RecipeBlueprint();
                            nextProduce.name = pnext.Name != null ? pnext.Name : baseProduce.name;
                            nextProduce.index = pnext.ProduceID != -1 ? pnext.ProduceID : baseProduce.index;
                            nextProduce.colored = pnext.uc ? pnext._useColor : baseProduce.colored;
                            nextProduce.prefix = pnext.up ? pnext._usePrefix : baseProduce.prefix;
                            nextProduce.suffix = pnext.us ? pnext._useSuffix : baseProduce.suffix;
                            nextProduce.texture = pnext.Tilesheet != null ? pnext.Tilesheet : baseProduce.texture;
                            nextProduce.tileindex = pnext.TileIndex != -1 ? pnext.TileIndex : baseProduce.tileindex;
                            nextProduce.time = pnext.ProductionTime != -1 ? pnext.ProductionTime : baseProduce.time;
                            nextProduce.stack = pnext.Stack != -1 ? pnext.Stack : baseProduce.stack;
                            nextProduce.description = pnext.Description != null ? pnext.Description : baseProduce._description;
                            nextProduce.quality = pnext.Quality != -9 ? pnext.Quality : baseProduce.quality;
                            nextProduce.materials = new List<IngredientBlueprint>() { new IngredientBlueprint() };
                            nextProduce.materials[0].index = pnext.Material;
                            nextProduce.materials[0].exactquality = pnext.MaterialQuality;
                            nextProduce.materials[0].stack = lPack.RequiredStack;
                            materials.Remove(pnext.Material);
                            legacyMachine.production.Add(nextProduce);
                        }
                    }

                    baseProduce.materials[0].index = materials.Count > 0 ? materials[0] : baseProduce.materials[0].index;
                    materials.Remove(baseProduce.materials[0].index);
                    baseProduce.include = materials.Count > 0 ? materials.ToArray() : null;
                    legacyMachine.production.Add(baseProduce);
                }

                next.machines.Add(legacyMachine);
                packs.Add(next);
            }

            foreach (CustomFarmingPack pack in packs)
                foreach (CustomMachineBlueprint blueprint in pack.machines)
                {
                    blueprint.pack = pack;
                    machines.AddOrReplace(blueprint);

                    if (blueprint.production != null)
                        foreach (RecipeBlueprint recipe in blueprint.production)
                            recipe.mBlueprint = blueprint;

                    if (blueprint.crafting != null)
                    {
                        toCrafting.AddOrReplace(blueprint.fullid, $"{blueprint.crafting}/Home/130/true/null/{blueprint.fullid}");
                        craftingrecipes.AddOrReplace(blueprint.fullid, 0);
                    }

                    if (blueprint.forsale && (blueprint.condition == null || PyTK.PyUtils.CheckEventConditions(blueprint.condition)))
                        new InventoryItem(new CustomMachine(blueprint), blueprint.price).addToNPCShop(blueprint.shop);
                }
            
            if (toCrafting.Count > 0)
                toCrafting.injectInto($"Data/CraftingRecipes");

            Monitor.Log(packs.Count + " Content Packs with " + machines.Count + " machines found.", LogLevel.Trace);
        }
    }
}
