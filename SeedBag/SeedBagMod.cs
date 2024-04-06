using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.Linq;
using SpaceShared.APIs;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace Portraiture2
{
    public class SeedBagMod : Mod
    {
        internal static SeedBagMod _instance;
        internal static IModHelper _helper => _instance.Helper;
        internal static ITranslationHelper i18n => _helper.Translation;
        internal static Config config;
        internal static Harmony HarmonyInstance;

        internal static bool DrawingTool = false;

        public override void Entry(IModHelper helper)
        {
            _instance = this;
            config = helper.ReadConfig<Config>();
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            HarmonyInstance = new Harmony("Platonymous.SeedBag");

            HarmonyInstance.Patch(
                AccessTools.Method(typeof(SpriteBatch), "Draw", new Type[] { typeof(Texture2D), typeof(Vector2), typeof(Rectangle?), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) })
                ,new HarmonyMethod(this.GetType(),nameof(Draw)));

            HarmonyInstance.Patch(
                AccessTools.Method(typeof(Game1), nameof(Game1.drawTool), new Type[] {typeof(Farmer), typeof(int)})
                ,new HarmonyMethod(this.GetType(), nameof(BeforeTool))
                , new HarmonyMethod(this.GetType(), nameof(AfterTool)));
        }

        public static void BeforeTool(Farmer f)
        {
            if(f == Game1.player)
                DrawingTool = true;
        }

        public static void AfterTool()
        {
            DrawingTool = false;
        }

        public static void Draw(ref Texture2D texture, ref Rectangle? sourceRectangle)
        {
            if (DrawingTool && Game1.player?.CurrentTool is SeedBagTool && texture == Game1.toolSpriteSheet)
            {
                texture = SeedBagTool.Texture;
                sourceRectangle = new Rectangle(0, 0, 16, 16);
            }
        }

        private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ShopMenu { ShopId: "SeedShop" } menu && new SeedBagTool() is SeedBagTool tool)
                menu.AddForSale(tool , new ItemStockInformation(tool.salePrice(),1));
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            SeedBagTool.LoadTextures(Helper);
            var spaceCore = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            spaceCore.RegisterSerializerType(typeof(SeedBagTool));
        }

    }
}
