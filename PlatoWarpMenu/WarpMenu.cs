using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK;
using PyTK.Extensions;
using PyTK.PlatoUI;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace PlatoWarpMenu
{
    internal class WarpMenu : PlatoUIMenu
    {

        internal static IModHelper Helper { get; set; }
        internal static ITranslationHelper i18n => Helper.Translation;
        const float cFontAdjust = 0.5f;

        internal UIElement Menu { get; set; }
        internal UIElement InfoMenu { get; set; }

        internal UIElement InfoBack { get; set; }

        internal UIElement CurrentLocation { get; set; }

        internal static Texture2D CircleMarker { get; set; }

        public static float menuScale = 1f;
        public static int menuWidth => (int)(1260 * menuScale);
        public static int menuHeight => (int) (700 * menuScale);
        public static int margins => (int)(10 * menuScale);
        public static int options = 20;
        public static int optionWidth => menuWidth / 10;

        public static string menuFont1 => PlatoWarpMenuMod.instance?.config?.MenuFont1;
        public static string menuFont2 => PlatoWarpMenuMod.instance?.config?.MenuFont2;
        public static int optionHeight => ((menuHeight - (2 * margins) - options)) / options;

        public static Color? PriorColor = null;
        List<MenuItem> menuItems = new List<MenuItem>();

        public WarpMenu()
            : base("WarpMenu", null, false, null, null, false)
        {
            menuScale = 1f;
            menuScale = Math.Max(1f, Game1.game1.Window.ClientBounds.Width / (menuWidth + 40f));
            menuScale = Math.Min(Game1.game1.Window.ClientBounds.Height / ((menuHeight/menuScale) + 40f), menuScale);
            Console.WriteLine(menuScale + " menuScale");
            menuScale /= Game1.options.zoomLevel;
            Menu = SetUpMenu();
            BaseMenu.Add(Menu);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            if (Game1.activeClickableMenu is WarpMenu)
                Game1.activeClickableMenu = new WarpMenu();
        }

        public static void Open()
        {
            var menu = new WarpMenu();
            Game1.activeClickableMenu = new WarpMenu();
            menu.BaseMenu.UpdateBounds();
        }

        private UIElement SetUpMenu()
        {
            if (CircleMarker == null)
                CircleMarker = PyDraw.getCircle(363, Color.White);
            if (menuFont1 != "" && menuFont1 != null)
                UIFontRenderer.LoadFont(Helper, Path.Combine("fonts", menuFont1 + ".fnt"), menuFont1);

            if (menuFont2 != "" && menuFont2 != null && menuFont2 != menuFont1)
                UIFontRenderer.LoadFont(Helper, Path.Combine("fonts", menuFont2 + ".fnt"), menuFont2);

            UIElement menu = UIElement.GetContainer("WarpMenu", 0, UIHelper.GetCentered(0, 0, menuWidth, menuHeight), 1f);
            UIElement back = UIElement.GetImage(UIHelper.DarkTheme, Color.White * 0.9f, "Back", 1, -1).AsTiledBox(16, true);
            menu.Add(back);

            AddLeftMenu(menu);
            AddInfoMenu(menu);

            return menu;
        }

        private void PopulateMenu()
        {
            AddWarpMenu();
        }

        private void AddInfoMenu(UIElement menu)
        {
            InfoMenu = UIElement.GetContainer("InfoMenu", 0, UIHelper.GetTopRight(margins * -1, margins, (menuWidth / 2) - (2 * margins), menuHeight - (2 * margins)));
            InfoBack = UIElement.GetImage(PyDraw.getBorderedRectangle((menuWidth / 2) - (2 * margins), menuHeight - (2 * margins), Color.White * 0.2f, 1, Color.White), Color.MediumOrchid, positioner:UIHelper.GetCentered());
            InfoMenu.Add(InfoBack);

            menu.Add(InfoMenu);
        }

        private void AddLeftMenu(UIElement menu)
        {
            menuItems.Clear();

            PopulateMenu();

            UIElement leftMenu = UIElement.GetContainer(z: 1, positioner: UIHelper.GetTopLeft(margins, margins, (optionWidth * 2) + 2, menuHeight - (2 * margins)));

            for (int i = 0; i < menuItems.Count; i++)
                if (menuItems[i] is MenuItem item)
                    AddLeftMenuOption(leftMenu, item, i);

            menu.Add(leftMenu);
        }

        private void AddLeftMenuOption(UIElement leftMenu, MenuItem item, int index, int row = 0)
        {
            int maxT = 11;
            int placeRow = row + (index / options);
            string placeText = item.Name;
            if (placeText.Length > maxT + 3)
                placeText = placeText.Substring(0, maxT) + "...";

            index = index % options;
            UIElement button = UIElement.GetImage(PyDraw.getBorderedRectangle(optionWidth, optionHeight - 1, Color.White * 0.2f, 1, Color.White), row == 0 ? Color.CornflowerBlue : Color.LightGreen, item.Id, 1, 0,UIHelper.GetTopLeft(placeRow * (optionWidth + 1), index * (optionHeight + 1), optionWidth, optionHeight - 1)).WithInteractivity(update: UpdateItem, click: ClickItem);
            float textScale = Math.Min(0.5f * menuScale,0.7f);

            if (LocalizedContentManager.CurrentLanguageLatin && menuFont1 != "" && menuFont1 != null)
                textScale *= cFontAdjust;

            UITextElement text = new UITextElement(placeText, Game1.smallFont, Color.White, textScale, 1, positioner: UIHelper.GetCentered());
            
            if (LocalizedContentManager.CurrentLanguageLatin && menuFont1 != "" && menuFont1 != null)
                text.WithFont(menuFont1);

            button.Z = row;
            button.Add(text);
            leftMenu.Add(button);

            if (item.Special)
                PyTK.PyUtils.setDelayedAction(100, () => button.ClickAction(Point.Zero, false, true, false, button));

            if (row > 0)
            {
                button.Disable();
                button.Visible = false;
                text.Visible = false;
            }
            for (int i = 0; i < item.Children.Count; i++)
                if (item.Children[i] is MenuItem child)
                    AddLeftMenuOption(leftMenu, child, i, row + 1);
        }

        private void ClickItem(Point p, bool h, bool r, bool s, UIElement element)
        {
            if (r)
            {
                element.Parent.Children.ForEach(e =>
                {
                    HideElement(e);

                    if (element.Id.StartsWith(e.Id))
                        ShowElement(e, true);
                    else if (e.Id.StartsWith(element.Id) && e.Z == element.Z + 1)
                        ShowElement(e);
                    else if (e.Z <= element.Z && element.Id.Split('.') is string[] elm && e.Id.Split('.') is string[] el)
                    {

                        if (el.Length == 2 && el[0] == elm[0])
                            ShowElement(e);

                        if (el.Length == 3 && el[1] == elm[1])
                            ShowElement(e);

                        if (el.Length == 4 && el[2] == elm[2])
                            ShowElement(e);

                        if (el.Length == 5 && el[3] == elm[3])
                            ShowElement(e);
                    }
                });

                if (FindMenuItem(menuItems,element.Id) is MenuItem item && item.Screen is UIElement screen)
                {
                    InfoMenu.Clear();
                    InfoMenu.Add(InfoBack);
                    InfoMenu.Add(screen);
                    InfoMenu.UpdateBounds();
                }
               
            }
        }

        private MenuItem FindMenuItem(List<MenuItem> items, string id)
        {
            foreach(MenuItem item in items)
            {
                if (item.Id == id)
                    return item;
                else if (FindMenuItem(item.Children, id) is MenuItem cItem)
                    return cItem;
            }

            return null;
        }

        private void HideElement(UIElement element)
        {
            element.Deselect();
            element.Disable();
            element.Visible = false;
            element.Children.ForEach(c => c.Visible = false);
        }

        private void ShowElement(UIElement element, bool selected = false)
        {
            if(selected)
                element.Select();

            element.Enable();
            element.Visible = true;
            element.Children.ForEach(c => ShowElement(c));
        }

        private void UpdateItem(GameTime t, UIElement element)
        {
            if (element.IsSelected)
                element.Color = Color.Orange;
            else
                element.Color = element.Z == 0 ? Color.CornflowerBlue : Color.LightGreen;
        }

        private void AddWarpMenu()
        {
            MenuItem ArtifactsMenuItem = new MenuItem("menu.artifacts", null);
            MenuItem CharacterMenuItem = new MenuItem("menu.characters", null);
            MenuItem OutdoorsMenuItem = new MenuItem("menu.outdoors", null);
            MenuItem IndoorsMenuItem = new MenuItem("menu.indoors", null);
            MenuItem FarmMenuItem = new MenuItem("menu.farm", null);
            MenuItem BuildingsMenuItem = new MenuItem("menu.buildings", null);
            Dictionary<string, List<MenuItem>> OtherMenus = new Dictionary<string, List<MenuItem>>();
            List<GameLocation> locs = Game1.locations.OrderBy(l => l.Name).ToList();

            for (int i = 0; i < locs.Count; i++)
                if (locs[i] is GameLocation location)
                {
                    foreach (var npc in location.getCharacters().Where(n => n.isVillager()))
                        CharacterMenuItem.Children.Add(new MenuItem("menu.characters." + i + "_" + npc.Name + "_", GetMenuForLocation(location, npc), false, npc.Name));

                    if (location.Objects.Values.FirstOrDefault(o => o.ParentSheetIndex == 590) is StardewValley.Object)
                        ArtifactsMenuItem.Children.Add(new MenuItem("menu.artifacts." + i + "_" + location.Name + "_", GetMenuForLocation(location, null, true), false, location.Name));

                    if (location is BuildableGameLocation bgl)
                    {
                        foreach (Building b in bgl.buildings.Where(bu => bu.indoors.Value is GameLocation))
                        {
                            bool current = b.indoors.Value == Game1.currentLocation;
                            BuildingsMenuItem.Children.Add(new MenuItem("menu.buildings." + i + "_" + b.indoors.Value.uniqueName.Value + "_", GetMenuForLocation(b.indoors.Value, null,false,true), current, b.indoors.Value.uniqueName.Value + $"(X{b.humanDoor.X} Y{b.humanDoor.Y})"));
                        }
                    }

                    bool isCurrent = false;

                    if (Game1.currentLocation is GameLocation gl && gl == location)
                        isCurrent = true;

                    if (location.Map.Properties.ContainsKey("Group") && location.Map.Properties["Group"].ToString() is string group)
                    {
                        string name = location.Name;
                        if (location.Map.Properties.ContainsKey("Group") && location.Map.Properties["Name"].ToString() is string n)
                            name = n;

                        var menuItem = new MenuItem("menu.group_" + group + "." + i + "_" + location.Name + "_", GetMenuForLocation(location), isCurrent, name);

                        if (OtherMenus.ContainsKey(group))
                            OtherMenus[group].Add(menuItem);
                        else
                            OtherMenus.Add(group, new List<MenuItem>() { menuItem });
                    }
                    else if ((location.IsFarm || location.IsGreenhouse) && !location.Name.ToLower().StartsWith("buildable") && !location.isStructure.Value)
                        FarmMenuItem.Children.Add(new MenuItem("menu.farm." + i + "_" + location.Name + "_", GetMenuForLocation(location), isCurrent, location.Name));
                    else if (location.IsOutdoors && !location.Name.ToLower().StartsWith("buildable") && !location.isStructure.Value)
                        OutdoorsMenuItem.Children.Add(new MenuItem("menu.outdoors." + i + "_" + location.Name + "_", GetMenuForLocation(location), isCurrent, location.Name));
                    else if (!location.Name.ToLower().StartsWith("buildable") && !location.isStructure.Value)
                        IndoorsMenuItem.Children.Add(new MenuItem("menu.indoors." + i + "_" + location.Name + "_", GetMenuForLocation(location), isCurrent, location.Name));
                    else
                        BuildingsMenuItem.Children.Add(new MenuItem("menu.buildings." + i + "_" + location.Name + "_", GetMenuForLocation(location), isCurrent, location.Name));
                }

            if (OutdoorsMenuItem.Children.Count > 0)
                menuItems.Add(OutdoorsMenuItem);

            if (IndoorsMenuItem.Children.Count > 0)
                menuItems.Add(IndoorsMenuItem);

            if (BuildingsMenuItem.Children.Count > 0)
                menuItems.Add(BuildingsMenuItem);

            if (FarmMenuItem.Children.Count > 0)
                menuItems.Add(FarmMenuItem);
            
            foreach (var g in OtherMenus)
            {
                var mg = new MenuItem("menu.group_" + g.Key, null,false, g.Key);
                foreach (var mi in g.Value)
                    mg.Children.Add(mi);
                mg.Children = mg.Children.OrderBy(c => c.Name).ToList();
                menuItems.Add(mg);
            }

            if (CharacterMenuItem.Children.Count > 0)
            {
                CharacterMenuItem.Children = CharacterMenuItem.Children.OrderBy(c => c.Name).ToList();
                menuItems.Add(CharacterMenuItem);
            }

            if (ArtifactsMenuItem.Children.Count > 0)
            {
                ArtifactsMenuItem.Children = ArtifactsMenuItem.Children.OrderBy(c => c.Name).ToList();
                menuItems.Add(ArtifactsMenuItem);
            }
        }

        private UIElement GetMenuForLocation(GameLocation location, NPC npc = null, bool artifacts = false, bool structure = false)
        {
            var lmenu = UIElement.GetContainer("Location Menu " + location.Name, positioner: UIHelper.GetCentered(0, 0, 1f, 1f));
            float textScale = 1.4f;

            if (LocalizedContentManager.CurrentLanguageLatin && menuFont2 != "" && menuFont2 != null)
                textScale *= cFontAdjust;
            var lname = !structure ? location.Name : location.uniqueName.Value;
            if (i18n.GetTranslations().ToList().Exists(t => t.Key == "location."+lname))
                lname = i18n.Get("location." + lname);

            var lmenuHead = new UITextElement(lname, Game1.dialogueFont, Color.White, textScale, positioner: UIHelper.GetTopCenter(0, 5));


            var back = location.Map.GetLayer("Back");
            
            double w = back.LayerWidth;
            double h = back.LayerHeight;
            double s = (h / w);

            int tw = (menuWidth / 2) - (4 * margins);
            int th = (int)(tw * s);

            if(th > menuHeight * 0.75f)
            {
                s = (w / h);
                th = (int) (menuHeight * 0.75f);
                tw = (int) (th * s);
            }

            var lmenuViewer = UIElement.GetImage(PyDraw.getWhitePixel(), Color.Green, positioner: UIHelper.GetTopCenter(0,50,tw,th));

            var lmenuWarp = new UITextElement($"{i18n.Get("menu.locations.warp")} (X:0 Y:0)", Game1.dialogueFont, Color.White, textScale, positioner: UIHelper.GetTopCenter(0, th + 60));
            var lmenuLoading = new UITextElement(i18n.Get("menu.locations.loading"), Game1.dialogueFont, Color.White, textScale, positioner: UIHelper.GetCentered());
            lmenuViewer.Add(lmenuLoading);

            if (LocalizedContentManager.CurrentLanguageLatin && menuFont2 != "" && menuFont2 != null)
            {
                lmenuHead.WithFont(menuFont2);
                lmenuWarp.WithFont(menuFont2);
                lmenuLoading.WithFont(menuFont2);
            }

            lmenu.Add(lmenuViewer);

            Point tPoint = new Point(0, 0);

            lmenuViewer.WithInteractivity(update: (t, el) =>
            {
                    PlatoWarpMenuMod.GetLocationShot(location, () =>
                    {
                        Texture2D screen = PlatoWarpMenuMod.LastScreen;
                        if (PlatoWarpMenuMod.instance.config.UseTempFolder)
                            screen = Helper.Content.Load<Texture2D>(Path.Combine("Temp", (location.isStructure.Value ? location.uniqueName.Value : location.Name) + ".png"));
                        var image = UIElement.GetImage(screen, Color.White, positioner: el.Positioner);
                        image.WithInteractivity(click: (point, right, released, hold, imageContainer) =>
                         {
                             if (released && !right)
                             {
                                 if (!location.isTileLocationTotallyClearAndPlaceableIgnoreFloors(new Vector2(tPoint.X, tPoint.Y)))
                                     return;
                                 Game1.activeClickableMenu = null;
                                 Game1.player.warpFarmer(new Warp(0, 0, structure ? location.uniqueName.Value : location.Name, tPoint.X, tPoint.Y, false));
                             }
                         }, hover: (point, drag, imageContainer) =>
                           {
                               var rect = imageContainer.Bounds;
                               int x =(int) (((point.X - rect.X) / (double)rect.Width) * location.Map.GetLayer("Back").LayerWidth);
                               int y = (int)(((point.Y - rect.Y) / (double)rect.Height) * location.Map.GetLayer("Back").LayerHeight);
                               tPoint = new Point(x, y);

                               lmenuWarp.Text = $"{i18n.Get("menu.locations.warp")} (X:{ (int)x} Y:{(int)y})";

                               if (!location.isTileLocationTotallyClearAndPlaceableIgnoreFloors(new Vector2(x, y)))
                                   lmenuWarp.TextColor = Color.Red;
                               else
                                   lmenuWarp.TextColor = Color.White;
                           });
                        
                        lmenu.Add(image);
                        lmenu.Remove(lmenuViewer);
                        lmenu.UpdateBounds();

                        if (npc != null)
                        {
                            var rect = image.Bounds;
                            int mx =(int) ((npc.getTileX() / (double) location.Map.GetLayer("Back").LayerWidth) * rect.Width);
                            int my = (int)((npc.getTileY() / (double)location.Map.GetLayer("Back").LayerHeight) * rect.Height);
                            var lmenuMarker = UIElement.GetImage(CircleMarker, Color.Magenta * 0.6f, positioner: UIHelper.GetTopLeft(mx - 12, my - 12, 33, 33));
                            image.Add(lmenuMarker);
                        }

                        if (artifacts)
                        {
                            foreach (var spot in location.Objects.Keys.Where(o => location.Objects[o].ParentSheetIndex == 590))
                            {
                                var rect = image.Bounds;
                                int mx = (int)((spot.X / (double)location.Map.GetLayer("Back").LayerWidth) * rect.Width);
                                int my = (int)((spot.Y / (double)location.Map.GetLayer("Back").LayerHeight) * rect.Height);
                                var lmenuMarker = UIElement.GetImage(CircleMarker, Color.Red, positioner: UIHelper.GetTopLeft(mx - 5, my - 5, 11, 11));
                                image.Add(lmenuMarker);
                            }

                        }
                    });
                el.WithoutInteractivity(true);
            });
            

            lmenu.Add(lmenuHead);
            lmenu.Add(lmenuWarp);
            return lmenu;
        }
    }
}
