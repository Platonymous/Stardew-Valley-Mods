using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMXLoader
{
    public class UIPresets
    {

        public static UIElement GetColorPicker(Color oldColor, Action<Color> updateHandler = null, bool small = false)
        {
            int size = small ? 60 : 100;
            return PlatoUIColorPicker.getColorPickerMenu(oldColor, updateHandler, size);
        }

        public static UIElement GetColorPicker(List<Color> oldColors, Action<int, Color> updateHandler = null, bool small = false)
        {
            int index = 0;

            int size = small ? 60 : 100;

            UIElement container = UIElement.GetContainer("MultiPicker", 0, UIHelper.GetCentered(0, 0, size * 8, size * 4));

            UIElement currentColorPicker = GetColorPicker(oldColors[index], (c) =>
            {
                updateHandler?.Invoke(index, c);
                container.GetElementById("ColorCircle>" + index).Color = c;
            }, small);

            var cpPositioner = UIHelper.GetTopRight(0, 0, currentColorPicker.Bounds.Width, currentColorPicker.Bounds.Height);
            currentColorPicker.Positioner = cpPositioner;
            currentColorPicker.Z = 1;

            Texture2D circle = PyDraw.getCircle((int)(size * 2), Color.White, Color.Transparent);

            for (int i = 0; i < oldColors.Count; i++)
            {
                int y = (-size / 5) + (i * ((int)(size * 1.1f)));
                UIElement colorContainer = UIElement.GetImage(UIHelper.DarkTheme, Color.White * 0.75f, "Color>" + i, 1f, i == index ? 3 : 0, UIHelper.GetTopLeft((int)(((100 - size) * 0.5f)), y, size, size)).WithTypes("ColorPick").AsTiledBox(size / 5, true).WithInteractivity(click: (point, right, release, hold, element) =>
                 {
                     int idx = int.Parse(element.Id.Split('>')[1]);

                     if (index == idx)
                         return;
   
                     else if(!right && release)
                     {
                         foreach (UIElement e in container.GetElementsByType(true, "ColorPick"))
                             e.Z = 0;

                         index = idx;
                         element.Z = 3;

                         currentColorPicker.Color = container.GetElementById("ColorCircle>" + idx).Color;
                         currentColorPicker.GetElementById("CPB_Old").Color = oldColors[idx];

                         container.UpdateBounds();
                     }
                 });
                UIElement colorCircle = UIElement.GetImage(circle, oldColors[i], "ColorCircle>" + i, positioner: UIHelper.GetCentered(0,0,0.4f));
                colorContainer.Add(colorCircle);
                container.Add(colorContainer);
            }
            container.Add(currentColorPicker);

            return container;
        }

        public static UIElement GetCloseButton(Action closingAction)
        {
            (UIHelper.BounceClose as AnimatedTexture2D).Paused = true;
            (UIHelper.BounceClose as AnimatedTexture2D).CurrentFrame = 0;
            (UIHelper.BounceClose as AnimatedTexture2D).SetSpeed(12);

            return UIElement.GetImage(UIHelper.BounceClose, Color.White, "CloseBtn", 1, 9, UIHelper.GetTopRight(20, -40, 40)).WithInteractivity(click: (point, right, released, hold, element) =>
            {
                if (released)
                    closingAction?.Invoke();
            }, hover: (point, hoverin, element) =>
            {
                if (hoverin != element.WasHover)
                    Game1.playSound("smallSelect");

                AnimatedTexture2D a = (element.Theme as AnimatedTexture2D);

                if (hoverin)
                    a.Paused = false;
                else
                {
                    a.Paused = true;
                    a.CurrentFrame = 0;
                }

            });
        }
    }
}
