using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK;
using PyTK.Extensions;
using PyTK.PlatoUI;
using PyTK.Types;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Portraiture
{
    public class MenuLoader
    {

        const int listElementHeight = 128;
        const int numPortraits = 7;
        const int listElementWidth = (listElementHeight - margin * 2) * numPortraits + (margin * (numPortraits + 1));

        const int elementsPerPage = 3;
        const int margin = 10;

        public static void OpenMenu(IClickableMenu prioMenu = null)
        {
            PlatoUIMenu menu = null;
            List<UIElement> folders = new List<UIElement>();
            foreach (string folder in TextureLoader.folders)
                folders.AddOrReplace(GetElementForFolder(folder));

            int f = folders.Count;

            Texture2D circle = PyDraw.getCircle(margin * 9, Color.White);

            UIElement container = UIElement.GetContainer("PortraitureMenu", 0, UIHelper.GetCentered(0, 0, listElementWidth + margin * 2, listElementHeight * elementsPerPage + margin * 3), 1);
            UIElement listbox = new UIElement("ListBox", UIHelper.GetCentered(0, 0, listElementWidth + margin * 2, listElementHeight * elementsPerPage + margin * 3), 0, UIHelper.PlainTheme, Color.Black * 0.5f, 1, false);
            UIElementList list = new UIElementList("PortraitFolders", true, 0, 1, 5, 0, true, UIHelper.GetCentered(0, 0, listElementWidth, listElementHeight * elementsPerPage + margin), UIHelper.GetFixed(0, 0, listElementWidth, listElementHeight), folders.ToArray());
            UIElement closeBtn = UIElement.GetImage(circle, Color.White, "CloseBtn", 0.8f, 99, UIHelper.GetAttachedDiagonal(listbox, false, true, margin, -1 * margin, margin * 3, margin * 3)).WithInteractivity(click: (point, right, release, hold, element) =>
              {
                  if (release)
                      menu?.exitThisMenu();
              }, hover: (point, hoverIn, element) =>
             {
                  if (hoverIn != element.WasHover)
                      Game1.playSound("smallSelect");

                  if (hoverIn)
                      element.Opacity = 1f;
                  else
                      element.Opacity = 0.8f;
              });

            closeBtn.Add(UIElement.GetImage(circle, Color.DarkCyan, "CloseBtn_Inner", 1, 0, UIHelper.GetCentered(0, 0, margin, margin)));
            container.Add(closeBtn);
            listbox.Add(list);
            container.Add(listbox);

            UIElement scrollBar = UIElement.GetContainer("ScrollBarContainer", 0, UIHelper.GetAttachedHorizontal(listbox, false, 1, margin, 0, margin * 2, listElementHeight * 2));
            UIElement up = UIElement.GetImage(PyDraw.getFade(margin * 2, margin * 2, Color.White, Color.Gray, false), Color.DarkCyan, "ScrollUp", 1, 0, UIHelper.GetTopCenter());
            UIElement down = UIElement.GetImage(PyDraw.getFade(margin * 2, margin * 2, Color.White, Color.Gray, false), Color.DarkCyan, "ScrollDown", 1, 0, UIHelper.GetBottomCenter());
            UIElement bar = UIElement.GetImage(PyDraw.getFade(margin * 2, listElementHeight * 2 - margin * 4, Color.White, Color.Gray, false), Color.White, "ScrollBar", 1, 0, UIHelper.GetCentered());
            UIElement scrollPoint = UIElement.GetImage(circle, Color.DarkCyan, "ScrollPoint", 1, 0, (t, p) =>
                 {
                     int w = p.Bounds.Width;
                     int h = p.Bounds.Height / (f - 2);
                     int x = 0;
                     int y = list.Position * h;
                     return new Rectangle(p.Bounds.X + x, p.Bounds.Y + y, w, h);
                 });

            if (f > 3)
            {
                bar.Add(scrollPoint.WithBase(list));
                scrollBar.Add(up.WithBase(list).WithInteractivity(click: clickScrollBar, hover: hoverScrollBar));
                scrollBar.Add(down.WithBase(list).WithInteractivity(click: clickScrollBar, hover: hoverScrollBar));
                scrollBar.Add(bar.WithBase(list).WithInteractivity(click: clickScrollBar));
                listbox.Add(scrollBar);
            }
            menu = UIHelper.OpenMenu("PortraitureMenu", container);
            menu.exitFunction = new IClickableMenu.onExit(() =>
            {
                if (prioMenu != null)
                    Game1.activeClickableMenu = prioMenu;
            });
        }

        public static void hoverScrollBar(Point point, bool hoverIn, UIElement element)
        {
            element.Color = hoverIn ? Color.Cyan : Color.DarkCyan;
        }

        public static void clickScrollBar(Point point, bool right, bool release, bool hold, UIElement element)
        {
            if (!right && release)
            {
                bool scrolled = false;
                if (element.Id == "ScrollUp")
                    scrolled = (element.Base as UIElementList).ChangePosition(-1);

                if (element.Id == "ScrollDown")
                    scrolled = (element.Base as UIElementList).ChangePosition(1);

                if (scrolled)
                    Game1.playSound("smallSelect");               
            }
        }

        public static UIElement GetElementForFolder(string folder)
        {

            if (folder == null)
                folder = "Null";

            bool active = TextureLoader.getFolderName() == folder;

            UIElement element = new UIElement(folder, UIHelper.Fill, 0, UIHelper.PlainTheme, Color.White, active ? 1f : 0.75f, false).AsSelectable("Folder", (s, e) =>
             {
                 e.Opacity = s ? 1f : 0.7f;

                 e.GetElementById(e.Id + "_BgName").Color = s ? Color.DarkCyan : Color.Black;

                 if (s)
                     Game1.playSound("coin");

                 if (e.Base != null)
                 {
                     if (s)
                         foreach (UIElement selected in e.Base.GetSelected())
                             if (selected != e)
                                 selected.Deselect();

                     if (!s)
                         if ((new List<UIElement>(e.Base.GetSelected())).Count == 0)
                             e.Select();
                 }

                 if(s)
                     setFolder(e.Id);

             }).WithInteractivity(hover:(point,hoverIn,e) =>
             {
                 if (e.IsSelected)
                     return;

                 if(hoverIn != e.WasHover)
                     Game1.playSound("smallSelect");

                 if (hoverIn)
                     e.Opacity = e.IsSelected ? 1f : 0.9f;
                 else
                     e.Opacity = e.IsSelected ? 1f : 0.75f;

             });

            element.IsSelected = active;
            element.Overflow = true;
            int LastX = 0;
            float i = 0;
            bool scaled = false;

            if(folder == "Vanilla")
            {
                List<NPC> npcs = new List<NPC>();
                for (int c = 0; c < numPortraits; c++)
                {
                    NPC npc = null;
                    while (npc == null || npcs.Contains(npc))
                        npc = Utility.getRandomTownNPC();

                    npcs.Add(npc);

                    Texture2D p = Game1.content.Load<Texture2D>(@"Portraits/" + npc.Name);

                    if (p is Texture2D portrait)
                    {
                        Texture2D t = portrait is ScaledTexture2D st ? st.STexture : portrait;
                        int mx = Math.Max(t.Width / 2, 64);
                        Rectangle s = new Rectangle(0, 0, mx, mx);
                        int w = listElementHeight - margin * 2;
                        int x = LastX + margin;
                        LastX = x + w;
                        i++;

                        UIElement pic = UIElement.GetImage(portrait, Color.White, folder + "_Portrait_" + npc.Name, 1f / (i + 1), 0, UIHelper.GetTopLeft(x, margin, w)).WithSourceRectangle(s);
                        element.Add(pic);
                    }
                }
            }
            else
                foreach (var texture in TextureLoader.pTextures.Where(k => k.Key.StartsWith(folder)))
                    if (texture.Value is Texture2D portrait)
                    {
                        if (i >= numPortraits)
                        {
                            i++;
                            continue;
                        }

                        if (portrait is ScaledTexture2D || scaled)
                            scaled = true;

                        Texture2D t = portrait is ScaledTexture2D st ? st.STexture : portrait;
                        int mx = Math.Max(t.Width / 2, 64);
                        Rectangle s = new Rectangle(0, 0, mx, mx);
                        int w = listElementHeight - margin * 2;
                        int x = LastX + margin;
                        LastX = x + w;
                        i++;

                        UIElement pic = UIElement.GetImage(portrait, Color.White, folder + "_Portrait_" + texture.Key, 1f / (i+1), 0, UIHelper.GetTopLeft(x, margin, w)).WithSourceRectangle(s);

                        element.Add(pic);
                    }

            UITextElement name = new UITextElement(folder, Game1.smallFont, Color.White,0.5f, 1f, folder + "_Name", 2, UIHelper.GetTopLeft(margin, margin));
            UITextElement num = new UITextElement(folder == "Vanilla" ? " " : i.ToString(), Game1.tinyFont, Color.Black,1f, 1f, folder + "_Num", 2, UIHelper.GetBottomRight(-1* margin, -1* margin));
            

            var size = (Game1.smallFont.MeasureString(folder) * 0.5f).toPoint();
            var scaleText = scaled ? "128+" : "64";
            var scaleSize = (Game1.smallFont.MeasureString("XX") * 0.5f).toPoint();
            int sIBSize = Math.Max(scaleSize.X, scaleSize.Y) + margin * 2;
            Point bgSize = new Point(size.X + margin * 4, size.Y + margin * 2);
            Texture2D bgName = PyTK.PyDraw.getFade(bgSize.X * 4, bgSize.Y * 4, Color.White * 0.8f, Color.Transparent);

            UIElement nameBg = UIElement.GetImage(bgName, active ? Color.DarkCyan : Color.Black, folder + "_BgName", 1, 1, UIHelper.GetTopLeft(0, 0, bgSize.X));
            UIElement scaleInfoText = new UITextElement(scaleText, Game1.smallFont, Color.White, 0.5f, 1, folder + "_Scale", 2, UIHelper.GetCentered());
            UIElement scaleInfoBackground = UIElement.GetImage(PyDraw.getCircle((int)(sIBSize * (scaled ? 4 : 1)), Color.White), Color.LightGray,folder + "_ScaleBG",1,1, UIHelper.GetTopRight(-1 * margin, margin, sIBSize));

            scaleInfoBackground.Add(scaleInfoText);
            element.Add(name);
            element.Add(num);
            element.Add(scaleInfoBackground);
            element.Add(nameBg);
            return element;
        }

        private static void setFolder(string folder)
        {
            string current = TextureLoader.getFolderName();
            while (TextureLoader.getFolderName() != folder)
            {
                TextureLoader.nextFolder();
                if (TextureLoader.getFolderName() == current)
                    break;
            }
        }
    }
}
