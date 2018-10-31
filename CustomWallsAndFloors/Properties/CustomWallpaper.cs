using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;

namespace CustomWallsAndFloors
{
    public class CustomWallpaper : Wallpaper, ICustomObject
    {
        public static Dictionary<string, Texture2D> Walls = new Dictionary<string, Texture2D>();
        public static Dictionary<string, Texture2D> Floors = new Dictionary<string, Texture2D>();
        private static readonly Rectangle wallpaperContainerRect = new Rectangle(193, 496, 16, 16);
        private static readonly Rectangle floorContainerRect = new Rectangle(209, 496, 16, 16);

        public Texture2D Texture;

        public CustomWallpaper(string id, int which, bool isFloor = false)
            : base(which,isFloor)
        {
            name = id + "." + which;
        }

        public override Item getOne()
        {
            string[] id = name.Split('.');
            return new CustomWallpaper(id[0],int.Parse(id[1]),isFloor);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {
            if (Texture == null)
            {
                string[] id = name.Split('.');
                int which = int.Parse(id[1]);

                Dictionary<string, Texture2D> TextureDict = isFloor ? Floors : Walls;

                if (TextureDict.ContainsKey(id[0]))
                {
                    sourceRect.Value = Game1.getSourceRectForStandardTileSheet(Texture, which, isFloor ? 28 : 16, isFloor ? 26 : 18);
                    Texture = TextureDict[id[0]];
                }
                else
                {
                    sourceRect.Value = isFloor ? new Rectangle(which % 8 * 32, 336 + which / 8 * 32, 28, 26) : new Rectangle(which % 16 * 16, which / 16 * 48 + 8, 16, 28);
                    if (Wallpaper.wallpaperTexture == null)
                        Wallpaper.wallpaperTexture = Game1.content.Load<Texture2D>("Maps\\walls_and_floors");

                    Texture = Wallpaper.wallpaperTexture;
                }
            }

            spriteBatch.Draw(Wallpaper.wallpaperTexture, location + new Vector2(32f, 32f), new Rectangle?(isFloor ? floorContainerRect : wallpaperContainerRect), color * transparency, 0.0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
            spriteBatch.Draw(Texture, location + new Vector2(32f, isFloor ? 30f : 32f), new Rectangle?((Rectangle)((NetFieldBase<Rectangle, NetRectangle>)this.sourceRect)), color * transparency, 0.0f, new Vector2(isFloor ? 14f : 8f, isFloor ? 13f : 14f), 2f * scaleSize, SpriteEffects.None, layerDepth + 1f / 1000f);
        }

        public ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            return new CustomWallpaper(additionalSaveData["id"], int.Parse(additionalSaveData["which"]), (replacement as Wallpaper).isFloor);
        }

        public object getReplacement()
        {
            if (isFloor)
                return new Wallpaper(1, true);
            else
                return new Wallpaper(1, false);
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            return null;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            
        }
    }
}
