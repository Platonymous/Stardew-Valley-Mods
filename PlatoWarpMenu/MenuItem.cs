using PyTK.PlatoUI;
using System.Collections.Generic;

namespace PlatoWarpMenu
{
    public class MenuItem
    {
        public string Id { get; }

        public string Name { get; }

        public List<MenuItem> Children { get; } = new List<MenuItem>();

        public UIElement Screen { get; }

        public bool Special { get; set; } = false;

        public MenuItem(string id, UIElement screen, bool special = false, string name = "")
        {
            Id = id;
            Screen = screen;

            if (name == "")
                name = WarpMenu.i18n.Get(Id);

            Special = special;

            Name = name;
        }
    }
}
