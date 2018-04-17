using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK;
using PyTK.Tiled;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using xTile;

namespace TMXLoader.Compatibility
{
    class CustomFarmTypes
    {

        internal static IModHelper Helper;
        internal static IModHelper TMXHelper => TMXLoaderMod.helper;

        internal static void LoadingTheMaps()
        {
            Type mod = Type.GetType("CustomFarmTypes.Mod, CustomFarmTypes");
            if (mod == null)
                return;

            Helper = (mod.GetField("instance", BindingFlags.Static | BindingFlags.Public).GetValue(null) as Mod).Helper;
            TMXLoaderMod.monitor.Log("Checking for TMX Custom Farmtypes...");
            string[] choices = Directory.GetDirectories(Path.Combine(Helper.DirectoryPath, "FarmTypes"));
            int i = 0;
            foreach (string choice in choices)
            {
                if (File.Exists(Path.Combine(choice, "type.json")) && File.Exists(Path.Combine(choice, "map.tmx")) && File.Exists(Path.Combine(choice, "icon.png")))
                {
                    Type farmType = Type.GetType("CustomFarmTypes.FarmType, CustomFarmTypes");
                    string path = Path.Combine(choice, "type.json");
                    var farmTypeObject = Activator.CreateInstance(farmType);
                    MethodInfo m = Helper.GetType().GetMethod("ReadJsonFile", BindingFlags.Instance | BindingFlags.Public);
                    MethodInfo gm = m.MakeGenericMethod(new Type[] { farmTypeObject.GetType() });
                    var type = gm.Invoke(Helper, new object[] { path });

                    if (type == null)
                    {
                        TMXLoaderMod.monitor.Log("Problem reading type.json for custom farm type \"" + choice + "\".");
                        continue;
                    }

                    farmTypeObject.GetType().GetProperty("Folder", BindingFlags.Public | BindingFlags.Instance).SetValue(type, Path.Combine("FarmTypes", Path.GetFileName(choice)));
                    farmTypeObject.GetType().GetMethod("register", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { type });
                    i++;
                }
            }
            TMXLoaderMod.monitor.Log(i + " Found");
        }

        internal class FakeLoadMap
        {

            internal Map loadMap()
            {
                return Game1.getFarm().map;
            }

        }

        [HarmonyPatch]
        internal class LoadMapFix
        {
            internal static MethodInfo TargetMethod()
            {
                if (Type.GetType("CustomFarmTypes.FarmType, CustomFarmTypes") != null)
                    return AccessTools.Method(Type.GetType("CustomFarmTypes.FarmType, CustomFarmTypes"), "loadMap");
                else
                    return AccessTools.Method(typeof(FakeLoadMap), "loadMap");

            }

            internal static bool Prefix(object __instance)
            {
                string Folder = (string)__instance.GetType().GetProperty("Folder", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);
                string path = Path.Combine(Helper.DirectoryPath, Folder, "map.tmx");

                if (File.Exists(path))
                    return false;

                return true;
            }

            internal static void Postfix(object __instance, ref Map __result)
            {
                string Folder = (string)__instance.GetType().GetProperty("Folder", BindingFlags.Public | BindingFlags.Instance).GetValue(__instance);
                string mapPath = Path.Combine(Folder, "map.tmx");
                string filePath = Path.Combine(Helper.DirectoryPath, mapPath);

                if (!File.Exists(filePath))
                    return;

                TMXLoaderMod.monitor.Log("Loading.. " + Path.Combine(Folder, "map.tmx"));
                __result = TMXContent.Load(mapPath, Helper);
            }
        }


        [HarmonyPatch]
        internal class GreenHouseEventFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(PyUtils.getTypeSDV("Events.WorldChangeEvent"), "setUp");
            }

            internal static void Postfix(WorldChangeEvent __instance)
            {
                if (Game1.getFarm().map.Properties.ContainsKey("Greenhouse"))
                    if (TMXHelper.Reflection.GetField<int>(__instance, "whichEvent").GetValue() == 0)
                        setUpGreenhouseJoja(__instance);
                    else if (TMXHelper.Reflection.GetField<int>(__instance, "whichEvent").GetValue() == 1)
                        setUpGreenhouseCC(__instance);
            }

            internal static void setUpGreenhouseJoja(WorldChangeEvent wce)
            {
                Game1.currentLightSources.Clear();
                Game1.getFarm().temporarySprites.Clear();

                GameLocation location = Game1.getFarm();

                string[] position = location.map.Properties["Greenhouse"].ToString().Split(',');
                Point greenHousePosition = new Point(int.Parse(position[0]), int.Parse(position[1]));

                int sourceXTile = greenHousePosition.X;
                int sourceYTile = greenHousePosition.Y - 2;
                Game1.isRaining = false;

                location.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.mouseCursors, new Rectangle(288, 1349, 19, 28), 150f, 5, 999, new Vector2((float)((sourceXTile - 3) * Game1.tileSize + 2 * Game1.pixelZoom), (float)((sourceYTile - 1) * Game1.tileSize - Game1.tileSize / 2)), false, false)
                {
                    scale = (float)Game1.pixelZoom,
                    layerDepth = 0.0961f
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.mouseCursors, new Rectangle(288, 1377, 19, 28), 140f, 5, 999, new Vector2((float)((sourceXTile + 3) * Game1.tileSize - 4 * Game1.pixelZoom), (float)((sourceYTile - 2) * Game1.tileSize)), false, false)
                {
                    scale = (float)Game1.pixelZoom,
                    layerDepth = 0.0961f
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.mouseCursors, new Rectangle(390, 1405, 18, 32), 1000f, 2, 999, new Vector2((float)(sourceXTile * Game1.tileSize + 2 * Game1.pixelZoom), (float)((sourceYTile - 3) * Game1.tileSize)), false, false)
                {
                    scale = (float)Game1.pixelZoom,
                    layerDepth = 0.0961f
                });

                Game1.currentLightSources.Add(new LightSource(4, new Vector2(sourceXTile, sourceYTile) * Game1.tileSize, 4f));
                Game1.viewport.X = Math.Max(0, Math.Min(location.map.DisplayWidth - Game1.viewport.Width, sourceXTile * Game1.tileSize - Game1.viewport.Width / 2));
                Game1.viewport.Y = Math.Max(0, Math.Min(location.map.DisplayHeight - Game1.viewport.Height, sourceYTile * Game1.tileSize - Game1.viewport.Height / 2));
            }

            internal static void setUpGreenhouseCC(WorldChangeEvent wce)
            {
                Game1.currentLightSources.Clear();
                Game1.getFarm().temporarySprites.Clear();
                GameLocation location = Game1.getFarm();

                string[] position = location.map.Properties["Greenhouse"].ToString().Split(',');
                Point greenHousePosition = new Point(int.Parse(position[0]), int.Parse(position[1]));

                int sourceXTile = greenHousePosition.X;
                int sourceYTile = greenHousePosition.Y - 2;
                Game1.isRaining = false;

                Utility.addSprinklesToLocation(location, sourceXTile, sourceYTile - 1, 7, 7, 15000, 150, Color.LightCyan, null, false);
                Utility.addStarsAndSpirals(location, sourceXTile, sourceYTile - 1, 7, 7, 15000, 150, Color.White, null, false);
                Game1.currentLightSources.Add(new LightSource(4, new Vector2(sourceXTile, sourceYTile) * Game1.tileSize, 4f, Color.DarkGoldenrod));
                List<TemporaryAnimatedSprite> temporarySprites1 = location.temporarySprites;
                TemporaryAnimatedSprite temporaryAnimatedSprite1 = new TemporaryAnimatedSprite(Game1.mouseCursors, new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2((sourceXTile * Game1.tileSize), ((sourceYTile - 3) * Game1.tileSize - Game1.tileSize)), false, false);
                temporaryAnimatedSprite1.scale = Game1.pixelZoom;
                temporaryAnimatedSprite1.layerDepth = 1f;
                int num1 = 1;
                temporaryAnimatedSprite1.xPeriodic = num1 != 0;
                double num2 = 2000.0;
                temporaryAnimatedSprite1.xPeriodicLoopTime = (float)num2;
                double num3 = (Game1.tileSize / 4);
                temporaryAnimatedSprite1.xPeriodicRange = (float)num3;
                int num4 = 1;
                temporaryAnimatedSprite1.light = num4 != 0;
                Color darkGoldenrod1 = Color.DarkGoldenrod;
                temporaryAnimatedSprite1.lightcolor = darkGoldenrod1;
                double num5 = 1.0;
                temporaryAnimatedSprite1.lightRadius = (float)num5;
                temporarySprites1.Add(temporaryAnimatedSprite1);

                Game1.currentLightSources.Add(new LightSource(4, new Vector2(sourceXTile, sourceYTile) * Game1.tileSize, 4f));
                Game1.viewport.X = Math.Max(0, Math.Min(location.map.DisplayWidth - Game1.viewport.Width, sourceXTile * Game1.tileSize - Game1.viewport.Width / 2));
                Game1.viewport.Y = Math.Max(0, Math.Min(location.map.DisplayHeight - Game1.viewport.Height, sourceYTile * Game1.tileSize - Game1.viewport.Height / 2));
            }
        }

        [HarmonyPatch]
        internal class GreenHouseDrawFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(PyUtils.getTypeSDV("Farm"), "draw");
            }

            internal static void Prefix(Farm __instance)
            {
                if (__instance.map.Properties.ContainsKey("Greenhouse"))
                    TMXHelper.Reflection.GetField<Rectangle>(__instance, "greenhouseSource").SetValue(new Rectangle(-1,-1,0,0));
            }

            internal static void Postfix(Farm __instance, SpriteBatch b)
            {
                if (__instance.map.Properties.ContainsKey("Greenhouse"))
                {
                    Rectangle greenhouseSource = new Rectangle(160, 160 * (Game1.player.mailReceived.Contains("ccPantry") ? 1 : 0), 112, 160);
                    TMXHelper.Reflection.GetField<Rectangle>(__instance, "greenhouseSource").SetValue(greenhouseSource);

                    string[] position = __instance.map.Properties["Greenhouse"].ToString().Split(',');
                    Point greenHousePosition = new Point(int.Parse(position[0]), int.Parse(position[1]));
                    b.Draw(__instance.houseTextures, Game1.GlobalToLocal(Game1.viewport, new Vector2((greenHousePosition.X - 3) * 64f, (greenHousePosition.Y - 9) * 64f)), new Rectangle?(greenhouseSource), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0704f);
                }
            }
        }

        internal static void fixGreenhouseWarp()
        {
            Farm farm = Game1.getFarm();
            if (farm.map.Properties.ContainsKey("Greenhouse"))
            {
                string[] position = farm.map.Properties["Greenhouse"].ToString().Split(',');
                Point greenHousePosition = new Point(int.Parse(position[0]), int.Parse(position[1]));

                GameLocation greenhouse = Game1.getLocationFromName("Greenhouse");
                Warp warp = greenhouse.warps.Find(w => w.TargetName == "Farm" && w.TargetX == 28 && w.TargetY == 16);
                if (warp != null)
                {
                    warp.TargetX = greenHousePosition.X;
                    warp.TargetY = greenHousePosition.Y + 1;
                }

            }
        }





    }

    

}