using Microsoft.Xna.Framework.Graphics;
using PyTK.Extensions;
using PyTK.Types;
using StardewModdingAPI;
using System.Collections.Generic;

namespace CustomWallsAndFloorsRedux
{
    public class CustomSet
    {
        public Texture2D Walls { get; set; }
        public Texture2D Floors { get; set; }

        public IContentPack Pack { get; set; }

        public Settings Settings { get; set; } = new Settings();

        public string Id => Pack.Manifest.UniqueID;

        public CustomSet(IContentPack pack, bool addToCatalogue = true, bool injectTileSheets = true)
        {
            Walls = pack.HasFile("walls.png") ? pack.ModContent.Load<Texture2D>("walls.png") : null;
            Floors = pack.HasFile("floors.png") ? pack.ModContent.Load<Texture2D>("floors.png") : null;
            
            if (pack.HasFile("settings.json"))
                Settings = pack.ModContent.Load<Settings>("settings.json");
            
            Pack = pack;
            
            if(addToCatalogue)
                AddToCatalogue();

            if (injectTileSheets)
                InjectTileSheets();
        }

        public bool HasWalls => Walls != null;
        public bool HasFloors => Floors != null;

        public IEnumerable<CustomWallpaper> GetWalls()
        {
            int walls = !HasWalls ? 0 : (Walls.Width / 16) * (Walls.Height / 48);
            for (int i = 0; i < walls; i++)
            {
                var wall = new CustomWallpaper(i, this, false);

                if (wall.Animation is Animation anim)
                    i += anim.Frames;

                yield return wall;
            }
        }

        public IEnumerable<CustomWallpaper> GetFloors()
        {
            int floors = !HasFloors ? 0 : (Floors.Width / 32) * (Floors.Height / 32);

            for (int i = 0; i < floors; i++)
            {
                var floor = new CustomWallpaper(i, this, true);

                if (floor.Animation is Animation anim)
                    i += anim.Frames;

                yield return floor;
            }
        }

        public void AddToCatalogue()
        {
            if(HasFloors)
                foreach(var floor in GetFloors())
                    new InventoryItem(floor, 0).addToWallpaperCatalogue();

            if (HasWalls)
                foreach (var wall in GetWalls())
                    new InventoryItem(wall, 0).addToWallpaperCatalogue();
        }

        public void InjectTileSheets()
        {
            if (HasWalls)
                foreach (CustomWallpaper wall in GetWalls())
                    wall.Inject();

            if (HasFloors)
                foreach (CustomWallpaper floor in GetFloors())
                    floor.Inject();
        }
    }
}
