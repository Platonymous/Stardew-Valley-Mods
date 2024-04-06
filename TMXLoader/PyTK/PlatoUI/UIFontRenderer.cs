using BmFont;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System.Collections.Generic;

namespace TMXLoader
{
    public class UIFontRenderer
    {
        public static Dictionary<string, UIFont> Fonts { get; set; } = new Dictionary<string, UIFont>();
 
        public static void LoadFont(UIFont font)
        {
            Fonts.Remove(font.Id);
            Fonts.Add(font.Id, font);
        }

        public static void LoadFont(IModHelper helper, string assetName, string id = "")
        {
            if (id == "")
                id = assetName;

            UIFont font = new UIFont(helper, assetName, id);
            LoadFont(font);
        }

        public static void DrawText(string fontId, SpriteBatch spriteBatch, int x, int y, string text, Color color, float scale, float layerDepth, Vector2 origin)
        {
            int dx = x;
            int dy = y;
            if (Fonts.TryGetValue(fontId, out UIFont current))
            {
                foreach (char c in text)
                {
                    FontChar fc;
                    if (current.CharacterMap.TryGetValue(c, out fc))
                    {
                        var sourceRectangle = new Rectangle(fc.X, fc.Y, fc.Width, fc.Height);
                        var position = new Vector2(dx + fc.XOffset, dy + fc.YOffset);
                        spriteBatch.Draw(
                            texture: current.FontPages[fc.Page],
                            position: position,
                            sourceRectangle: sourceRectangle,
                            color: color,
                            rotation: 0f,
                            scale: scale,
                            effects: SpriteEffects.None,
                            layerDepth: layerDepth,
                            origin: origin) ;
                        dx += (int)(fc.XAdvance * scale);
                    }
                }
            }
        }

        public static Point MeasureString(string fontId, string text, float scale)
        {
            int dx = 0;
            int dy = 0;
            int dh = 0;
            if (Fonts.TryGetValue(fontId, out UIFont current))
            {
                foreach (char c in text)
                    if (current.CharacterMap.TryGetValue(c, out FontChar fc))
                    {
                        dh = (int)(fc.Height * scale);
                        var sourceRectangle = new Rectangle(fc.X, fc.Y, fc.Width, fc.Height);
                        var position = new Vector2(dx + fc.XOffset, dy + fc.YOffset);
                        dx += (int)(fc.XAdvance * scale);
                    }
            }

            return new Point(dx, dh);
        }
    }
}
