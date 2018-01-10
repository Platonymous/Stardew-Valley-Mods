using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using System.IO;
using PyTK;
using PyTK.Extensions;
using PyTK.Types;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework.Graphics;
using Harmony;
using System.Reflection;

namespace CustomFarmingRedux
{
    public class CustomFarmingReduxMod : Mod
    {
        public static IModHelper _helper;
        public static IMonitor _monitor;
        public static List<CustomMachineBlueprint> machines;
        public static List<CustomFarmingPack> packs;
        public static Config _config;
        public static string folder = "Machines";
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
                            kv.Key.texture = Helper.Content.Load<Texture2D>($"{folder}/{blueprint.folder}/{blueprint.texture}");
                            kv.Key.sourceRect = Game1.getSourceRectForStandardTileSheet(kv.Key.texture, blueprint.tileindex, blueprint.tilewidth, blueprint.tileheight);
                            kv.Value.DisplayName = blueprint.name;
                            Helper.Reflection.GetField<string>(kv.Value, "description").SetValue(blueprint.description);
                        }
                    }
            }
        }

        private void loadPacks()
        {
            PyUtils.loadContentPacks(out packs, Path.Combine(Helper.DirectoryPath, folder), SearchOption.AllDirectories, Monitor);
            machines = new List<CustomMachineBlueprint>();
            Dictionary<string, string> toCrafting = new Dictionary<string, string>();

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
