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

        public MenuItem(string id, UIElement screen, string name = "")
        {
            Id = id;
            Screen = screen;

            if (name == "")
                name = WarpMenu.i18n.Get(Id);

            Name = name;
        }
    }
}
