using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using PyTK.PlatoUI;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.IO;
using xTile.Dimensions;
using xTile.Display;

namespace PlatoUI
{
    public class PlatoUIMod : Mod
    {
        public static IMonitor monitor;
        public static IModHelper helper;
        private static PlatoUIMenu AnimnalMenu;
        Dictionary<string, string> AnimalData;
        Dictionary<string, Texture2D> AnimalTextures;
        public static Texture2D DbTheme;
        public static Texture2D HbTheme;
        public static Texture2D Right;
        internal static int LastRow = 0;

        public override void Entry(IModHelper helper)
        {
            monitor = Monitor;
            PlatoUIMod.helper = helper;
            DbTheme = helper.Content.Load<Texture2D>(@"LooseSprites\DialogBoxGreen", ContentSource.GameContent);
            HbTheme = helper.Content.Load<Texture2D>(@"LooseSprites\hoverbox", ContentSource.GameContent);
            Right = helper.Content.Load<Texture2D>(@"LooseSprites\Cursors", ContentSource.GameContent).getArea(new Microsoft.Xna.Framework.Rectangle(0,192,64,64));


            helper.ConsoleCommands.Add("testui", "", (s, p) =>
              {
                  if (Context.IsWorldReady)
                      Game1.activeClickableMenu = getAnimalMenu();
              });

            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            AnimalData = helper.Content.Load<Dictionary<string, string>>(@"Data\FarmAnimals", ContentSource.GameContent);
            AnimalTextures = new Dictionary<string, Texture2D>();
            foreach (string key in AnimalData.Keys)
            {
                try
                {
                    AnimalTextures.Add(key, helper.Content.Load<Texture2D>(@"Animals/" + key, ContentSource.GameContent));
                    AnimalTextures.Add(key+"_Grey", helper.Content.Load<Texture2D>(@"Animals/" + key, ContentSource.GameContent).clone().setSaturation(0));
                }
                catch
                {
                }
                try
                {
                    AnimalTextures.Add("Baby" + key, helper.Content.Load<Texture2D>(@"Animals/Baby" + key, ContentSource.GameContent));
                }
                catch
                {
                    if (AnimalTextures.ContainsKey(key))
                        AnimalTextures.Add("Baby" + key, AnimalTextures[key]);

                }
            }
        }

        public PlatoUIMenu getAnimalMenu(int row = 0)
        {
            LastRow = row;
            Dictionary<string, List<Building>> Buildings = new Dictionary<string, List<Building>>();

            foreach(var b in Game1.getFarm().buildings)
            {
                if (b.indoors.Value is AnimalHouse ah && !ah.isFull())
                {
                    if (!Buildings.ContainsKey(b.buildingType.Value))
                        Buildings.Add(b.buildingType.Value, new List<Building>());

                    Buildings[b.buildingType.Value].Add(b);
                }
            }

            int boxWidth = 64;
            int maxCols = 4;
            int maxRows = 4;
            int boxHeight = 64;
            int margin = 10;
            //new UIElement("AnimalList", UIHelper.GetCentered(0, 0, boxWidth, boxHeight), 0, DbTheme, Color.White, 1, false).AsTiledBox(32,true);
            UIElement AnimalMenu = new UIElement("Animals", UIHelper.GetCentered(0, 0, boxWidth * 2 * (maxCols + 1), boxHeight * 2 * (maxCols+1)), 0, DbTheme, Color.White, 1, false).AsTiledBox(32,true);
            UIElement AnimalContainer = UIElement.GetContainer("AnimalContainer",1, UIHelper.GetCentered(0, 0, (maxCols * (boxWidth * 2 + margin)) - margin, (maxRows * (boxHeight * 2 + margin)) - margin));
            int i = 0;
            int g = 0;
            int l = AnimalData.Keys.Count;
            bool btn = false;
            UIElement arrow = UIElement.GetImage(Right, Color.White, positioner: UIHelper.GetCentered(0, 0, 100, 100));

            foreach (KeyValuePair<string, string> a in AnimalData)
            {

                if (i < row * maxCols)
                {
                    i++;
                    continue;
                }

                int my = (i - row * maxCols) / maxCols;
                int mx = (i - row * maxCols) % maxCols;

                if (i == ((maxRows + row) * maxCols - (row == 0 ? 1 : 2)) && l >= i - (row == 0 ? 0 : 1))
                {
                    UIElement next = UIElement.GetImage(UIHelper.PlainTheme, Color.White * 0.2f, 1f, "Next", 0, UIHelper.GetTopLeft((int)(mx * boxWidth * 2) + mx * margin, (int)(my * boxHeight * 2) + my * margin, boxWidth * 2, boxHeight * 2)).WithInteractivity(hover: hover, click: (point, right, release, e) => Game1.activeClickableMenu = release ? getAnimalMenu(row + (maxRows)) : Game1.activeClickableMenu);
                    next.Add(arrow);
                    AnimalContainer.Add(next);
                    btn = true;
                    break;
                }

                if (i >= (maxRows + row) * maxCols)
                    break;

                if (!AnimalTextures.ContainsKey(a.Key))
                    continue;

                string[] strArray = a.Value.Split('/');
                int tileW = Convert.ToInt32(strArray[16]);
                int tileH = Convert.ToInt32(strArray[17]);

                AnimatedTexture2D texture = new AnimatedTexture2D(AnimalTextures[a.Key + (Buildings.Exists(b => b.Key.ToLower().Contains(strArray[15].ToLower())) ? "" : "_Grey")].getArea(new Microsoft.Xna.Framework.Rectangle(0, 0, AnimalTextures[a.Key].Width, tileH)), tileW, tileH, 6, true, 1);
                texture.Paused = true;
                int m = Math.Min(texture.Width / 16,2);
                int w = (int)boxWidth;
                int h = (int)((float)(w * m) / (float)texture.Width) * texture.Height;

                UIElement box = new UIElement(a.Key, UIHelper.GetTopLeft((int)(mx * boxWidth * 2) + mx * margin, (int)(my * boxHeight * 2) + my * margin, boxWidth * 2, boxHeight * 2), 0, UIHelper.PlainTheme, Color.White * 0.2f, 1, false).WithInteractivity(hover: hover, click:click);
                UIElement image = UIElement.GetImage(texture, Color.White, 1, a.Key + "_Image", 1, UIHelper.GetCentered(0, 0, w * m, h));
                box.Add(image);
                AnimalContainer.Add(box);
                i++;
            }

            if (row != 0 && !btn)
            {
                int my = (i - row * maxCols) / maxCols;
                int mx = (i - row * maxCols) % maxCols;

                UIElement back = UIElement.GetImage(UIHelper.PlainTheme, Color.White * 0.2f, 1f, "Back", 0, UIHelper.GetTopLeft((int)(mx * boxWidth * 2) + mx * margin, (int)(my * boxHeight * 2) + my * margin, boxWidth * 2, boxHeight * 2)).WithInteractivity(hover: hover, click: (point, right, release, e) => Game1.activeClickableMenu = release ? getAnimalMenu(0) : Game1.activeClickableMenu);
                back.Add(arrow);
                AnimalContainer.Add(back);
            }

            AnimalMenu.Add(AnimalContainer);

            return new PlatoUIMenu("AnimalMenu", AnimalMenu, false);
        }

        private void hover(Point point, bool hoverIn, UIElement element)
        {
            element.Color = Color.White * (hoverIn ? 0.5f : 0.2f);

            if (!element.WasHover && hoverIn)
                Game1.playSound("smallSelect");

            foreach (UIElement child in element.Children)
                if (child.Theme is AnimatedTexture2D an)
                {
                    an.Paused = !hoverIn;
                    if (an.Paused)
                        an.CurrentFrame = 0;
                }
        }

        private void click(Point point, bool right, bool released, UIElement element)
        {
            if (!released)
                return;

            Game1.playSound("money");

            foreach (UIElement child in element.Children)
                if (child.Theme is AnimatedTexture2D an)
                {
                    string[] strArray = AnimalData[element.Id].Split('/');
                    foreach(Building b in Game1.getFarm().buildings)
                        if (b.indoors.Value is AnimalHouse ah && b.buildingType.Value.ToLower().Contains(strArray[15].ToLower()) && !ah.isFull())
                        {
                            var animal = new FarmAnimal(element.Id, helper.Multiplayer.GetNewID(), Game1.player.UniqueMultiplayerID);
                            animal.home = b;
                            animal.homeLocation.Value = new Vector2(b.tileX.Value, b.tileY.Value);
                            ah.animals.Add(animal.myID.Value,animal);
                            ah.animalsThatLiveHere.Add(animal.myID.Value);
                            break;
                        }

                    break;
                }
            Monitor.Log("click");
            Game1.activeClickableMenu = getAnimalMenu(LastRow);
        }


        private void Display_RenderedHud(object sender, StardewModdingAPI.Events.RenderedHudEventArgs e)
        {
            if(Context.IsWorldReady)
                UIHelper.DrawHud(Game1.spriteBatch);
        }

    }
}
