using Harmony;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlatoTK;
using PlatoTK.UI.Components;
using PlatoTK.UI;
using Microsoft.Xna.Framework;
using StardewValley.Locations;

namespace PlatoWarpMenu
{
    public class PlatoWarpMenuMod : Mod
    {
        public Config config;
        internal ITranslationHelper i18n => Helper.Translation;

        internal static bool intercept = false;

        internal static IModHelper _helper;

        internal static GameLocation CurrentLocation;

        internal static Action Callback;

        public static Texture2D LastScreen = null;

        internal static PlatoWarpMenuMod instance;

        internal static Queue<GameLocation> locations = new Queue<GameLocation>();

        internal static string tempFolder;

        internal static IMapAPI pytk = null;
        public override void Entry(IModHelper helper)
        {
            instance = this;
            _helper = helper;
            tempFolder = Path.Combine(helper.DirectoryPath, "Temp");
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            config = helper.ReadConfig<Config>();

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            helper.Events.GameLoop.GameLaunched += (s, e) =>
            {

                SetUpConfigMenu();

                if (Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone") is IMobilePhoneApi api)
                {
                    Texture2D appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "mobile_app_icon.png"));
                    bool success = api.AddApp(Helper.ModRegistry.ModID + "MobileWarpMenu", "Warp Menu", () =>
                    {
                        OpenUpMenu();
                    }, appIcon);
                }

                if (Helper.ModRegistry.GetApi<IMapAPI>("Platonymous.Toolkit") is IMapAPI pApi)
                    pytk = pApi;
            };

            var harmony = HarmonyInstance.Create("Platonymous.PlatoWarpMenu");
            bool patched = false;

            if (!config.CompatibilityMode || Constants.TargetPlatform == GamePlatform.Android)
            {
                try
                {
                    harmony.Patch(Type.GetType("System.Drawing.Image, System.Drawing").GetMethod("Save", new Type[] { typeof(string), Type.GetType("System.Drawing.Imaging.ImageFormat, System.Drawing") }), prefix: new HarmonyMethod(this.GetType().GetMethod("InterceptScreenshot", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)));
                    patched = true;
                }
                catch
                {

                }
            }

            var displayDevice = Game1.mapDisplayDevice;

            helper.Events.GameLoop.UpdateTicked += (s, e) =>
            {
                displayDevice = Game1.mapDisplayDevice;
            };

            helper.Events.GameLoop.UpdateTicking += (s, e) =>
            {
                if (Game1.activeClickableMenu is IUIMenu uim)
                {
                    uim.ShouldDraw = () =>
                    {
                       return Game1.mapDisplayDevice == displayDevice;
                    };
                }
            };
        }

        public void OpenUpMenu()
        {
            IWrapper wrapper = Helper.GetPlatoHelper().UI.LoadFromFile(Path.Combine(Helper.DirectoryPath, "menu.xml"), "PlatoWarpMenu");

            List<string> sets = GetSets();
            Dictionary<string, Texture2D> locationShots = new Dictionary<string, Texture2D>();
            locationShots.Add("Placeholder", Helper.GetPlatoHelper().Content.Textures.GetRectangle(200, 200, Color.Transparent));
            int rightX = 780;

            string activeSet = (Game1.currentLocation.IsFarm || Game1.currentLocation.IsGreenhouse) ? i18n.Get("menu.farm") : Game1.currentLocation.isStructure.Value ? i18n.Get("menu.buildings") : Game1.currentLocation.IsOutdoors ? i18n.Get("menu.outdoors") : i18n.Get("menu.indoors");
            
            if (Game1.currentLocation.map.Properties.ContainsKey("Group") && Game1.currentLocation.map.Properties["Group"].ToString() is string s && sets.Contains(s))
                activeSet = s;

            string activeLocation = Game1.currentLocation.isStructure.Value ? Game1.currentLocation.uniqueName.Value : Game1.currentLocation.Name;
            Point selectedTile = new Point(-1, -1);

            Action<IComponent> pickSet = (c) =>
            {
                activeSet = c.Id;
                selectedTile = new Point(-1, -1);
                c.Parent.DeSelectAll(p => true);
                c.Select();
                c.SelectAll(se => se.HasTag("caption"));
                c.GetWrapper().Repopulate();
                c.GetWrapper().Recompose();
            };

            Action<IComponent> pickLocation = (c) =>
            {
                activeLocation = c.Id;
                selectedTile = new Point(-1, -1);
                c.Parent.DeSelectAll(p => true);
                c.Select();
                c.SelectAll(se => se.HasTag("caption"));
                wrapper.Set("LocationName", activeLocation);
                c.GetWrapper().Repopulate();
                c.GetWrapper().Recompose();
            };

            Action<IComponent> selectSpot = (c) =>
            {
                if (c.Params.Length > 1 && int.TryParse(c.Params[0], out int x) && int.TryParse(c.Params[1], out int y))
                {
                    c.Parent.DeSelectAll(m => true);
                    selectedTile = new Point(x, y);
                    wrapper.GetComponentById("WarpInfo").Recompose();
                    c.Select();
                }
            };

            Action<IComponent> closeMenu = (c) =>
             {
                 Game1.activeClickableMenu = null;
             };

            
            bool loaded = false;

            Rectangle tBounds = Rectangle.Empty;
            List<Point> highlights = new List<Point>();
            GameLocation tLocation = null;
            Func<string> getWarpText = () =>
            {
                if (selectedTile.X == -1)
                    return i18n.Get("menu.locations.warp") + " ?";

                return i18n.Get("menu.locations.warp") + $" X{selectedTile.X} Y{selectedTile.Y}";
            };

            Action<IComponent> warpTo = (c) =>
            {
                if (tLocation is GameLocation)
                {
                    if (selectedTile.X == -1)
                        selectedTile = new Point(tLocation.Map.GetLayer("Back").TileWidth / 2, tLocation.Map.GetLayer("Back").TileHeight / 2);
                    Game1.warpFarmer(tLocation.isStructure.Value ? tLocation.uniqueName.Value : tLocation.Name, selectedTile.X, selectedTile.Y, Game1.player.FacingDirection, tLocation.isStructure.Value);
                    Game1.activeClickableMenu = null;
                }
            };

            GameLocation inprocess = null;

            Func<Texture2D> getLocationImage = () =>
            {
                loaded = false;

                bool character = (activeSet == i18n.Get("menu.characters"));
            string locationName = !character ? activeLocation : Game1.locations.FirstOrDefault(l => l.characters.Any(a => a.Name == activeLocation))?.Name;
                highlights.Clear();
            if (character && Game1.locations.SelectMany(l => l.characters.Where(a => a.Name == activeLocation))?.FirstOrDefault() is NPC npc)
                    highlights.Add(npc.getTileLocation() is Vector2 v ? new Point((int)v.X, (int)v.Y) : Point.Zero);
                else if (activeSet == i18n.Get("menu.artifacts") && Game1.getLocationFromName(locationName) is GameLocation aLoc)
                {
                    foreach (var vec in aLoc.Objects.Keys.Where(k => aLoc.Objects[k].ParentSheetIndex == 590))
                        highlights.Add(new Point((int)vec.X, (int)vec.Y));
                }
                else
                    highlights.Clear();

                if (locationShots.ContainsKey(activeLocation) && locationShots[activeLocation] is Texture2D)
                {
                    tBounds = new Rectangle(0, 0, locationShots[activeLocation].Width, locationShots[activeLocation].Height);
                    tLocation = Game1.getLocationFromName(locationName) ?? Game1.getLocationFromName(locationName, true);
                    loaded = true;
                    return locationShots[activeLocation];
                }

                if (!locationShots.ContainsKey(locationName))
                {
                    if (!string.IsNullOrEmpty(locationName) && (Game1.getLocationFromName(locationName) ?? Game1.getLocationFromName(locationName, true)) is GameLocation location)
                    {
                        if (inprocess != location)
                        {
                            inprocess = location;
                            GetLocationShot(location, () =>
                            {
                                Texture2D screen = LastScreen;
                                if (PlatoWarpMenuMod.instance.config.UseTempFolder)
                                    try
                                    {
                                        screen = Helper.Content.Load<Texture2D>(Path.Combine("Temp", (location.isStructure.Value ? location.uniqueName.Value : location.Name) + ".png"));
                                    }
                                    catch { }

                                if (!locationShots.ContainsKey(locationName))
                                    locationShots.Add(locationName, screen);

                                if (!locationShots.ContainsKey(activeLocation))
                                    locationShots.Add(activeLocation, screen);

                                if (screen is Texture2D)
                                {
                                    loaded = true;
                                    tBounds = new Rectangle(0, 0, screen.Width, screen.Height);
                                    tLocation = Game1.getLocationFromName(locationName) ?? Game1.getLocationFromName(locationName, true);
                                    wrapper.GetComponentById("LocationImage").Recompose();
                                }
                                else
                                    inprocess = null;
                            });
                        }
                    }
                    else return locationShots["Placeholder"];
                }

                if (locationShots.ContainsKey(locationName) && locationShots[locationName] is Texture2D)
                {
                    loaded = true;
                    tBounds = new Rectangle(0, 0, locationShots[locationName].Width, locationShots[locationName].Height);
                    tLocation = Game1.getLocationFromName(locationName) ?? Game1.getLocationFromName(locationName, true);

                    return locationShots[locationName];
                }

                return locationShots["Placeholder"];
            };

            int locationsRows = 0;

            Func<IComponent, IEnumerable<IComponent>> leftMenu = (template) => GetSetMenu(sets, activeSet, template);
            Func<IComponent, IEnumerable<IComponent>> locationsMenu = (template) => GetLocationsForSet(activeSet, activeLocation, sets, template, out locationsRows);
            Func<IComponent, IEnumerable<IComponent>> tiles = (template) => GetTiles(tBounds, tLocation, highlights, loaded, template);

            int rowWidth = 125;

            Func<int> getRightX = () => locationsRows == 0 ? 780 : (rowWidth * (locationsRows  + 1)) + 30;
            Func<int> getRightWidth = () => -getRightX();

            wrapper.Set("RightX", getRightX);
            wrapper.Set("RightWidth", getRightWidth);
            wrapper.Set("menufont1", new Font(Helper.GetPlatoHelper(), @"fonts/" + config.MenuFont1 + ".fnt"));
            wrapper.Set("menufont2", new Font(Helper.GetPlatoHelper(), @"fonts/" + config.MenuFont2 + ".fnt"));
            wrapper.Set("LocationName", activeLocation);
            wrapper.Set("PickSet", pickSet);
            wrapper.Set("PickLocation", pickLocation);
            wrapper.Set("SelectSpot", selectSpot);
            wrapper.Set("template.leftMenu", leftMenu);
            wrapper.Set("template.locationsMenu", locationsMenu);
            wrapper.Set("template.tiles", tiles);
            wrapper.Set("LocationImage", getLocationImage);
            wrapper.Set("WarpSpot", getWarpText);
            wrapper.Set("WarpTo", warpTo);
            wrapper.Set("CloseMenu", closeMenu);


            Game1.activeClickableMenu = Helper.GetPlatoHelper().UI.OpenMenu(wrapper);
        }

        public List<string> GetSets()
        {
            List<string> sets = new List<string>();

            sets.Add(i18n.Get("menu.outdoors"));
            sets.Add(i18n.Get("menu.indoors"));
            sets.Add(i18n.Get("menu.buildings"));
            sets.Add(i18n.Get("menu.farm"));

            foreach (GameLocation location in Game1.locations)
                if (location.map.Properties.ContainsKey("Group") && location.map.Properties["Group"].ToString() is string group)
                    sets.Add(group);

            sets.Add(i18n.Get("menu.characters"));
            sets.Add(i18n.Get("menu.artifacts"));

            return sets;
        }

        public IEnumerable<IComponent> GetTiles(Rectangle tBounds, GameLocation tLocation, List<Point> highlights, bool loaded, IComponent template)
        {
            List<IComponent> components = new List<IComponent>();
            if (!loaded)
            {
                Helper.GetPlatoHelper().SetDelayedAction(1, () =>
                {
                    template.Parent.Repopulate();
                    template.Parent.Recompose();
                });
                return components;
            }

            int ph = template.Parent.Bounds.Height;
            int pw = template.Parent.Bounds.Width;
            float fitToHeight = (float)ph / (float)tBounds.Height;
            float fitToWidth = (float)pw / (float)tBounds.Width;
            float scale = Math.Min(Math.Min(fitToHeight, fitToWidth), 4f);
            float w = (tBounds.Width * scale);
            float h = (tBounds.Height * scale);

            int sx = (int)((pw - w) / 2);
            int sy = (int)((ph - h) / 2);
            float tilesize = (Game1.tileSize / 4f) * scale;
            int tilesx = (int)(w / tilesize);
            int tilesy = (int)(h / tilesize);
            for (int tx = 0; tx <= tilesx; tx++)
                for (int ty = 0; ty <= tilesy; ty++)
                {
                    bool clear = tLocation?.isTileLocationTotallyClearAndPlaceableIgnoreFloors(new Vector2(tx, ty)) ?? false;
                    bool high = highlights.Any(p => p.X == tx && p.Y == ty);
                    if (clear || high)
                    {
                        var t = template.Clone(Helper.GetPlatoHelper());
                        t.ParseAttribute("Width", ((int)tilesize).ToString());
                        t.ParseAttribute("Height", ((int)tilesize).ToString());
                        t.ParseAttribute("X", ((int)(sx + tx * tilesize)).ToString());
                        t.ParseAttribute("Y", ((int)(sy + ty * tilesize)).ToString());
                        if (high)
                            t.AddTag("highlight");

                        if (clear)
                            t.Params = new string[] { tx.ToString(), ty.ToString() };
                        components.Add(t);
                    }
                }

            return components;
        }

        public IEnumerable<IComponent> GetSetMenu(List<string> sets, string activeSet, IComponent template)
        {
            int h = 25;
            int margin = 5;

            List<IComponent> menuItems = new List<IComponent>();

            int y = margin * 2;
            foreach (string menu in sets)
            {
                var m = template.Clone(Helper.GetPlatoHelper(), menu);
                if (menu.Equals(activeSet))
                    m.Select();

                if (m.GetComponentsByTag("caption").FirstOrDefault() is TextComponent t)
                {
                    t.ParseAttribute("content", menu);
                    t.Recompose();
                }

                m.ParseAttribute("Y", y.ToString());

                y += (h + margin);

                menuItems.Add(m);
            }

            return menuItems;
        }

        public IEnumerable<IComponent> GetLocationsForSet(string set, string activeLocation, List<string> menuEntries, IComponent template, out int cols)
        {
            int h = 25;
            int w = 120;
            int margin = 5;

            List<string> locations = new List<string>();

            Func<GameLocation, bool> inGroup = (l) => l.map.Properties.ContainsKey("Group");

            if (set == i18n.Get("menu.outdoors"))
                locations = Game1.locations.Where(l => !inGroup(l) && l.IsOutdoors && !(l.IsFarm || l.IsGreenhouse)).Select(g => g.NameOrUniqueName).ToList();
            else if (set == i18n.Get("menu.indoors"))
                locations = Game1.locations.Where(l => !inGroup(l) && !l.IsOutdoors && !(l.IsFarm || l.IsGreenhouse)).Select(g => g.NameOrUniqueName).ToList();
            else if (set == i18n.Get("menu.buildings"))
            {
                locations = Game1.locations.Where(l => l is BuildableGameLocation)
                    .SelectMany(bgl => (bgl as BuildableGameLocation).buildings)
                    .Where(b => b.indoors.Value is GameLocation)
                    .Select(b => b.indoors.Value.NameOrUniqueName)
                    .ToList();
            }
            else if (set == i18n.Get("menu.farm"))
                locations = Game1.locations.Where(l => l.IsFarm || l.IsGreenhouse).Select(g => g.NameOrUniqueName).ToList();
            else if (set == i18n.Get("menu.artifacts"))
                locations = Game1.locations.Where(l => l.Objects.Values.FirstOrDefault(o => o.ParentSheetIndex == 590) is StardewValley.Object).Select(g => g.NameOrUniqueName).ToList();
            else if (set == i18n.Get("menu.farm"))
                locations = Game1.locations.Where(l => l.IsFarm || l.IsGreenhouse).Select(g => g.NameOrUniqueName).ToList();
            else if (set == i18n.Get("menu.characters"))
            {
                locations = new List<string>();
                foreach (var loc in Game1.locations)
                    foreach (var character in loc.characters.Where(v => v.isVillager()))
                        locations.Add(character.Name);
            }
            else
                locations = Game1.locations.Where(l => inGroup(l) && l.map.Properties["Group"].ToString().Equals(set)).Select(g => g.NameOrUniqueName).ToList();

            List<IComponent> menuItems = new List<IComponent>();

            int y = margin * 2;
            int x = margin * 2;
            int i = 0;
            cols = 1;
            foreach (string menu in locations.OrderBy(l => l))
            {
                var m = template.Clone(Helper.GetPlatoHelper(), menu);
                if (m.GetComponentsByTag("caption").FirstOrDefault() is TextComponent t)
                {
                    t.ParseAttribute("content", menu);
                    t.Recompose();
                }

                if (activeLocation == menu)
                {
                    m.Parent.DeSelectAll(se => true);
                    m.Select();
                    m.SelectAll(se => se.HasTag("caption"));
                }

                m.ParseAttribute("Y", y.ToString());
                m.ParseAttribute("X", x.ToString());
                y += (h + margin);
                //x += (w + margin);

                menuItems.Add(m);
                i++;

                if (i * (h+(margin * 2)) >= Game1.viewport.Height) //(i == 5)
                {
                    i = 0;
                    cols++;
                    x += (w + margin);
                    y = margin * 2;
                    //y += (h + margin);
                    //x = margin * 2;
                }

            }

            return menuItems;
        }

        public static void GetLocationShot(GameLocation location, Action callback)
        {
            CurrentLocation = location;
            Callback = callback;

            _helper.GetPlatoHelper().SetTickDelayedUpdateAction(1, () =>
            {
                _helper.Events.Display.RenderedActiveMenu += Display_Rendered;
            });
        }

        public static void Display_Rendered(object sender, RenderedActiveMenuEventArgs e)
        {
            intercept = true;
           var g = Game1.currentLocation;

            var priorColor = Game1.ambientLight;
            bool priorLight = Game1.drawLighting;
            Game1.ambientLight = Microsoft.Xna.Framework.Color.White;
            Game1.drawLighting = false;
            _helper.GetPlatoHelper().SetTickDelayedUpdateAction(2, () => {
                Game1.ambientLight = priorColor;
                Game1.drawLighting = priorLight;
            }
            );

            Game1.currentLocation = CurrentLocation; 
            Game1.currentLocation.resetForPlayerEntry();
            pytk?.EnableMoreMapLayers(Game1.currentLocation.Map);

            try
            {
                try { 
                    Game1.spriteBatch.End();
                }
                catch { }

                if (instance.config.CompatibilityMode || Constants.TargetPlatform == GamePlatform.Android)
                {
                    var fix = new ScreenshotAndroidFix();
                    fix.takeMapScreenshot(Game1.game1, 0.25f, CurrentLocation.isStructure.Value ? CurrentLocation.uniqueName.Value : CurrentLocation.Name);
                }
                else
                {
                    Game1.game1.takeMapScreenshot(0.25f, CurrentLocation.isStructure.Value ? CurrentLocation.uniqueName.Value : CurrentLocation.Name);
                }
                try { 
                    Game1.spriteBatch.Begin();
                }
                catch { }
            }
            catch
            {

            }

            Game1.currentLocation = g;
            _helper.Events.Display.RenderedActiveMenu -= Display_Rendered;
            intercept = false;
            Callback?.Invoke();
        }

        public static bool InterceptScreenshot(object __instance, ref string filename)
        {
            if (!intercept)
                return true;

            if (!Directory.Exists(Path.Combine(_helper.DirectoryPath, "Temp")))
                Directory.CreateDirectory(Path.Combine(_helper.DirectoryPath, "Temp"));

            filename = Path.Combine(_helper.DirectoryPath, "Temp", Path.GetFileName(filename));

            if (instance.config.UseTempFolder)
                return true;

            using (var mem = new MemoryStream())
            {
                var pngFormat = Type.GetType("System.Drawing.Imaging.ImageFormat, System.Drawing").GetProperty("Png", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);
                AccessTools.DeclaredMethod(Type.GetType("System.Drawing.Bitmap, System.Drawing"),"Save",new Type[] { typeof(Stream), Type.GetType("System.Drawing.Imaging.ImageFormat, System.Drawing") }).Invoke(__instance, new object[] { mem, pngFormat });
                LastScreen = Texture2D.FromStream(Game1.graphics.GraphicsDevice, mem);
            }

            return false;
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button != config.MenuButton)
                return;

            if (Context.IsWorldReady)
                if (Game1.activeClickableMenu is IUIMenu pui && pui.Menu.Id == "PlatoWarpMenu")
                    Game1.activeClickableMenu = null;
                else
                    OpenUpMenu();
        }

        private void SetUpConfigMenu()
        {
            if (Constants.TargetPlatform == GamePlatform.Android)
                return;

            if (!Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
                return;

            var api = Helper.ModRegistry.GetApi<PlatoTK.APIs.IGMCM>("spacechase0.GenericModConfigMenu");


            api.RegisterModConfig(ModManifest, () =>
            {
                config.MenuButton = SButton.J;
                config.UseTempFolder = false;
            }, () =>
            {
                Helper.WriteConfig<Config>(config);
            });

            var fonts = new string[] { "opensans", "escrita" };

            api.RegisterLabel(ModManifest, ModManifest.Name, ModManifest.Description);
            api.RegisterSimpleOption(ModManifest, i18n.Get("MenuButton"), "", () => config.MenuButton, (SButton b) => config.MenuButton = b);
            api.RegisterSimpleOption(ModManifest, i18n.Get("UseTempFolder"), "", () => config.UseTempFolder, (bool b) => config.UseTempFolder = b);
            if (LocalizedContentManager.CurrentLanguageLatin)
            {
                api.RegisterChoiceOption(ModManifest, i18n.Get("MenuFont1"), "", () => config.MenuFont1, (string m) => config.MenuFont1 = m == "vanilla" ? "" : m, fonts);
                api.RegisterChoiceOption(ModManifest, i18n.Get("MenuFont2"), "", () => config.MenuFont2, (string m) => config.MenuFont2 = m == "vanilla" ? "" : m, fonts);
            }
        }
    }
}
